using UnityEngine;
using TMPro; // 👈 QUAN TRỌNG

public class PlayerCoinsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText; // 👈 đổi sang TMP
    [SerializeField] private PlayerCoins playerCoins;
    [SerializeField] private string textPrefix = "Coin: ";

    private void Awake()
    {
        if (coinText == null)
        {
            coinText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        ResolvePlayerCoins();

        if (playerCoins != null)
        {
            playerCoins.OnCoinsChanged += HandleCoinsChanged;
            HandleCoinsChanged(playerCoins.Coins);
        }
    }

    private void OnDisable()
    {
        if (playerCoins != null)
        {
            playerCoins.OnCoinsChanged -= HandleCoinsChanged;
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

    private void HandleCoinsChanged(int totalCoins)
    {
        if (coinText == null || playerCoins == null)
        {
            return;
        }

        // 👇 HIỂN THỊ dạng 3/10
        coinText.text = textPrefix + totalCoins + "/" + playerCoins.MaxCoins;
    }
}