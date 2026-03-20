using System;
using UnityEngine;

public class PlayerCoins : MonoBehaviour
{
    [Header("Coins")]
    [SerializeField, Min(0)] private int startingCoins;
    [SerializeField, Min(1)] private int maxCoins = 10; // 👈 THÊM

    private int coins;

    public int Coins => coins;
    public int MaxCoins => maxCoins; // 👈 THÊM

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        coins = Mathf.Max(0, startingCoins);
    }

    private void Start()
    {
        // 👇 Auto đếm số coin trong map (không cần set tay)
        maxCoins = FindObjectsByType<CoinRotate>(FindObjectsSortMode.None).Length;
    }

    private void OnEnable()
    {
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }
}