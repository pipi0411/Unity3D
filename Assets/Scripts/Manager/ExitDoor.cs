using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ExitDoor : MonoBehaviour
{
    [Header("Coin Requirement")]
    [SerializeField] private PlayerCoins playerCoins;

    [Header("UI Feedback")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string findMoreCoinsMessage = "Not enough coins, keep searching!";
    [SerializeField] private string winMessage = "All coins collected. End game!";
    [SerializeField, Min(0f)] private float messageDuration = 2f;

    [Header("End Game")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool pauseGameOnWin = false;
    [SerializeField] private bool loadSceneOnWin = true;
    [SerializeField] private string winSceneName = "WinGame";

    private bool isGameEnded;
    private Coroutine messageCoroutine;

    private void Awake()
    {
        ResolvePlayerCoins();
    }

    private void Reset()
    {
        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHandleDoorContact(other.GetComponentInParent<PlayerCoins>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHandleDoorContact(collision.collider.GetComponentInParent<PlayerCoins>());
    }

    private void OnDisable()
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }
    }

    private void ResolvePlayerCoins()
    {
        if (playerCoins != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerCoins = playerObject.GetComponent<PlayerCoins>();
            return;
        }

        playerCoins = FindAnyObjectByType<PlayerCoins>();
    }

    private void TryHandleDoorContact(PlayerCoins contactPlayerCoins)
    {
        if (isGameEnded || contactPlayerCoins == null)
        {
            return;
        }

        if (playerCoins == null)
        {
            playerCoins = contactPlayerCoins;
        }

        if (contactPlayerCoins.Coins < contactPlayerCoins.MaxCoins)
        {
            ShowMessage(findMoreCoinsMessage + " (" + contactPlayerCoins.Coins + "/" + contactPlayerCoins.MaxCoins + ")");
            return;
        }

        EndGame();
    }

    private void EndGame()
    {
        isGameEnded = true;
        ShowMessage(winMessage);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (loadSceneOnWin)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!string.IsNullOrWhiteSpace(winSceneName) && Application.CanStreamedLevelBeLoaded(winSceneName))
            {
                SceneManager.LoadScene(winSceneName);
            }

            return;
        }

        if (pauseGameOnWin)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ShowMessage(string message)
    {
        if (statusText == null)
        {
            Debug.Log(message);
            return;
        }

        statusText.text = message;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ClearMessageAfterDelay());
    }

    private System.Collections.IEnumerator ClearMessageAfterDelay()
    {
        if (messageDuration <= 0f)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(messageDuration);

        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        messageCoroutine = null;
    }
}
