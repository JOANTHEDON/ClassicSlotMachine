# Classic Slot Machine - Unity 3D / 2D UI

A fully functional, polished, and playable **3-reel classic slot machine game** built with **Unity 6 (6000.3.13f1)** and C#.

---

## 🎮 Game Overview

This game implements a classic casino 3-reel slot machine experience following clean Object-Oriented Programming (OOP) and responsive UI design principles.

### Key Features
- **3-Reel Dynamic Spinning**: Realistic vertical spinning animation with acceleration, high-speed spin with continuous symbol recycling, smooth deceleration curve, and landing bounce effect.
- **Predetermined RNG Outcomes**: Outcomes are strictly generated using a dedicated `SlotRNG` prior to reel animation, ensuring true fair gameplay completely independent of framerate or animation timing.
- **Matching Winning Logic**: Winning occurs when all 3 center-row symbols match across the payline.
- **Dynamic Payout & Betting**:
  - Starting virtual balance: 1,000 coins.
  - Configurable bet size (min: 5, default: 10, max: 100) with `+` and `-` controls.
  - Multipliers applied directly to current bet.
- **Bonus Feature - Lucky Seven Jackpot**:
  - Hitting `7 | 7 | 7` awards the top 20× multiplier along with an animated screen flash, winning row highlight, and celebratory jackpot banner.
- **Edge Case Protection**:
  - Rapid click protection (spins disabled while reels are in motion).
  - Insufficient coin detection prevents negative balances.
  - Zero memory allocation per frame (reusable UI symbol slots).

---

## 🍒 Symbol Mapping & Payout Rules

Center row combinations evaluate to the following payouts:

| Asset File | Symbol | Symbol ID | Payout Multiplier | Example Payout (Bet = 10) |
|---|---|---|:---:|:---:|
| `slot-symbol1.png` | Number 7 | `7` | **20×** | 200 coins *(Jackpot!)* |
| `slot-symbol4.png` | BAR | `BAR` | **10×** | 100 coins |
| `slot-symbol3.png` | Bell | `Bell` | **8×** | 80 coins |
| `slot-symbol2.png` | Cherry | `Cherry` | **5×** | 50 coins |

---

## 🏗️ Architecture & OOP Design

The codebase follows the Single Responsibility Principle:

```
Assets/Scripts/
├── Core/
│   ├── SlotGameManager.cs     # Main game flow, spin lifecycle, betting & payouts
│   ├── SlotRNG.cs             # Independent outcome generator & evaluation logic
│   ├── SlotResult.cs          # Data model representing spin outcome
│   └── PlayerWallet.cs        # Coin balance, debit/credit transactions & events
├── Reels/
│   ├── SlotReel.cs            # Physics-like continuous spin, symbol recycling, bounce
│   └── SlotSymbol.cs          # UI representation and caching for a symbol cell
├── ScriptableObject/
│   └── SymbolData.cs          # Data-driven symbol definitions (ID, Sprite, Payout)
├── UI/
│   └── SlotUIController.cs    # Manages HUD, balance, bets, win banners, flash FX
├── Audio/
│   └── SlotAudioController.cs # Manages audio triggers (safe fallback if clips omitted)
└── Editor/
    └── SlotSceneBuilder.cs    # Automated Unity Editor scene generator and wirer
```

---

## 🚀 How to Run in Unity Editor

1. Open **Unity Hub** and add project from folder:
   ```
   C:\Users\saadn\ClassicSlotMachine
   ```
2. In the top Unity menu, click:
   ```
   Slot Machine -> Build Complete Slot Machine Scene
   ```
   *(This automatically creates and wires `Assets/Scenes/SlotMachine.unity` with all panels, reels, symbols, and buttons).*
3. Press the **Play** button in Unity.
4. Use the **+ / -** buttons to adjust your bet, then click **SPIN**!

---

## 🌐 How to Run WebGL Build

1. Inside Unity, open **File -> Build Settings**.
2. Select **WebGL** as the platform.
3. Ensure `Assets/Scenes/SlotMachine.unity` is checked.
4. Click **Build** and choose `Build/WebGL`.
5. To test locally:
   ```bash
   python -m http.server 8000 --directory Build/WebGL
   ```
   Open `http://localhost:8000` in any modern web browser (Chrome, Edge, Firefox).

---

## 🧪 Verified Test Cases

- [x] **Match 7-7-7**: Triggers Lucky 7 Jackpot, awards 20× payout, activates screen flash and banner.
- [x] **Match BAR-BAR-BAR**: Awards 10× payout and highlights center row.
- [x] **Match Bell-Bell-Bell**: Awards 8× payout.
- [x] **Match Cherry-Cherry-Cherry**: Awards 5× payout.
- [x] **Losing Combination**: Displays "No win. Try again!", awards 0 coins.
- [x] **Insufficient Coins**: Disables spin and notifies player when balance < bet.
- [x] **Rapid Repeated Clicking**: Spin button is immediately locked until reel stop sequence completes.
- [x] **Resolution Independence**: Canvas Scaler set to 1920×1080 (Match Height/Width: 0.5) with responsive anchors.
