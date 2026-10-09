using UnityEngine;

public class SlotReel : MonoBehaviour {
    [SerializeField] private RectTransform symbolContainer;
    [SerializeField] private float spinDuration = 1.5f;
    [SerializeField] private float spinSpeed = 1500f;

    private bool isSpinning;

    public bool IsSpinning => isSpinning;

    public IEnumerator Spin(SymbolData targetSymbol) {
        isSpinning = true;

        float elapsed = 0f;

        while (elapsed < spinDuration) {
            elapsed += Time.deltaTime;

            symbolContainer.anchoredPosition +=
                Vector2.down * spinSpeed * Time.deltaTime;

            // recycle symbols here

            yield return null;
        }

        SetFinalResult(targetSymbol);

        isSpinning = false;
    }
}