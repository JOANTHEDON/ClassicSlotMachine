using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotUIController : MonoBehaviour {
    [Header("Labels & Text Displays (Legacy UI Text or Any Text)")]
    [SerializeField] private Text balanceText;
    [SerializeField] private Text betText;
    [SerializeField] private Text winAmountText;
    [SerializeField] private Text resultStatusText;

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private Button decreaseBetButton;
    [SerializeField] private Button maxBetButton;

    [Header("Win & Jackpot Effects")]
    [SerializeField] private GameObject winHighlightPanel;
    [SerializeField] private GameObject jackpotBanner;
    [SerializeField] private Image screenFlashImage;

    private Coroutine flashRoutine;

    public void UpdateBalance(int balance) {
        if (balanceText != null) {
            balanceText.text = balance.ToString("N0");
        }
    }

    public void UpdateBet(int bet) {
        if (betText != null) {
            betText.text = bet.ToString("N0");
        }
    }

    public void UpdateWinAmount(int winAmount) {
        if (winAmountText != null) {
            winAmountText.text = winAmount > 0 ? winAmount.ToString("N0") : "0";
        }
    }

    public void SetStatusMessage(string message, Color? color = null) {
        if (resultStatusText != null) {
            resultStatusText.text = message;
            if (color.HasValue) {
                resultStatusText.color = color.Value;
            }
        }
    }

    public void SetSpinInteractable(bool interactable) {
        if (spinButton != null) {
            spinButton.interactable = interactable;
        }
        if (increaseBetButton != null) increaseBetButton.interactable = interactable;
        if (decreaseBetButton != null) decreaseBetButton.interactable = interactable;
        if (maxBetButton != null) maxBetButton.interactable = interactable;
    }

    public void ShowWinEffects(bool isWin, bool isJackpot) {
        if (winHighlightPanel != null) {
            winHighlightPanel.SetActive(isWin);
        }

        if (jackpotBanner != null) {
            jackpotBanner.SetActive(isJackpot);
        }

        if (isJackpot && screenFlashImage != null) {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(ScreenFlashRoutine());
        }
    }

    public void ClearWinEffects() {
        if (winHighlightPanel != null) winHighlightPanel.SetActive(false);
        if (jackpotBanner != null) jackpotBanner.SetActive(false);
        if (screenFlashImage != null) {
            Color c = screenFlashImage.color;
            c.a = 0f;
            screenFlashImage.color = c;
        }
    }

    private IEnumerator ScreenFlashRoutine() {
        if (screenFlashImage == null) yield break;

        for (int i = 0; i < 3; i++) {
            float t = 0f;
            while (t < 0.15f) {
                t += Time.deltaTime;
                Color c = screenFlashImage.color;
                c.a = Mathf.Lerp(0f, 0.45f, t / 0.15f);
                screenFlashImage.color = c;
                yield return null;
            }

            t = 0f;
            while (t < 0.2f) {
                t += Time.deltaTime;
                Color c = screenFlashImage.color;
                c.a = Mathf.Lerp(0.45f, 0f, t / 0.2f);
                screenFlashImage.color = c;
                yield return null;
            }
        }
    }
}
