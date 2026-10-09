using UnityEngine;

public class SlotRNG : MonoBehaviour {
    [SerializeField] private SymbolData[] availableSymbols;

    public SymbolData GetRandomSymbol() {
        int index = Random.Range(0, availableSymbols.Length);
        return availableSymbols[index];
    }

    public SymbolData[] GenerateResult(int reelCount) {
        SymbolData[] result = new SymbolData[reelCount];

        for (int i = 0; i < reelCount; i++) {
            result[i] = GetRandomSymbol();
        }
        return result;
    }

    public bool IsWinningCombination(SymbolData[] result) {
        if (result == null || result.Length == 0) return false;

        SymbolData firstSymbol = result[0];

        for (int i = 1; i < result.Length; i++) {
            if (result[i] != firstSymbol) return false;
        }

        return true;
    }

    public int CalculatePayout(SymbolData[] result) {
        if (!IsWinningCombination(result)) return 0;

        return result[0].payout;
    }
}
