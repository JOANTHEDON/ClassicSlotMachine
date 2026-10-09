#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class SlotSceneBuilder {
    [MenuItem("Slot Machine/Build Complete Slot Machine Scene")]
    public static void BuildScene() {
        // 1. Create or Open SlotMachine scene
        string scenePath = "Assets/Scenes/SlotMachine.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Load Sprites
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/bg_gradient.png");
        Sprite machineSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot-machine1.png");
        Sprite leverIdleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot-machine2.png");
        Sprite leverPulledSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot-machine3.png");
        Sprite middleBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot_machine_Middle_box.png");
        Sprite btn2Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot_machine_buttons-02.png");
        Sprite btn3Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot_machine_buttons-03.png");
        Sprite btn4Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Slot Machine/slot_machine_buttons-04.png");

        // Load Symbol Data ScriptableObjects
        SymbolData sevenData = AssetDatabase.LoadAssetAtPath<SymbolData>("Assets/Symbols/Seven.asset");
        SymbolData cherryData = AssetDatabase.LoadAssetAtPath<SymbolData>("Assets/Symbols/Cherry.asset");
        SymbolData bellData = AssetDatabase.LoadAssetAtPath<SymbolData>("Assets/Symbols/Ring.asset");
        SymbolData barData = AssetDatabase.LoadAssetAtPath<SymbolData>("Assets/Symbols/Bar.asset");

        SymbolData[] allSymbols = new SymbolData[] { sevenData, cherryData, bellData, barData };

        // Load TMP Font Asset
        TMP_FontAsset tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // 2. Setup EventSystem (CRITICAL for UI Buttons & Clicks)
        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModuleType != null) {
            eventSystemGo.AddComponent(inputModuleType);
        } else {
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        // 3. Setup Canvas
        GameObject canvasGo = new GameObject("SlotCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Background
        GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvasGo.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgGo.GetComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = Color.white;
        bgImg.raycastTarget = false; // Never block clicks

        // Main Machine Panel (Uses slot-machine2 by default for lever up)
        GameObject machineGo = new GameObject("MachinePanel", typeof(RectTransform), typeof(Image));
        machineGo.transform.SetParent(canvasGo.transform, false);
        RectTransform machineRt = machineGo.GetComponent<RectTransform>();
        machineRt.sizeDelta = new Vector2(1020, 780);
        machineRt.anchoredPosition = new Vector2(0, 40);
        Image machineImg = machineGo.GetComponent<Image>();
        machineImg.sprite = leverIdleSprite != null ? leverIdleSprite : machineSprite;
        machineImg.preserveAspect = true;
        machineImg.raycastTarget = false;

        // Middle Reel Frame Box
        GameObject frameBoxGo = new GameObject("ReelFrameBox", typeof(RectTransform), typeof(Image));
        frameBoxGo.transform.SetParent(machineGo.transform, false);
        RectTransform frameBoxRt = frameBoxGo.GetComponent<RectTransform>();
        frameBoxRt.sizeDelta = new Vector2(560, 310);
        frameBoxRt.anchoredPosition = new Vector2(-28, -25);
        Image frameBoxImg = frameBoxGo.GetComponent<Image>();
        frameBoxImg.sprite = middleBoxSprite;
        frameBoxImg.preserveAspect = true;
        frameBoxImg.raycastTarget = false;

        // Reels Container (with RectMask2D)
        GameObject reelsContainer = new GameObject("ReelsContainer", typeof(RectTransform), typeof(RectMask2D));
        reelsContainer.transform.SetParent(frameBoxGo.transform, false);
        RectTransform reelsRt = reelsContainer.GetComponent<RectTransform>();
        reelsRt.sizeDelta = new Vector2(500, 260);
        reelsRt.anchoredPosition = Vector2.zero;

        // Create 3 Reels
        float[] reelXPositions = new float[] { -155f, 0f, 155f };
        SlotReel[] createdReels = new SlotReel[3];

        for (int r = 0; r < 3; r++) {
            GameObject reelGo = new GameObject($"Reel_{r + 1}", typeof(RectTransform), typeof(SlotReel));
            reelGo.transform.SetParent(reelsContainer.transform, false);
            RectTransform reelRt = reelGo.GetComponent<RectTransform>();
            reelRt.sizeDelta = new Vector2(140, 260);
            reelRt.anchoredPosition = new Vector2(reelXPositions[r], 0);

            SlotReel reelComponent = reelGo.GetComponent<SlotReel>();
            createdReels[r] = reelComponent;

            // Symbol container inside reel
            GameObject symContainerGo = new GameObject("SymbolContainer", typeof(RectTransform));
            symContainerGo.transform.SetParent(reelGo.transform, false);
            RectTransform symContainerRt = symContainerGo.GetComponent<RectTransform>();
            symContainerRt.sizeDelta = new Vector2(140, 260);
            symContainerRt.anchoredPosition = Vector2.zero;

            // 5 symbol slots to allow seamless looping
            float cellHeight = 85f;
            float[] yOffsets = new float[] { 170f, 85f, 0f, -85f, -170f };
            SlotSymbol[] slots = new SlotSymbol[5];

            for (int s = 0; s < 5; s++) {
                GameObject slotGo = new GameObject($"SymbolSlot_{s}", typeof(RectTransform), typeof(Image), typeof(SlotSymbol));
                slotGo.transform.SetParent(symContainerRt, false);
                RectTransform slotRt = slotGo.GetComponent<RectTransform>();
                slotRt.sizeDelta = new Vector2(75, 75);
                slotRt.anchoredPosition = new Vector2(0, yOffsets[s]);

                Image slotImg = slotGo.GetComponent<Image>();
                slotImg.sprite = allSymbols[(s + r) % allSymbols.Length].sprite;
                slotImg.preserveAspect = true;
                slotImg.raycastTarget = false;

                SlotSymbol symComp = slotGo.GetComponent<SlotSymbol>();
                symComp.SetSymbol(allSymbols[(s + r) % allSymbols.Length]);
                slots[s] = symComp;
            }

            // Assign fields via SerializedObject
            SerializedObject so = new SerializedObject(reelComponent);
            so.FindProperty("symbolContainer").objectReferenceValue = symContainerRt;
            so.FindProperty("cellHeight").floatValue = cellHeight;
            so.FindProperty("spinSpeed").floatValue = 2400f;
            SerializedProperty slotsProp = so.FindProperty("symbolSlots");
            slotsProp.arraySize = 5;
            for (int i = 0; i < 5; i++) {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            }
            so.ApplyModifiedProperties();
        }

        // Center Win Line Indicator
        GameObject winLineGo = new GameObject("CenterWinLine", typeof(RectTransform), typeof(Image));
        winLineGo.transform.SetParent(frameBoxGo.transform, false);
        RectTransform winLineRt = winLineGo.GetComponent<RectTransform>();
        winLineRt.sizeDelta = new Vector2(510, 4);
        winLineRt.anchoredPosition = Vector2.zero;
        Image winLineImg = winLineGo.GetComponent<Image>();
        winLineImg.color = new Color(1f, 0.85f, 0.2f, 0.5f);
        winLineImg.raycastTarget = false;

        // Win Highlight Panel (Golden glow border)
        GameObject winHighlightGo = new GameObject("WinHighlightGlow", typeof(RectTransform), typeof(Image));
        winHighlightGo.transform.SetParent(frameBoxGo.transform, false);
        RectTransform winHighlightRt = winHighlightGo.GetComponent<RectTransform>();
        winHighlightRt.sizeDelta = new Vector2(520, 280);
        Image winHighlightImg = winHighlightGo.GetComponent<Image>();
        winHighlightImg.color = new Color(1f, 0.9f, 0.2f, 0.35f);
        winHighlightImg.raycastTarget = false;
        winHighlightGo.SetActive(false);

        // Flash Overlay Image (Jackpot flash)
        GameObject flashGo = new GameObject("ScreenFlashOverlay", typeof(RectTransform), typeof(Image));
        flashGo.transform.SetParent(canvasGo.transform, false);
        RectTransform flashRt = flashGo.GetComponent<RectTransform>();
        flashRt.anchorMin = Vector2.zero;
        flashRt.anchorMax = Vector2.one;
        flashRt.sizeDelta = Vector2.zero;
        Image flashImg = flashGo.GetComponent<Image>();
        flashImg.color = new Color(1f, 0.9f, 0.2f, 0f);
        flashImg.raycastTarget = false;

        // Bottom Controls Bar
        GameObject controlsGo = new GameObject("ControlsBar", typeof(RectTransform));
        controlsGo.transform.SetParent(canvasGo.transform, false);
        RectTransform controlsRt = controlsGo.GetComponent<RectTransform>();
        controlsRt.sizeDelta = new Vector2(1000, 140);
        controlsRt.anchoredPosition = new Vector2(0, -420);

        // Balance Panel
        GameObject balPanel = CreateTmpPanel(controlsGo.transform, "BalancePanel", new Vector2(-360, 0), new Vector2(200, 90), "BALANCE", "1,000", tmpFont);
        TextMeshProUGUI balValueText = balPanel.transform.Find("ValueText").GetComponent<TextMeshProUGUI>();

        // Win Panel
        GameObject winPanel = CreateTmpPanel(controlsGo.transform, "WinPanel", new Vector2(-140, 0), new Vector2(200, 90), "WIN", "0", tmpFont);
        TextMeshProUGUI winValueText = winPanel.transform.Find("ValueText").GetComponent<TextMeshProUGUI>();

        // Bet Panel with +/- Buttons
        GameObject betPanel = CreateTmpPanel(controlsGo.transform, "BetPanel", new Vector2(100, 0), new Vector2(220, 90), "BET", "10", tmpFont);
        TextMeshProUGUI betValueText = betPanel.transform.Find("ValueText").GetComponent<TextMeshProUGUI>();

        // Decrease Bet Button (-)
        Button decBetBtn = CreateTmpButton(betPanel.transform, "DecBetButton", new Vector2(-75, -12), new Vector2(36, 36), "-", 24, tmpFont);
        // Increase Bet Button (+)
        Button incBetBtn = CreateTmpButton(betPanel.transform, "IncBetButton", new Vector2(75, -12), new Vector2(36, 36), "+", 24, tmpFont);

        // Spin Button
        GameObject spinBtnGo = new GameObject("SpinButton", typeof(RectTransform), typeof(Image), typeof(Button));
        spinBtnGo.transform.SetParent(controlsGo.transform, false);
        RectTransform spinRt = spinBtnGo.GetComponent<RectTransform>();
        spinRt.sizeDelta = new Vector2(180, 85);
        spinRt.anchoredPosition = new Vector2(340, 0);
        Image spinImg = spinBtnGo.GetComponent<Image>();
        spinImg.color = new Color(0.95f, 0.25f, 0.2f, 1f); // Vibrant slot spin red
        spinImg.raycastTarget = true;
        Button spinButton = spinBtnGo.GetComponent<Button>();

        // Spin Button Text (TMP)
        GameObject spinTxtGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        spinTxtGo.transform.SetParent(spinBtnGo.transform, false);
        RectTransform spinTxtRt = spinTxtGo.GetComponent<RectTransform>();
        spinTxtRt.sizeDelta = spinRt.sizeDelta;
        TextMeshProUGUI spinText = spinTxtGo.GetComponent<TextMeshProUGUI>();
        spinText.text = "SPIN";
        if (tmpFont != null) spinText.font = tmpFont;
        spinText.fontSize = 34;
        spinText.fontStyle = FontStyles.Bold;
        spinText.alignment = TextAlignmentOptions.Center;
        spinText.color = Color.white;
        spinText.raycastTarget = false;

        // Result Status Text (Above controls)
        GameObject statusGo = new GameObject("ResultStatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusGo.transform.SetParent(canvasGo.transform, false);
        RectTransform statusRt = statusGo.GetComponent<RectTransform>();
        statusRt.sizeDelta = new Vector2(900, 50);
        statusRt.anchoredPosition = new Vector2(0, -325);
        TextMeshProUGUI statusText = statusGo.GetComponent<TextMeshProUGUI>();
        statusText.text = "PRESS SPIN TO PLAY";
        if (tmpFont != null) statusText.font = tmpFont;
        statusText.fontSize = 30;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.yellow;
        statusText.raycastTarget = false;

        // Jackpot Banner (Celebration popup)
        GameObject jackpotGo = new GameObject("JackpotBanner", typeof(RectTransform), typeof(Image));
        jackpotGo.transform.SetParent(canvasGo.transform, false);
        RectTransform jackpotRt = jackpotGo.GetComponent<RectTransform>();
        jackpotRt.sizeDelta = new Vector2(700, 160);
        jackpotRt.anchoredPosition = new Vector2(0, 180);
        Image jackpotImg = jackpotGo.GetComponent<Image>();
        jackpotImg.color = new Color(0.1f, 0.1f, 0.1f, 0.92f);
        jackpotImg.raycastTarget = false;

        GameObject jackpotTxtGo = new GameObject("JackpotText", typeof(RectTransform), typeof(TextMeshProUGUI));
        jackpotTxtGo.transform.SetParent(jackpotGo.transform, false);
        RectTransform jackpotTxtRt = jackpotTxtGo.GetComponent<RectTransform>();
        jackpotTxtRt.sizeDelta = jackpotRt.sizeDelta;
        TextMeshProUGUI jackpotTxt = jackpotTxtGo.GetComponent<TextMeshProUGUI>();
        jackpotTxt.text = "★ JACKPOT! 777 ★";
        if (tmpFont != null) jackpotTxt.font = tmpFont;
        jackpotTxt.fontSize = 48;
        jackpotTxt.fontStyle = FontStyles.Bold;
        jackpotTxt.alignment = TextAlignmentOptions.Center;
        jackpotTxt.color = new Color(1f, 0.85f, 0.1f, 1f);
        jackpotTxt.raycastTarget = false;
        jackpotGo.SetActive(false);

        // Setup Game Systems GameObject
        GameObject systemsGo = new GameObject("--- SLOT SYSTEMS ---");
        PlayerWallet wallet = systemsGo.AddComponent<PlayerWallet>();
        SlotRNG rng = systemsGo.AddComponent<SlotRNG>();
        SlotAudioController audio = systemsGo.AddComponent<SlotAudioController>();
        SlotUIController uiCtrl = systemsGo.AddComponent<SlotUIController>();
        SlotGameManager manager = systemsGo.AddComponent<SlotGameManager>();

        // Wire SlotRNG
        SerializedObject rngSo = new SerializedObject(rng);
        SerializedProperty symProp = rngSo.FindProperty("availableSymbols");
        symProp.arraySize = 4;
        for (int i = 0; i < 4; i++) {
            symProp.GetArrayElementAtIndex(i).objectReferenceValue = allSymbols[i];
        }
        rngSo.ApplyModifiedProperties();

        // Wire SlotUIController
        SerializedObject uiSo = new SerializedObject(uiCtrl);
        uiSo.FindProperty("balanceText").objectReferenceValue = balValueText;
        uiSo.FindProperty("betText").objectReferenceValue = betValueText;
        uiSo.FindProperty("winAmountText").objectReferenceValue = winValueText;
        uiSo.FindProperty("resultStatusText").objectReferenceValue = statusText;
        uiSo.FindProperty("spinButton").objectReferenceValue = spinButton;
        uiSo.FindProperty("increaseBetButton").objectReferenceValue = incBetBtn;
        uiSo.FindProperty("decreaseBetButton").objectReferenceValue = decBetBtn;
        uiSo.FindProperty("winHighlightPanel").objectReferenceValue = winHighlightGo;
        uiSo.FindProperty("jackpotBanner").objectReferenceValue = jackpotGo;
        uiSo.FindProperty("screenFlashImage").objectReferenceValue = flashImg;

        // Wire Lever artwork images
        uiSo.FindProperty("machineImage").objectReferenceValue = machineImg;
        uiSo.FindProperty("leverIdleSprite").objectReferenceValue = leverIdleSprite;
        uiSo.FindProperty("leverPulledSprite").objectReferenceValue = leverPulledSprite;
        uiSo.ApplyModifiedProperties();

        // Wire SlotGameManager
        SerializedObject mgrSo = new SerializedObject(manager);
        SerializedProperty reelsProp = mgrSo.FindProperty("reels");
        reelsProp.arraySize = 3;
        for (int i = 0; i < 3; i++) {
            reelsProp.GetArrayElementAtIndex(i).objectReferenceValue = createdReels[i];
        }
        mgrSo.FindProperty("rng").objectReferenceValue = rng;
        mgrSo.FindProperty("wallet").objectReferenceValue = wallet;
        mgrSo.FindProperty("uiController").objectReferenceValue = uiCtrl;
        mgrSo.FindProperty("audioController").objectReferenceValue = audio;
        mgrSo.ApplyModifiedProperties();

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Successfully created and configured SlotMachine scene with Lever Swap at " + scenePath);

        // Add to Build Settings
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;
    }

    private static GameObject CreateTmpPanel(Transform parent, string name, Vector2 pos, Vector2 size, string label, string defaultValue, TMP_FontAsset font) {
        GameObject panelGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(parent, false);
        RectTransform panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.sizeDelta = size;
        panelRt.anchoredPosition = pos;
        Image panelImg = panelGo.GetComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.16f, 0.85f);
        panelImg.raycastTarget = false;

        // Title Label
        GameObject labelGo = new GameObject("TitleLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(panelGo.transform, false);
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(size.x, 26);
        labelRt.anchoredPosition = new Vector2(0, size.y * 0.25f);
        TextMeshProUGUI labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
        labelTxt.text = label;
        if (font != null) labelTxt.font = font;
        labelTxt.fontSize = 16;
        labelTxt.fontStyle = FontStyles.Bold;
        labelTxt.alignment = TextAlignmentOptions.Center;
        labelTxt.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        labelTxt.raycastTarget = false;

        // Value Text
        GameObject valGo = new GameObject("ValueText", typeof(RectTransform), typeof(TextMeshProUGUI));
        valGo.transform.SetParent(panelGo.transform, false);
        RectTransform valRt = valGo.GetComponent<RectTransform>();
        valRt.sizeDelta = new Vector2(size.x, 38);
        valRt.anchoredPosition = new Vector2(0, -size.y * 0.15f);
        TextMeshProUGUI valTxt = valGo.GetComponent<TextMeshProUGUI>();
        valTxt.text = defaultValue;
        if (font != null) valTxt.font = font;
        valTxt.fontSize = 26;
        valTxt.fontStyle = FontStyles.Bold;
        valTxt.alignment = TextAlignmentOptions.Center;
        valTxt.color = Color.white;
        valTxt.raycastTarget = false;

        return panelGo;
    }

    private static Button CreateTmpButton(Transform parent, string name, Vector2 pos, Vector2 size, string text, int fontSize, TMP_FontAsset font) {
        GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);
        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = size;
        btnRt.anchoredPosition = pos;
        Image btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.35f, 1f);
        btnImg.raycastTarget = true;

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.sizeDelta = size;
        TextMeshProUGUI btnTxt = txtGo.GetComponent<TextMeshProUGUI>();
        btnTxt.text = text;
        if (font != null) btnTxt.font = font;
        btnTxt.fontSize = fontSize;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.white;
        btnTxt.raycastTarget = false;

        return btnGo.GetComponent<Button>();
    }
}
#endif
