using UnityEngine;
using UnityEngine.UI;

public class SlotSymbol : MonoBehaviour {
    [SerializeField] private Image symbolImage;
    [SerializeField] private RectTransform rectTransform;

    private SymbolData symbolData;

    public RectTransform RectTransform {
        get {
            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }

    private void Awake() {
        if (rectTransform == null) {
            rectTransform = GetComponent<RectTransform>();
        }
        if (symbolImage == null) {
            symbolImage = GetComponentInChildren<Image>();
        }
    }

    public void SetSymbol(SymbolData data) {
        symbolData = data;
        if (symbolImage != null && data != null) {
            symbolImage.sprite = data.sprite;
            symbolImage.enabled = true;
        }
    }

    public SymbolData GetSymbol() {
        return symbolData;
    }

    public void Highlight(bool enable) {
        if (symbolImage != null) {
            symbolImage.color = enable ? new Color(1f, 1f, 0.4f, 1f) : Color.white;
        }
    }
}
