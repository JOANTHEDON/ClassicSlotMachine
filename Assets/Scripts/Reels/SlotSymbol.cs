using UnityEngine;
using UnityEngine.UI;
public class SlotSymbol : MonoBehaviour {
    [SerializeField] private Image symbolImage;

    private SymbolData symbolData;

    public void SetSymbol(SymbolData data) {
        symbolData = data;
        symbolImage.sprite = data.sprite;
    }

    public SymbolData GetSymbol() {
        return symbolData;
    }
}
