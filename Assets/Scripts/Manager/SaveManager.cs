using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [Serializable]
    private class SaveData
    {
        public string sceneName;
        public PlayerSaveData player;
        public List<EnemySaveData> enemies = new List<EnemySaveData>();
        public List<string> activePickupIds = new List<string>();
        public List<string> activeCoinIds = new List<string>();
    }

    [Serializable]
    private class PlayerSaveData
    {
        public SerializableVector3 position;
        public float health;
        public int coins;
        public int maxCoins;
        public List<PlayerWeaponController.WeaponSaveEntry> weapons = new List<PlayerWeaponController.WeaponSaveEntry>();
    }

    [Serializable]
    private class EnemySaveData
    {
        public string path;
        public SerializableVector3 position;
        public float health;
        public bool isDead;
    }

    [Serializable]
    private struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    public static SaveManager Instance { get; private set; }

    private SaveData pendingLoadData;

    public static bool HasSaveFile => File.Exists(GetSavePath());

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceOnBoot()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("SaveManager");
        Instance = managerObject.AddComponent<SaveManager>();
        DontDestroyOnLoad(managerObject);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static bool SaveCurrentGame()
    {
        EnsureInstance();
        return Instance.SaveNow();
    }

    public static bool LoadSavedGame()
    {
        EnsureInstance();
        return Instance.LoadNow();
    }

    public static bool DeleteSavedGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);
        return true;
    }

    private bool SaveNow()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning("Save failed: Player not found in current scene.");
            return false;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerCoins playerCoins = player.GetComponent<PlayerCoins>();
        PlayerWeaponController weaponController = player.GetComponent<PlayerWeaponController>();

        SaveData data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;
        data.player = new PlayerSaveData
        {
            position = new SerializableVector3(player.transform.position),
            health = playerHealth != null ? playerHealth.CurrentHealth : 0f,
            coins = playerCoins != null ? playerCoins.Coins : 0,
            maxCoins = playerCoins != null ? playerCoins.MaxCoins : 1,
            weapons = weaponController != null ? weaponController.CreateSaveSnapshot() : new List<PlayerWeaponController.WeaponSaveEntry>()
        };

        SkeletonMovement[] enemies = FindObjectsByType<SkeletonMovement>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            SkeletonMovement enemy = enemies[i];
            data.enemies.Add(new EnemySaveData
            {
                path = GetHierarchyPath(enemy.transform),
                position = new SerializableVector3(enemy.transform.position),
                health = enemy.CurrentHealth,
                isDead = enemy.IsDead
            });
        }

        Item_Pickup[] pickups = FindObjectsByType<Item_Pickup>(FindObjectsSortMode.None);
        for (int i = 0; i < pickups.Length; i++)
        {
            Item_Pickup pickup = pickups[i];
            string id = pickup.GetSaveId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                data.activePickupIds.Add(id);
            }
        }

        CoinRotate[] coins = FindObjectsByType<CoinRotate>(FindObjectsSortMode.None);
        for (int i = 0; i < coins.Length; i++)
        {
            CoinRotate coin = coins[i];
            string id = coin.GetSaveId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                data.activeCoinIds.Add(id);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("Game saved: " + GetSavePath());
        return true;
    }

    private bool LoadNow()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Load failed: Save file not found.");
            return false;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null || string.IsNullOrWhiteSpace(data.sceneName) || data.player == null)
        {
            Debug.LogWarning("Load failed: Save data is invalid.");
            return false;
        }

        pendingLoadData = data;
        SceneManager.sceneLoaded -= OnSceneLoadedApplySave;
        SceneManager.sceneLoaded += OnSceneLoadedApplySave;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(data.sceneName);
        return true;
    }

    private void OnSceneLoadedApplySave(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadData == null)
        {
            return;
        }

        StartCoroutine(ApplyLoadedDataRoutine(pendingLoadData));
        pendingLoadData = null;
        SceneManager.sceneLoaded -= OnSceneLoadedApplySave;
    }

    private System.Collections.IEnumerator ApplyLoadedDataRoutine(SaveData data)
    {
        // Wait one frame so Start/UI subscriptions and auto-initializers complete first.
        yield return null;

        Player player = null;
        for (int i = 0; i < 30 && player == null; i++)
        {
            player = FindAnyObjectByType<Player>();
            if (player == null)
            {
                yield return null;
            }
        }

        ApplyLoadedData(data, player);
    }

    private void ApplyLoadedData(SaveData data, Player player)
    {
        if (player != null)
        {
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            player.transform.position = data.player.position.ToVector3();

            if (characterController != null)
            {
                characterController.enabled = true;
                characterController.Move(Vector3.zero);
            }

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetCurrentHealth(data.player.health);
            }

            PlayerCoins playerCoins = player.GetComponent<PlayerCoins>();
            if (playerCoins != null)
            {
                playerCoins.SetMaxCoins(data.player.maxCoins);
                playerCoins.SetCoins(data.player.coins);
            }

            PlayerWeaponController weaponController = player.GetComponent<PlayerWeaponController>();
            if (weaponController != null)
            {
                weaponController.ApplySaveSnapshot(data.player.weapons);
            }
        }
        else
        {
            Debug.LogWarning("Load warning: Player not found after scene load retries.");
        }

        if (data.activePickupIds != null && data.activePickupIds.Count > 0)
        {
            HashSet<string> activePickupSet = new HashSet<string>(data.activePickupIds);
            Item_Pickup[] pickups = FindObjectsByType<Item_Pickup>(FindObjectsSortMode.None);
            for (int i = 0; i < pickups.Length; i++)
            {
                Item_Pickup pickup = pickups[i];
                string id = pickup.GetSaveId();
                if (!activePickupSet.Contains(id))
                {
                    pickup.RemoveForLoadState();
                }
            }
        }

        if (data.activeCoinIds != null && data.activeCoinIds.Count > 0)
        {
            HashSet<string> activeCoinSet = new HashSet<string>(data.activeCoinIds);
            CoinRotate[] mapCoins = FindObjectsByType<CoinRotate>(FindObjectsSortMode.None);
            for (int i = 0; i < mapCoins.Length; i++)
            {
                CoinRotate coin = mapCoins[i];
                string id = coin.GetSaveId();
                if (!activeCoinSet.Contains(id))
                {
                    coin.RemoveForLoadState();
                }
            }
        }

        SkeletonMovement[] enemies = FindObjectsByType<SkeletonMovement>(FindObjectsSortMode.None);
        Dictionary<string, EnemySaveData> enemyMap = new Dictionary<string, EnemySaveData>();
        for (int i = 0; i < data.enemies.Count; i++)
        {
            EnemySaveData enemyData = data.enemies[i];
            if (!string.IsNullOrWhiteSpace(enemyData.path))
            {
                enemyMap[enemyData.path] = enemyData;
            }
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            SkeletonMovement enemy = enemies[i];
            string path = GetHierarchyPath(enemy.transform);
            if (enemyMap.TryGetValue(path, out EnemySaveData enemyData))
            {
                enemy.ApplySaveData(enemyData.position.ToVector3(), enemyData.health, enemyData.isDead);
            }
            else
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
