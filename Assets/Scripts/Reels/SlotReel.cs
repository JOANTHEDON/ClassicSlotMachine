using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotReel : MonoBehaviour {
    [Header("Reel Configuration")]
    [SerializeField] private RectTransform symbolContainer;
    [SerializeField] private SlotSymbol[] symbolSlots;
    [SerializeField] private float cellHeight = 130f;
    [SerializeField] private float spinSpeed = 2200f;
    [SerializeField] private AnimationCurve stopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isSpinning;
    private SlotRNG rng;
    private SymbolData currentCenterSymbol;

    public bool IsSpinning => isSpinning;
    public SymbolData CenterSymbol => currentCenterSymbol;

    public void Initialize(SlotRNG rngReference) {
        rng = rngReference;
        if (symbolSlots == null || symbolSlots.Length == 0) {
            symbolSlots = GetComponentsInChildren<SlotSymbol>(true);
        }

        // Fill initial symbols
        if (rng != null && rng.AvailableSymbols != null && rng.AvailableSymbols.Length > 0) {
            for (int i = 0; i < symbolSlots.Length; i++) {
                symbolSlots[i].SetSymbol(rng.GetRandomSymbol());
            }
            if (symbolSlots.Length >= 2) {
                currentCenterSymbol = symbolSlots[1].GetSymbol();
            }
        }
    }

    public IEnumerator Spin(SymbolData targetSymbol, float duration) {
        isSpinning = true;
        currentCenterSymbol = targetSymbol;

        // Reset highlights
        SetHighlight(false);

        float elapsed = 0f;
        float totalSpinTime = Mathf.Max(0.5f, duration);
        float accelTime = 0.2f;
        float decelTime = 0.5f;
        float constantTime = Mathf.Max(0.1f, totalSpinTime - accelTime - decelTime);

        // Phase 1: Acceleration & Fast Spin
        float currentSpeed = 0f;
        while (elapsed < accelTime + constantTime) {
            elapsed += Time.deltaTime;
            if (elapsed < accelTime) {
                currentSpeed = Mathf.Lerp(300f, spinSpeed, elapsed / accelTime);
            } else {
                currentSpeed = spinSpeed;
            }

            MoveAndRecycle(currentSpeed * Time.deltaTime);
            yield return null;
        }

        // Phase 2: Deceleration & Snapping to target
        float decelElapsed = 0f;
        while (decelElapsed < decelTime) {
            decelElapsed += Time.deltaTime;
            float t = decelElapsed / decelTime;
            currentSpeed = Mathf.Lerp(spinSpeed, 200f, stopCurve.Evaluate(t));
            MoveAndRecycle(currentSpeed * Time.deltaTime);
            yield return null;
        }

        // Align and display target in center
        SetFinalResult(targetSymbol);

        // Small landing bounce
        yield return StartCoroutine(BounceRoutine());

        isSpinning = false;
    }

    private void MoveAndRecycle(float deltaY) {
        if (symbolSlots == null || symbolSlots.Length == 0) return;

        int count = symbolSlots.Length;
        float topThreshold = (count / 2f) * cellHeight;
        float bottomThreshold = -((count / 2f) * cellHeight);

        for (int i = 0; i < count; i++) {
            RectTransform rt = symbolSlots[i].RectTransform;
            rt.anchoredPosition += Vector2.down * deltaY;

            if (rt.anchoredPosition.y < bottomThreshold) {
                // Recycle to top
                float overshoot = bottomThreshold - rt.anchoredPosition.y;
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, topThreshold - overshoot);

                // Assign a new random symbol during spin
                if (rng != null) {
                    symbolSlots[i].SetSymbol(rng.GetRandomSymbol());
                }
            }
        }
    }

    public void SetFinalResult(SymbolData targetSymbol) {
        currentCenterSymbol = targetSymbol;
        if (symbolSlots == null || symbolSlots.Length < 3) return;

        // Position slots symmetrically around center (y = 0)
        // slot 0: Top (y = +cellHeight)
        // slot 1: Center (y = 0) -> targetSymbol
        // slot 2: Bottom (y = -cellHeight)
        symbolSlots[1].RectTransform.anchoredPosition = Vector2.zero;
        symbolSlots[1].SetSymbol(targetSymbol);

        if (symbolSlots.Length > 0 && rng != null) {
            symbolSlots[0].RectTransform.anchoredPosition = new Vector2(0, cellHeight);
            symbolSlots[0].SetSymbol(rng.GetRandomSymbol());
        }

        if (symbolSlots.Length > 2 && rng != null) {
            symbolSlots[2].RectTransform.anchoredPosition = new Vector2(0, -cellHeight);
            symbolSlots[2].SetSymbol(rng.GetRandomSymbol());
        }
    }

    private IEnumerator BounceRoutine() {
        if (symbolSlots == null || symbolSlots.Length < 2) yield break;

        float bounceDuration = 0.15f;
        float bounceOvershoot = 12f;
        float elapsed = 0f;

        Vector2 basePos = symbolSlots[1].RectTransform.anchoredPosition;

        while (elapsed < bounceDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            float offset = Mathf.Sin(t * Mathf.PI) * bounceOvershoot;
            symbolSlots[1].RectTransform.anchoredPosition = basePos + new Vector2(0, -offset);
            yield return null;
        }

        symbolSlots[1].RectTransform.anchoredPosition = basePos;
    }

    public void SetHighlight(bool enable) {
        if (symbolSlots != null && symbolSlots.Length >= 2) {
            symbolSlots[1].Highlight(enable);
        }
    }
}