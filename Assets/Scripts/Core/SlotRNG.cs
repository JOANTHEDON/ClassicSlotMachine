using System.Collections.Generic;
using UnityEngine;

public class SlotRNG : MonoBehaviour {
    [SerializeField] private SymbolData[] availableSymbols;

    public SymbolData[] AvailableSymbols => availableSymbols;

    public SymbolData GetRandomSymbol() {
        if (availableSymbols == null || availableSymbols.Length == 0) return null;
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

    public SlotResult EvaluateResult(SymbolData[] result, int currentBet) {
        if (result == null || result.Length == 0) {
            return new SlotResult(result, false, 0, false);
        }

        SymbolData firstSymbol = result[0];
        bool isWin = true;

        for (int i = 1; i < result.Length; i++) {
            if (result[i] == null || result[i] != firstSymbol) {
                isWin = false;
                break;
            }
        }

        if (!isWin || firstSymbol == null) {
            return new SlotResult(result, false, 0, false);
        }

        int multiplier = firstSymbol.payoutMultiplier;
        bool isJackpot = string.Equals(firstSymbol.SymbolId, "7", System.StringComparison.OrdinalIgnoreCase) ||
                         firstSymbol.name.ToLower().Contains("seven");

        int totalPayout = multiplier * currentBet;
        return new SlotResult(result, true, totalPayout, isJackpot);
    }

    // Keep legacy helper for compatibility
    public bool IsWinningCombination(SymbolData[] result) {
        if (result == null || result.Length == 0) return false;
        SymbolData first = result[0];
        for (int i = 1; i < result.Length; i++) {
            if (result[i] != first) return false;
        }
        return true;
    }

    public int CalculatePayout(SymbolData[] result) {
        if (!IsWinningCombination(result) || result[0] == null) return 0;
        return result[0].payoutMultiplier;
    }
}
