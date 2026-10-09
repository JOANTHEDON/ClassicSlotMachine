using UnityEngine;

[CreateAssetMenu(menuName = "Slot Machine/Symbol")]
public class SymbolData : ScriptableObject {
    public string SymbolId;
    public Sprite sprite;

    [Min(0)]
    public int payout;
}