using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbolData", menuName = "Slot Machine/Symbol")]
public class SymbolData : ScriptableObject {
    [Tooltip("Unique identifier for this symbol (e.g., 7, Cherry, Bell, BAR)")]
    public string SymbolId;

    [Tooltip("Display name of the symbol")]
    public string displayName;

    [Tooltip("Sprite image displayed on the reels")]
    public Sprite sprite;

    [Tooltip("Payout multiplier for matching 3 symbols across the center row")]
    [Min(0)]
    public int payoutMultiplier = 5;

    // Backward compatibility for existing assets
    public int payout {
        get => payoutMultiplier;
        set => payoutMultiplier = value;
    }
}