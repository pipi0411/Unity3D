using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PauseInput : MonoBehaviour
{
    private static readonly List<Behaviour> suppressedSystems = new List<Behaviour>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu PauseScene chưa mở → mở pause
            if (!IsPauseSceneLoaded())
            {
                SuppressSystemsBeforePause();
                Time.timeScale = 0f;  // Tạm dừng game
                SceneManager.LoadSceneAsync("PauseScene", LoadSceneMode.Additive);
            }
        }
    }

    public static void RestoreSystemsAfterPause()
    {
        for (int i = 0; i < suppressedSystems.Count; i++)
        {
            Behaviour item = suppressedSystems[i];
            if (item != null)
            {
                item.enabled = true;
            }
        }

        suppressedSystems.Clear();
    }

    public static void ClearSuppressedSystems()
    {
        suppressedSystems.Clear();
    }

    private static void SuppressSystemsBeforePause()
    {
        suppressedSystems.Clear();

        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        for (int i = 0; i < eventSystems.Length; i++)
        {
            EventSystem current = eventSystems[i];
            if (current != null && current.enabled)
            {
                current.enabled = false;
                suppressedSystems.Add(current);
            }
        }

        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener current = listeners[i];
            if (current != null && current.enabled)
            {
                current.enabled = false;
                suppressedSystems.Add(current);
            }
        }
    }

    private bool IsPauseSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "PauseScene")
            {
                return true;
            }
        }
        return false;
    }
}