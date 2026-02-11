# Throw Dice Game

A Unity-based dice game where players try to reach score goals through strategic dice rolling.

## Game Overview

**Objective:** Win rounds by reaching a score goal within 3 throws.

**Flow:**
1. **Round Start** - Player draws dice into hand, score goal is set
2. **Hand Setup** - Player can discard and redraw dice (1 discard of up to 5 dice)
3. **Throw** - Dice are thrown and settle
4. **Scoring** - Score accumulates; check if goal reached
5. Repeat steps 2-4 up to 3 times per round
6. **Win** - Score goal reached → Shop → Next round (harder goal)
7. **Lose** - 3 throws used without reaching goal → Game Over

## Project Structure

```
Assets/
├── Prefabs/               # Prefab assets
│   ├── Die.prefab             # Die prefab
│   ├── Modifiers/             # Modifier prefabs
│   │   ├── DoubleOnesModifier.prefab
│   │   ├── FlatBonusModifier.prefab
│   │   └── MatchingBonusModifier.prefab
│   ├── Enhancements/          # Enhancement prefabs
│   │   ├── AddValueEnhancement.prefab
│   │   ├── BoostLowestEnhancement.prefab
│   │   └── SynergyEnhancement.prefab
│   └── UI/
│       ├── DiceDisplayItem.prefab  # Hand display item
│       └── ShopItemDisplay.prefab  # Shop item display
├── Data/                  # ScriptableObject assets
│   ├── Dice/                  # DiceData assets
│   ├── Modifiers/             # ModifierData assets
│   └── Enhancements/          # EnhancementData assets
├── Scripts/
│   ├── Core/              # Core systems
│   │   ├── GameEvents.cs      # Central event hub
│   │   ├── ScoreTracker.cs    # Score and goal management
│   │   └── CurrencyManager.cs # Money/currency system
│   ├── Data/              # Data definitions
│   │   └── DiceData.cs        # ScriptableObject for dice types
│   ├── Dice/              # Dice behavior
│   │   ├── Die.cs             # Die MonoBehaviour
│   │   ├── DiceManager.cs     # Dice state tracking
│   │   ├── DiceThrower.cs     # Dice spawning/throwing
│   │   └── Editor/
│   │       └── DiceThrowerEditor.cs
│   ├── GameLoop/          # Game flow management
│   │   ├── GameState.cs       # State enums
│   │   ├── GameManager.cs     # Top-level game control
│   │   ├── RoundManager.cs    # Round phase management
│   │   ├── ShopManager.cs     # Shop with modifiers/enhancements
│   │   ├── Phases/
│   │   │   ├── IRoundPhase.cs     # Phase interface
│   │   │   ├── HandSetupPhase.cs  # Discard/redraw phase
│   │   │   ├── ThrowPhase.cs      # Dice throwing phase
│   │   │   └── ScoringPhase.cs    # Score evaluation phase
│   │   └── Editor/
│   │       ├── GameManagerEditor.cs
│   │       ├── RoundManagerEditor.cs
│   │       └── ShopManagerEditor.cs
│   ├── Inventory/         # Player inventory
│   │   ├── DiceInventory.cs   # Dice collection
│   │   └── Hand.cs            # Current hand of dice
│   ├── Modifiers/         # Score modifier system
│   │   ├── IScoreModifier.cs      # Modifier interface
│   │   ├── ScoreModifierTiming.cs # PerDie vs AfterThrow
│   │   ├── BaseModifier.cs        # MonoBehaviour base class
│   │   ├── ModifierData.cs        # ScriptableObject for shop
│   │   ├── ModifierManager.cs     # Active modifier management
│   │   └── Implementations/
│   │       ├── DoubleOnesModifier.cs   # Doubles 1s
│   │       ├── FlatBonusModifier.cs    # +5 per throw
│   │       └── MatchingBonusModifier.cs # Bonus for pairs/triples
│   ├── Enhancements/      # Dice enhancement system
│   │   ├── IEnhancement.cs        # Enhancement interface
│   │   ├── BaseEnhancement.cs     # MonoBehaviour base class
│   │   ├── EnhancementData.cs     # ScriptableObject for shop
│   │   └── Implementations/
│   │       ├── AddValueEnhancement.cs      # +1 to all faces
│   │       ├── BoostLowestEnhancement.cs   # Raise minimum face
│   │       └── SynergyEnhancement.cs       # Boost 2 dice together
│   └── UI/                # User interface
│       ├── UIManager.cs           # Panel show/hide controller
│       ├── UIPanel.cs             # Base panel class
│       ├── GameStartPanel.cs      # Start screen
│       ├── RoundHUDPanel.cs       # Persistent HUD
│       ├── HandSetupPanel.cs      # Discard/redraw UI
│       ├── DiceDisplayItem.cs     # 3D rotatable die preview
│       ├── DicePreviewRenderer.cs # Manages 3D preview rendering
│       ├── DicePreviewInstance.cs # Individual preview die instance
│       ├── ThrowingPanel.cs       # Rolling status
│       ├── ScoringPanel.cs        # Score result
│       ├── ShopPanel.cs           # Shop with modifiers/enhancements
│       ├── ShopItemDisplay.cs     # Individual shop item UI
│       ├── ActiveModifiersPanel.cs # Shows equipped modifiers
│       ├── CurrencyDisplay.cs     # Shows player money
│       └── GameOverPanel.cs       # Game over screen
├── Scenes/
│   └── SampleScene.unity
└── Settings/              # URP settings
```

## Architecture

### Design Principles (SOLID)

- **Single Responsibility**: Each class has one focused job
- **Open/Closed**: Phase system allows new phases without modifying existing code
- **Liskov Substitution**: All phases implement IRoundPhase interchangeably
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Managers depend on abstractions, use events for loose coupling

### State Flow

```
GameManager (GameState)
├── NotStarted → StartGame()
├── InRound → RoundManager handles phases
│   ├── HandSetup → Throw → Scoring (repeat up to 3x)
│   └── Round ends: Won → InShop, Lost → GameOver
├── InShop → ShopManager (placeholder)
│   └── Continue → InRound (next round)
└── GameOver → RestartGame()
```

### Event System (GameEvents.cs)

Central event hub for loose coupling between systems:
- Game events: `OnGameStarted`, `OnGameOver`, `OnRoundStarted/Completed/Failed`
- Phase events: `OnHandSetupStarted/Completed`, `OnThrowStarted/Completed`, `OnScoringStarted/Completed`
- Shop events: `OnShopEntered`, `OnShopExited`
- Score events: `OnScoreChanged`, `OnScoreGoalSet`

## Core Components

### GameManager (MonoBehaviour)

Top-level controller for game flow.

**Configuration:**
- `_startingDiceCount`: Initial dice (default: 15)
- `_baseScoreGoal`: Round 1 goal (default: 20)
- `_scoreIncreasePerRound`: Goal increase per round (default: 10)
- `_defaultDiceData`: ScriptableObject for standard dice

**Scene Setup:**
1. Create empty GameObject "GameManager"
2. Add GameManager component
3. Assign references to RoundManager and ShopManager
4. Create or assign DiceData asset

### RoundManager (MonoBehaviour)

Manages phases within a single round.

**Configuration:**
- `_maxThrows`: Throws per round (default: 3)
- `_handSize`: Dice drawn per round (default: 5)
- `_discardsPerThrow`: Discard actions per throw (default: 1)
- `_maxDiscardCount`: Max dice per discard (default: 5)

**References:**
- `_diceThrower`: DiceThrower component
- `_diceManager`: DiceManager component

### ShopManager (MonoBehaviour)

Placeholder for shop functionality between rounds.

Currently just provides a Continue button to proceed. Future implementation will include:
- Purchasing new dice
- Selling dice
- Upgrading dice

### ScoreTracker

Tracks score within a round and manages goals.

**Scoring Formula:**
- Round N goal = `baseGoal + (increasePerRound × (N - 1))`
- Default: Round 1 = 20, Round 2 = 30, Round 3 = 40, etc.

### DiceInventory

Manages player's dice collection.

**Key Methods:**
- `Initialize(diceData, count)` - Setup starting inventory
- `AddDice(type, count)` - Add dice to inventory
- `RemoveDice(type, count)` - Remove dice from inventory

### Hand

Current dice selected for a round.

**Key Methods:**
- `DrawToFull()` - Draw from inventory to fill hand
- `Discard(indices)` - Return selected dice to inventory
- `ReturnAllToInventory()` - Return all dice (end of round)

### DiceData (ScriptableObject)

Defines a type of die.

**Create via:** Assets → Create → Dice Game → Dice Data

**Fields:**
- `_displayName`: Display name
- `_description`: Description text
- `_faceValues`: Array of 6 values (+Y, -Y, +X, -X, +Z, -Z)
- `_diePrefab`: Visual prefab for this die type

## Round Phases

### HandSetupPhase

Player can discard dice and redraw from inventory.

**API for UI:**
- `PerformDiscard(List<int> indices)` - Discard selected dice
- `SkipDiscard()` - Skip remaining discards
- `WaitingForInput` - Check if awaiting player input

### ThrowPhase

Dice are thrown and we wait for them to settle.

**Behavior:**
- Auto-executes throw on phase enter
- Monitors DiceManager.RollFinished
- Captures throw score when complete

### ScoringPhase

Evaluates throw and determines next action.

**Results (ScoringResult enum):**
- `GoalReached` - Round won
- `Continue` - More throws available
- `OutOfThrows` - Round lost

## Die System

### Die (MonoBehaviour)

Physics-enabled die with TextMeshPro face labels.

**Prefab Requirements:**
- Cube mesh with Rigidbody and BoxCollider
- 6 child TextMeshPro objects (one per face)
- Die script with sides configured

### DiceManager (MonoBehaviour)

Tracks active dice and detects when all are at rest.

**Events:**
- `OnRollComplete(int total)` - Fired when all dice settle

### DiceThrower (MonoBehaviour)

Spawns and throws dice with physics forces.

## Scene Setup Guide

1. **Create GameManager GameObject:**
   - Add GameManager component
   - Create DiceData asset (Assets → Create → Dice Game → Dice Data)
   - Assign default dice data

2. **Create RoundManager GameObject:**
   - Add RoundManager component
   - Assign to GameManager

3. **Create ShopManager GameObject:**
   - Add ShopManager component
   - Assign to GameManager

4. **Create DiceManager GameObject:**
   - Add DiceManager component
   - Assign to RoundManager

5. **Create DiceThrower GameObject:**
   - Add DiceThrower component
   - Create spawn point child transform
   - Assign references to RoundManager
   - Set Die prefab (create if needed)

6. **Create Die Prefab:**
   - Create cube with Rigidbody
   - Add Die component
   - Add 6 TextMeshPro children for faces
   - Assign TextMeshPro references in Die.sides

## Code Conventions

- Use `[SerializeField]` for inspector-exposed private fields
- Prefer composition over inheritance
- Use events for loose coupling (GameEvents static class)
- Keep MonoBehaviours thin; delegate logic to plain C# classes
- Use State pattern for phase management

## Physics Notes

- Die rest detection uses `Rigidbody.IsSleeping()` with velocity fallback
- Top face determined by dot product with Vector3.up
- Dice receive random rotation, forward force, and random torque for natural tumbling

## UI System

### Architecture

The UI system uses event-driven panels that respond to GameEvents:

```
UIManager
├── GameStartPanel      (shown at launch)
├── RoundHUDPanel       (persistent during rounds)
├── HandSetupPanel      (discard/redraw phase)
├── ThrowingPanel       (while dice rolling)
├── ScoringPanel        (after throw settles)
├── ShopPanel           (between rounds)
└── GameOverPanel       (game over screen)
```

### UI Scripts (Assets/Scripts/UI/)

| Script | Purpose |
|--------|---------|
| `UIManager.cs` | Top-level controller, shows/hides panels based on game events |
| `UIPanel.cs` | Base class for all panels (Show/Hide functionality) |
| `GameStartPanel.cs` | Start game button |
| `RoundHUDPanel.cs` | Persistent HUD (round, score, throws) |
| `HandSetupPanel.cs` | Dice selection for discard/redraw |
| `DiceDisplayItem.cs` | Individual die display in hand |
| `ThrowingPanel.cs` | "Rolling..." status |
| `ScoringPanel.cs` | Throw result and continue button |
| `ShopPanel.cs` | Shop placeholder |
| `GameOverPanel.cs` | Final score and restart button |

## UI Setup Guide

### 1. Create Main Canvas

- [ ] GameObject → UI → Canvas
- [ ] Set Canvas Scaler:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920x1080
  - Match: 0.5

### 2. Create UIManager

- [ ] Create empty GameObject "UIManager" under Canvas
- [ ] Add UIManager component
- [ ] Assign references (after creating panels):
  - Game Manager
  - Round Manager
  - Shop Manager

### 3. Create GameStartPanel

**Hierarchy:**
```
GameStartPanel (UIPanel)
├── Background (Image)
├── Title (TextMeshPro - "Dice Game")
├── Instructions (TextMeshPro)
└── StartButton (Button)
    └── Text (TextMeshPro - "Start Game")
```

**Setup:**
- [ ] Add GameStartPanel component to root
- [ ] Assign UI element references:
  - `_startButton` → StartButton
  - `_titleText` → Title
  - `_instructionsText` → Instructions
- [ ] Assign to UIManager's `_gameStartPanel`

### 4. Create RoundHUDPanel

**Hierarchy:**
```
RoundHUDPanel (UIPanel)
├── TopBar (Horizontal Layout)
│   ├── RoundText (TextMeshPro - "Round 1")
│   ├── ScoreText (TextMeshPro - "Score: 0")
│   ├── GoalText (TextMeshPro - "Goal: 20")
│   ├── ThrowsText (TextMeshPro - "Throw: 1/3")
│   └── InventoryText (TextMeshPro - "Dice: 15")
```

**Setup:**
- [ ] Add RoundHUDPanel component to root
- [ ] Assign all TextMeshPro references
- [ ] Position at top of screen
- [ ] Assign to UIManager's `_roundHUDPanel`

### 5. Create HandSetupPanel

**Hierarchy:**
```
HandSetupPanel (UIPanel)
├── Background (Image, semi-transparent)
├── InstructionText (TextMeshPro)
├── DiscardsRemainingText (TextMeshPro)
├── DiceDisplayContainer (Horizontal Layout Group)
│   └── (DiceDisplayItems spawned here)
├── DiscardButton (Button)
│   └── Text (TextMeshPro - "Discard Selected")
└── ConfirmButton (Button)
    └── Text (TextMeshPro - "Throw Dice")
```

**Setup:**
- [ ] Add HandSetupPanel component to root
- [ ] Assign references:
  - `_instructionText`
  - `_discardsRemainingText`
  - `_diceDisplayContainer`
  - `_confirmButton`
  - `_discardButton`
  - `_confirmButtonText` → ConfirmButton's child Text
- [ ] Assign to UIManager's `_handSetupPanel`

### 6. Create DicePreview Layer and Renderer

**Layer Setup:**
- [ ] Edit → Project Settings → Tags and Layers
- [ ] Add new layer "DicePreview" (e.g., layer 31)

**DicePreviewRenderer Setup:**
- [ ] Create empty GameObject "DicePreviewRenderer" in scene (not under Canvas)
- [ ] Add DicePreviewRenderer component
- [ ] Configure settings:
  - `_renderTextureSize`: 256 (or 512 for higher quality)
  - `_previewLayer`: Select "DicePreview" layer
  - `_backgroundColor`: Transparent or dark color
  - `_cameraDistance`: 2
  - `_fieldOfView`: 30

### 7. Create DiceDisplayItem Prefab

**Hierarchy:**
```
DiceDisplayItem (Prefab)
├── Background (Image)
├── SelectionBorder (Image - outline/border)
├── PreviewImage (RawImage - displays 3D die)
└── NameText (TextMeshPro - die name)
```

**Setup:**
- [ ] Create UI element with above hierarchy
- [ ] Add DiceDisplayItem component to root
- [ ] Assign references:
  - `_previewImage` → PreviewImage (RawImage)
  - `_backgroundImage` → Background
  - `_selectionBorder` → SelectionBorder
  - `_nameText` → NameText
- [ ] Configure rotation settings:
  - `_rotationSensitivity`: 0.5
  - `_autoRotateSpeed`: 15
  - `_autoRotateWhenIdle`: true
  - `_idleTimeBeforeAutoRotate`: 2
- [ ] Set colors: `_normalColor`, `_selectedColor`, `_borderNormalColor`, `_borderSelectedColor`
- [ ] Save as prefab in Assets/Prefabs/UI/
- [ ] Assign prefab to HandSetupPanel's `_diceDisplayPrefab`

**Note:** The DiceDisplayItem creates a 3D preview of the actual die model. Users can click and drag to rotate the die and view all faces. After 2 seconds of no interaction, the die auto-rotates slowly.

### 7. Create ThrowingPanel

**Hierarchy:**
```
ThrowingPanel (UIPanel)
├── Background (Image, semi-transparent)
└── StatusText (TextMeshPro - "Rolling...")
```

**Setup:**
- [ ] Add ThrowingPanel component to root
- [ ] Assign `_statusText`
- [ ] Center on screen
- [ ] Assign to UIManager's `_throwingPanel`

### 8. Create ScoringPanel

**Hierarchy:**
```
ScoringPanel (UIPanel)
├── Background (Image)
├── ResultText (TextMeshPro - "Keep Going!")
├── ScoreText (TextMeshPro - "Score: 15 / 20")
├── MessageText (TextMeshPro - "Need 5 more points")
└── ContinueButton (Button)
    └── Text (TextMeshPro - "Next Throw")
```

**Setup:**
- [ ] Add ScoringPanel component to root
- [ ] Assign all references:
  - `_resultText`
  - `_scoreText`
  - `_messageText`
  - `_continueButton`
  - `_continueButtonText`
- [ ] Assign to UIManager's `_scoringPanel`

### 9. Create ShopPanel

**Hierarchy:**
```
ShopPanel (UIPanel)
├── Background (Image)
├── TitleText (TextMeshPro - "SHOP")
├── InventoryText (TextMeshPro)
├── PlaceholderText (TextMeshPro - "Coming soon...")
└── ContinueButton (Button)
    └── Text (TextMeshPro - "Continue")
```

**Setup:**
- [ ] Add ShopPanel component to root
- [ ] Assign all references
- [ ] Assign to UIManager's `_shopPanel`

### 10. Create GameOverPanel

**Hierarchy:**
```
GameOverPanel (UIPanel)
├── Background (Image)
├── TitleText (TextMeshPro - "GAME OVER")
├── RoundReachedText (TextMeshPro - "Reached Round X")
├── FinalScoreText (TextMeshPro - "Final Score: X / Y")
└── RestartButton (Button)
    └── Text (TextMeshPro - "Play Again")
```

**Setup:**
- [ ] Add GameOverPanel component to root
- [ ] Assign all references
- [ ] Assign to UIManager's `_gameOverPanel`

### 11. Final UIManager Setup

- [ ] Assign all panel references to UIManager:
  - `_gameStartPanel`
  - `_roundHUDPanel`
  - `_handSetupPanel`
  - `_throwingPanel`
  - `_scoringPanel`
  - `_shopPanel`
  - `_gameOverPanel`
- [ ] Assign manager references:
  - `_gameManager`
  - `_roundManager`
  - `_shopManager`

### Reference Assignment Summary

| Component | Needs References To |
|-----------|---------------------|
| UIManager | All 7 panels, GameManager, RoundManager, ShopManager |
| GameStartPanel | Start Button |
| RoundHUDPanel | Round/Score/Goal/Throws/Inventory Text elements |
| HandSetupPanel | Instruction Text, Discards Text, Container, Buttons, DiceDisplayItem Prefab |
| DiceDisplayItem | PreviewImage (RawImage), Background Image, Selection Border, Name Text |
| DicePreviewRenderer | Preview Layer mask configured |
| ThrowingPanel | Status Text |
| ScoringPanel | Result/Score/Message Text, Continue Button |
| ShopPanel | Title Text, Inventory Text, Placeholder Text, Continue Button |
| GameOverPanel | Title Text, Round Text, Score Text, Restart Button |

### 3D Dice Preview System

The dice preview system allows players to see and rotate 3D models of their dice:

**Components:**
- `DicePreviewRenderer` - Singleton that manages a hidden camera and render setup
- `DicePreviewInstance` - Wrapper for each preview die, handles rotation
- `DiceDisplayItem` - UI element that displays the RenderTexture and handles input

**How it works:**
1. DicePreviewRenderer creates preview dice far from the main scene (position 1000,1000,1000)
2. Each die is set to a special "DicePreview" layer
3. A dedicated camera only renders that layer to a RenderTexture
4. The RenderTexture is displayed in a RawImage in the UI
5. Drag input rotates the 3D die and re-renders

**Features:**
- Click to select/deselect dice
- Drag to rotate and inspect all faces
- Auto-rotate after idle timeout
- Each die shows its actual configured face values

## Shop & Economy System

### Currency (CurrencyManager)

Manages player money that can be spent in the shop.

**Earning Money:**
- After successful rounds, players earn money based on remaining throws
- Default: $10 per remaining throw (configurable in GameManager)
- Money persists across rounds within a game
- Money resets to $0 on game over

**Key Methods:**
- `AddMoney(amount)` - Earn money
- `SpendMoney(amount)` - Spend money (returns false if insufficient)
- `CanAfford(amount)` - Check if player can afford something
- `Reset()` - Clear all money (game over)

### Modifiers

Equippable items that modify scoring rules during gameplay.

**Structure:**
```
Assets/Scripts/Modifiers/
├── IScoreModifier.cs           # Interface for all modifiers
├── ScoreModifierTiming.cs      # When modifier applies (PerDie/AfterThrow)
├── BaseModifier.cs             # MonoBehaviour base class
├── ModifierData.cs             # ScriptableObject for shop items
├── ModifierManager.cs          # Manages active modifiers (Singleton)
└── Implementations/
    ├── DoubleOnesModifier.cs   # "Lucky Ones" - doubles 1s
    ├── FlatBonusModifier.cs    # "Steady Hand" - +5 per throw
    └── MatchingBonusModifier.cs # "Pair Master" - bonus for matches
```

**Creating New Modifiers:**

1. Create a new class extending `BaseModifier`:
```csharp
public class MyModifier : BaseModifier
{
    private void Reset()
    {
        _name = "My Modifier";
        _description = "Does something cool.";
        _timing = ScoreModifierTiming.PerDie; // or AfterThrow
    }

    public override int ModifyScore(ScoreModifierContext context)
    {
        // Modify context.CurrentScore and return the new value
        return context.CurrentScore + 5;
    }
}
```

2. Create prefab with the modifier component
3. Create ModifierData asset (Assets → Create → Dice Game → Modifier)
4. Assign prefab, set cost, name, description, icon
5. Add to ShopManager's `_availableModifiers` list

**ScoreModifierContext Properties:**
- `CurrentScore` - Current score value (modify this)
- `OriginalValue` - Original value before any modifications
- `DieIndex` - Which die (0-based), or -1 for AfterThrow
- `TotalDiceCount` - Number of dice in throw
- `AllDieValues` - Array of all die values (for combo detection)
- `ThrowNumber` - Current throw (1-3)
- `RoundNumber` - Current round

### Enhancements

Permanent modifications applied to selected dice for the rest of the game.

**Structure:**
```
Assets/Scripts/Enhancements/
├── IEnhancement.cs             # Interface for all enhancements
├── BaseEnhancement.cs          # MonoBehaviour base class
├── EnhancementData.cs          # ScriptableObject for shop items
└── Implementations/
    ├── AddValueEnhancement.cs      # "Power Up" - +1 to all faces
    ├── BoostLowestEnhancement.cs   # "Lucky Break" - raise minimum
    └── SynergyEnhancement.cs       # "Synergy" - boost 2 dice together
```

**Creating New Enhancements:**

1. Create a new class extending `BaseEnhancement`:
```csharp
public class MyEnhancement : BaseEnhancement
{
    private void Reset()
    {
        _name = "My Enhancement";
        _description = "Upgrades dice somehow.";
        _requiredDiceCount = 1; // How many dice to select
    }

    public override int[] ApplyToDie(int[] currentValues)
    {
        // Modify and return the face values array
        int[] modified = (int[])currentValues.Clone();
        for (int i = 0; i < modified.Length; i++)
            modified[i] += 1;
        return modified;
    }
}
```

2. Create prefab with the enhancement component
3. Create EnhancementData asset (Assets → Create → Dice Game → Enhancement)
4. Assign prefab, set cost, name, description, icon
5. Add to ShopManager's `_availableEnhancements` list

**Multi-Dice Enhancements:**
- Set `_requiredDiceCount` to how many dice must be selected
- The enhancement is applied to each selected die
- Player must select exactly the required number

### ShopManager

Handles shop logic and purchases.

**Configuration:**
- `_availableModifiers` - List of ModifierData assets for sale
- `_availableEnhancements` - List of EnhancementData assets for sale

**Key Methods:**
- `PurchaseModifier(modifier)` - Buy a modifier
- `ApplyEnhancement(enhancement, selectedDice)` - Apply enhancement to dice
- `CanAffordModifier/Enhancement(item)` - Affordability checks

### ShopPanel UI

**Sections:**
1. **Money Display** - Shows current currency
2. **Modifiers Section** - List of purchasable modifiers
3. **Enhancements Section** - List of purchasable enhancements
4. **Dice Selection** - Appears when selecting dice for enhancement
5. **Continue Button** - Proceed to next round

**Enhancement Purchase Flow:**
1. Player clicks enhancement
2. Dice selection UI appears with inventory
3. Player selects required number of dice
4. Click "Apply" to purchase and modify dice
5. Original dice replaced with enhanced versions in inventory

### ActiveModifiersPanel

Persistent UI panel showing currently equipped modifiers.

**Setup:**
- Add to scene (can be child of Canvas or HUD)
- Automatically subscribes to ModifierManager events
- Updates when modifiers are added/removed

### Sample Items

**Modifiers (purchased from shop, affect scoring):**

| Name | Timing | Effect | Default Cost |
|------|--------|--------|--------------|
| Lucky Ones | PerDie | Doubles dice showing 1 | $50 |
| Steady Hand | AfterThrow | +5 to every throw | $50 |
| Pair Master | AfterThrow | +3/pair, +10/triple, +25/quad+ | $50 |

**Enhancements (permanent dice upgrades):**

| Name | Dice Required | Effect | Default Cost |
|------|---------------|--------|--------------|
| Power Up | 1 | +1 to all faces | $30 |
| Lucky Break | 1 | Raise lowest face to 3 | $30 |
| Synergy | 2 | +2 to all faces of both dice | $30 |

### Scene Setup for Shop

1. **Add ModifierManager:**
   - Create empty GameObject "ModifierManager"
   - Add ModifierManager component
   - Assign reference in GameManager

2. **Create Modifier/Enhancement Prefabs:**
   - Create prefab with modifier/enhancement component
   - Configure settings in the component

3. **Create Data Assets:**
   - Assets → Create → Dice Game → Modifier/Enhancement
   - Assign prefab, configure cost/name/description

4. **Configure ShopManager:**
   - Add ModifierData assets to `_availableModifiers`
   - Add EnhancementData assets to `_availableEnhancements`

5. **Update ShopPanel:**
   - Assign modifier/enhancement containers
   - Assign dice selection container and prefab
   - Assign instruction text, apply/cancel buttons

6. **Add CurrencyDisplay (optional):**
   - Add CurrencyDisplay component to HUD
   - Shows current money during gameplay
