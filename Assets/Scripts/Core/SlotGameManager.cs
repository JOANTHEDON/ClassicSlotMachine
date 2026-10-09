using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotGameManager : MonoBehaviour {
    [Header("Core References")]
    [SerializeField] private SlotReel[] reels;
    [SerializeField] private SlotRNG rng;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private SlotUIController uiController;
    [SerializeField] private SlotAudioController audioController;

    [Header("Bet Settings")]
    [SerializeField] private int minBet = 5;
    [SerializeField] private int maxBet = 100;
    [SerializeField] private int betStep = 5;
    [SerializeField] private int currentBet = 10;

    [Header("Spin Timings")]
    [SerializeField] private float baseSpinDuration = 1.2f;
    [SerializeField] private float reelStopDelay = 0.35f;

    private bool isPlaying;

    public int CurrentBet => currentBet;
    public bool IsPlaying => isPlaying;

    private void Start() {
        if (reels != null) {
            for (int i = 0; i < reels.Length; i++) {
                if (reels[i] != null) {
                    reels[i].Initialize(rng);
                }
            }
        }

        if (wallet != null) {
            wallet.OnBalanceChanged += HandleBalanceChanged;
            if (uiController != null) {
                uiController.UpdateBalance(wallet.Coins);
            }
        }

        UpdateBetDisplay();

        if (uiController != null) {
            uiController.SetStatusMessage("Press SPIN to play!", Color.white);
            uiController.ClearWinEffects();
            uiController.UpdateWinAmount(0);
            uiController.SetLeverPulled(false);

            // Runtime button click listener bindings (guarantees buttons work in Play mode)
            if (uiController.SpinButton != null) {
                uiController.SpinButton.onClick.RemoveListener(Spin);
                uiController.SpinButton.onClick.AddListener(Spin);
            }
            if (uiController.IncreaseBetButton != null) {
                uiController.IncreaseBetButton.onClick.RemoveListener(IncreaseBet);
                uiController.IncreaseBetButton.onClick.AddListener(IncreaseBet);
            }
            if (uiController.DecreaseBetButton != null) {
                uiController.DecreaseBetButton.onClick.RemoveListener(DecreaseBet);
                uiController.DecreaseBetButton.onClick.AddListener(DecreaseBet);
            }
            if (uiController.MaxBetButton != null) {
                uiController.MaxBetButton.onClick.RemoveListener(SetMaxBet);
                uiController.MaxBetButton.onClick.AddListener(SetMaxBet);
            }
        }
    }

    private void Update() {
        // Developer shortcut test keys (Editor only or Debug builds)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        HandleDebugInput();
#endif
    }

    private void HandleDebugInput() {
        if (isPlaying || rng == null || rng.AvailableSymbols == null) return;

        // Spacebar -> Normal Spin
        if (Input.GetKeyDown(KeyCode.Space)) {
            Spin();
        }
        // Alpha1 (1 key) -> Force 7-7-7 Jackpot
        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ForceSymbolOutcome("7");
        }
        // Alpha2 (2 key) -> Force BAR-BAR-BAR
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            ForceSymbolOutcome("BAR");
        }
        // Alpha3 (3 key) -> Force Bell-Bell-Bell
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            ForceSymbolOutcome("Bell");
        }
        // Alpha4 (4 key) -> Force Cherry-Cherry-Cherry
        else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            ForceSymbolOutcome("Cherry");
        }
        // Alpha5 (5 key) -> Force Guaranteed Loss
        else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            ForceLosingOutcome();
        }
        // KeypadPlus or UpArrow -> Increase Bet
        else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.UpArrow)) {
            IncreaseBet();
        }
        // KeypadMinus or DownArrow -> Decrease Bet
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.DownArrow)) {
            DecreaseBet();
        }
        // C key -> Free +500 Coins
        else if (Input.GetKeyDown(KeyCode.C)) {
            if (wallet != null) {
                wallet.AddCoins(500);
            }
        }
    }

    private void ForceSymbolOutcome(string symbolId) {
        SymbolData target = null;
        foreach (var sym in rng.AvailableSymbols) {
            if (sym != null && (string.Equals(sym.SymbolId, symbolId, System.StringComparison.OrdinalIgnoreCase) ||
                                sym.name.IndexOf(symbolId, System.StringComparison.OrdinalIgnoreCase) >= 0)) {
                target = sym;
                break;
            }
        }
        if (target != null) {
            ForceSpin(target);
        }
    }

    private void ForceLosingOutcome() {
        if (rng.AvailableSymbols.Length < 2) return;
        if (wallet != null && !wallet.HasEnoughCoins(currentBet)) return;

        SymbolData[] forcedResult = new SymbolData[reels.Length];
        forcedResult[0] = rng.AvailableSymbols[0];
        forcedResult[1] = rng.AvailableSymbols[1 % rng.AvailableSymbols.Length];
        forcedResult[2] = rng.AvailableSymbols[rng.AvailableSymbols.Length > 2 ? 2 : 0];

        StartCoroutine(SpinRoutine(forcedResult));
    }

    private void OnDestroy() {
        if (wallet != null) {
            wallet.OnBalanceChanged -= HandleBalanceChanged;
        }

        if (uiController != null) {
            if (uiController.SpinButton != null) uiController.SpinButton.onClick.RemoveListener(Spin);
            if (uiController.IncreaseBetButton != null) uiController.IncreaseBetButton.onClick.RemoveListener(IncreaseBet);
            if (uiController.DecreaseBetButton != null) uiController.DecreaseBetButton.onClick.RemoveListener(DecreaseBet);
            if (uiController.MaxBetButton != null) uiController.MaxBetButton.onClick.RemoveListener(SetMaxBet);
        }
    }

    private void HandleBalanceChanged(int newBalance) {
        if (uiController != null) {
            uiController.UpdateBalance(newBalance);
        }
    }

    public void IncreaseBet() {
        if (isPlaying) return;
        if (currentBet + betStep <= maxBet) {
            currentBet += betStep;
            UpdateBetDisplay();
            if (audioController != null) audioController.PlayButtonClick();
        }
    }

    public void DecreaseBet() {
        if (isPlaying) return;
        if (currentBet - betStep >= minBet) {
            currentBet -= betStep;
            UpdateBetDisplay();
            if (audioController != null) audioController.PlayButtonClick();
        }
    }

    public void SetMaxBet() {
        if (isPlaying) return;
        currentBet = maxBet;
        UpdateBetDisplay();
        if (audioController != null) audioController.PlayButtonClick();
    }

    private void UpdateBetDisplay() {
        if (uiController != null) {
            uiController.UpdateBet(currentBet);
        }
    }

    public void Spin() {
        if (isPlaying) return;

        // Validation 1: Enough balance
        if (wallet != null && !wallet.HasEnoughCoins(currentBet)) {
            if (uiController != null) {
                uiController.SetStatusMessage("Insufficient coins! Add coins or lower bet.", Color.red);
            }
            return;
        }

        StartCoroutine(SpinRoutine(null));
    }

    public void ForceSpin(SymbolData forcedSymbol) {
        if (isPlaying) return;
        if (wallet != null && !wallet.HasEnoughCoins(currentBet)) return;

        SymbolData[] forcedResult = new SymbolData[reels.Length];
        for (int i = 0; i < reels.Length; i++) {
            forcedResult[i] = forcedSymbol;
        }

        StartCoroutine(SpinRoutine(forcedResult));
    }

    private IEnumerator SpinRoutine(SymbolData[] forcedResult) {
        isPlaying = true;

        if (uiController != null) {
            uiController.SetSpinInteractable(false);
            uiController.ClearWinEffects();
            uiController.UpdateWinAmount(0);
            uiController.SetStatusMessage("Spinning...", Color.yellow);
            // Swap lever to pulled down (slot-machine3)
            uiController.SetLeverPulled(true);
        }

        // Deduct bet once
        if (wallet != null) {
            wallet.TrySpend(currentBet);
        }

        if (audioController != null) {
            audioController.PlayButtonClick();
            audioController.StartSpinLoop();
        }

        // Step 1: Pre-calculate RNG outcome before reels spin
        int reelCount = reels != null ? reels.Length : 3;
        SymbolData[] outcome = forcedResult != null ? forcedResult : rng.GenerateResult(reelCount);

        // Step 2: Spin all reels with staggered stops
        Coroutine[] spinCoroutines = new Coroutine[reelCount];
        for (int i = 0; i < reelCount; i++) {
            float duration = baseSpinDuration + (i * reelStopDelay);
            spinCoroutines[i] = StartCoroutine(reels[i].Spin(outcome[i], duration));
        }

        // Step 3: Wait for each reel to stop sequentially
        for (int i = 0; i < reelCount; i++) {
            while (reels[i].IsSpinning) {
                yield return null;
            }
            if (audioController != null) {
                audioController.PlayReelStop();
            }
        }

        if (audioController != null) {
            audioController.StopSpinLoop();
        }

        // Return lever to idle state (slot-machine2) once all reels have completed spinning
        if (uiController != null) {
            uiController.SetLeverPulled(false);
        }

        // Step 4: Evaluate outcome
        SlotResult result = rng.EvaluateResult(outcome, currentBet);

        // Step 5: Award winnings & trigger visual feedback
        if (result.isWin) {
            if (wallet != null) {
                wallet.AddCoins(result.payout);
            }

            // Highlight matching symbols on reels
            for (int i = 0; i < reelCount; i++) {
                reels[i].SetHighlight(true);
            }

            if (uiController != null) {
                uiController.UpdateWinAmount(result.payout);
                uiController.ShowWinEffects(true, result.isJackpot);

                if (result.isJackpot) {
                    uiController.SetStatusMessage($"*** LUCKY 7 JACKPOT! +{result.payout} COINS! ***", Color.green);
                    if (audioController != null) audioController.PlayJackpot();
                } else {
                    uiController.SetStatusMessage($"WINNER! +{result.payout} COINS!", Color.green);
                    if (audioController != null) audioController.PlayWin();
                }
            }
        } else {
            if (uiController != null) {
                uiController.SetStatusMessage("No win. Try again!", Color.white);
            }
        }

        // Delay before re-enabling spin button
        yield return new WaitForSeconds(0.4f);

        isPlaying = false;
        if (uiController != null) {
            uiController.SetSpinInteractable(true);
        }
    }
}
