using UnityEngine;

public class PlayerWallet : MonoBehaviour {
    [SerializeField] private int startingCoins = 1000;

    public int Coins { get; private set; }

    private void Awake() {
        Coins = startingCoins;
    }

    public bool TrySpend(int amount) {
        if (Coins < amount)
            return false;

        Coins -= amount;
        return true;
    }

    public void AddCoins(int amount) {
        Coins += amount;
    }
}
