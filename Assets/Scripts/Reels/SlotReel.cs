using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotReel : MonoBehaviour {
    [Header("Reel Configuration")]
    [SerializeField] private RectTransform symbolContainer;
    [SerializeField] private SlotSymbol[] symbolSlots;
    [SerializeField] private float cellHeight = 70f;
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

        // Align slots neatly initially:
        // Slot 0: Top (+cellHeight)
        // Slot 1: Center (0)
        // Slot 2: Bottom (-cellHeight)
        // Remaining slots placed outside
        if (symbolSlots != null && symbolSlots.Length >= 3) {
            for (int i = 0; i < symbolSlots.Length; i++) {
                float y = (1 - i) * cellHeight; // i=0 -> +cellHeight, i=1 -> 0, i=2 -> -cellHeight, i=3 -> -2*cellHeight...
                symbolSlots[i].RectTransform.anchoredPosition = new Vector2(0, y);
                if (rng != null) {
                    symbolSlots[i].SetSymbol(rng.GetRandomSymbol());
                }
            }
            currentCenterSymbol = symbolSlots[1].GetSymbol();
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

        // Align and display target in center slot (Slot 1)
        SetFinalResult(targetSymbol);

        // Landing bounce
        yield return StartCoroutine(BounceRoutine());

        isSpinning = false;
    }

    private void MoveAndRecycle(float deltaY) {
        if (symbolSlots == null || symbolSlots.Length == 0) return;

        int count = symbolSlots.Length;
        float topThreshold = 2f * cellHeight;
        float bottomThreshold = -2f * cellHeight;
        float cycleSpan = count * cellHeight;

        for (int i = 0; i < count; i++) {
            RectTransform rt = symbolSlots[i].RectTransform;
            rt.anchoredPosition += Vector2.down * deltaY;

            if (rt.anchoredPosition.y < bottomThreshold) {
                // Recycle to top
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + cycleSpan);

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

        // Exactly align:
        // Slot 0: Top (+cellHeight)
        // Slot 1: Center (0) -> targetSymbol
        // Slot 2: Bottom (-cellHeight)
        symbolSlots[1].RectTransform.anchoredPosition = Vector2.zero;
        symbolSlots[1].SetSymbol(targetSymbol);

        if (rng != null) {
            symbolSlots[0].RectTransform.anchoredPosition = new Vector2(0, cellHeight);
            symbolSlots[0].SetSymbol(rng.GetRandomSymbol());

            symbolSlots[2].RectTransform.anchoredPosition = new Vector2(0, -cellHeight);
            symbolSlots[2].SetSymbol(rng.GetRandomSymbol());

            // Park any extra recycling slots safely away outside the visible mask
            for (int i = 3; i < symbolSlots.Length; i++) {
                symbolSlots[i].RectTransform.anchoredPosition = new Vector2(0, (i - 1) * cellHeight);
            }
        }
    }

    private IEnumerator BounceRoutine() {
        if (symbolSlots == null || symbolSlots.Length < 3) yield break;

        float bounceDuration = 0.15f;
        float bounceOvershoot = 8f;
        float elapsed = 0f;

        Vector2 pos0 = symbolSlots[0].RectTransform.anchoredPosition;
        Vector2 pos1 = symbolSlots[1].RectTransform.anchoredPosition;
        Vector2 pos2 = symbolSlots[2].RectTransform.anchoredPosition;

        while (elapsed < bounceDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            float offset = Mathf.Sin(t * Mathf.PI) * bounceOvershoot;
            Vector2 downOffset = new Vector2(0, -offset);

            symbolSlots[0].RectTransform.anchoredPosition = pos0 + downOffset;
            symbolSlots[1].RectTransform.anchoredPosition = pos1 + downOffset;
            symbolSlots[2].RectTransform.anchoredPosition = pos2 + downOffset;
            yield return null;
        }

        symbolSlots[0].RectTransform.anchoredPosition = pos0;
        symbolSlots[1].RectTransform.anchoredPosition = pos1;
        symbolSlots[2].RectTransform.anchoredPosition = pos2;
    }

    public void SetHighlight(bool enable) {
        if (symbolSlots != null && symbolSlots.Length >= 2) {
            symbolSlots[1].Highlight(enable);
        }
    }
}