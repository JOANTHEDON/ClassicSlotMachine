using System;
using UnityEngine;

[Serializable]
public class SlotResult {
    public SymbolData[] symbols;
    public bool isWin;
    public int payout;
    public bool isJackpot;

    public SlotResult(SymbolData[] symbols, bool isWin, int payout, bool isJackpot = false) {
        this.symbols = symbols;
        this.isWin = isWin;
        this.payout = payout;
        this.isJackpot = isJackpot;
    }
}
