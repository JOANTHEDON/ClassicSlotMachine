using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour {
    [SerializeField] private int startingCoins = 1000;

    public int Coins { get; private set; }

    public event Action<int> OnBalanceChanged;

    private void Awake() {
        Coins = startingCoins;
    }

    private void Start() {
        OnBalanceChanged?.Invoke(Coins);
    }

    public bool HasEnoughCoins(int amount) {
        return Coins >= amount;
    }

    public bool TrySpend(int amount) {
        if (amount < 0 || Coins < amount)
            return false;

        Coins -= amount;
        OnBalanceChanged?.Invoke(Coins);
        return true;
    }

    public void AddCoins(int amount) {
        if (amount <= 0) return;
        Coins += amount;
        OnBalanceChanged?.Invoke(Coins);
    }

    public void SetCoins(int amount) {
        Coins = Mathf.Max(0, amount);
        OnBalanceChanged?.Invoke(Coins);
    }
}
