# Bludoku: combos, effects, and gameplay analytics

The project uses two combo systems: the accumulating General Combo and
Destruction Combo for clearing multiple areas at once. Both multipliers
contribute to the score awarded for a move. Gameplay analytics is implemented
as a separate integration layer on top of the existing Analytics module.

## 1. Clearing and scoring rules

Board checks completed rows, columns, and 3×3 boxes after a piece is placed.

- **ClearedAreas / FiguresRemovedCount** is the number of completed areas.
  For example, a row and a column cleared by one move count as two areas.
- **ClearedCells / ClearedCount** is the number of unique cleared cells.
  A cell shared by multiple areas is counted once.

Areas control combos; cells determine the base score.

ScoreMediator coordinates the sequence: update General Combo, determine
Destruction Combo, calculate the score, save state, and play the effects.

The formula in ScoreConfig is:

    score = round(
        cleared cells × Points Per Cell
        × General Multiplier × Destruction Multiplier
    )

Rounding happens once, after both multipliers are applied.
A move without a clear awards no points through this formula.

Configuration: Assets/_Bludoku/Configs/ScoreConfig.asset.

| Inspector field | Purpose | Current value |
| --- | --- | --- |
| Points Per Cell | Base points per cleared cell | 1 |
| Rounding | Floor rounds down; Nearest uses Mathf.RoundToInt; Ceiling rounds up | Floor |

## 2. General Combo: accumulating streak

GeneralComboSystem contains the rules; GeneralComboConfig contains the settings.
A **hand** is the current set of available pieces. It is completed when the
player places all of those pieces.

Rules:

1. Every placement that clears at least one area increases Combo by one.
   Clearing several areas in the same move still increases it only once.
2. That move sets HasClearedInHand, indicating that the current hand had a clear.
3. A placement without a clear does not reset Combo by itself.
4. Completing an entire hand without any clears resets Combo to zero.
5. If the completed hand had a clear, Combo carries over to the next hand.
   HasClearedInHand is cleared for the next hand.
6. Replacing the hand during a revive preserves Combo but clears HasClearedInHand.
   The replacement hand must produce its own clear to preserve the streak.
7. Starting a new game resets both Combo and HasClearedInHand.

Example: the first piece clears a row, the second clears nothing, and the third
clears a box. Combo increases twice and survives hand completion. If the next
hand has no clears, the streak resets when that hand is completed.

The multiplier uses the updated Combo, so the current clear immediately benefits
from the new value:

    if Combo ≤ 0: General Multiplier = 1
    otherwise: General Multiplier =
        max(1, Base Multiplier + Growth × (Combo − 1)^Exponent)

Configuration: Assets/_Bludoku/Configs/GeneralComboConfig.asset.

| Inspector field | Purpose | Current value |
| --- | --- | --- |
| Base Multiplier | Multiplier for the first combo | 1 |
| Growth | Growth amount | 0.1 |
| Exponent | Growth exponent; 1 produces linear growth | 1 |
| Minimum Visible Combo | Minimum combo required to display the General Combo text | 2 |
| Animation | Text appearance and disappearance settings | See the effects section |

With the current settings: Combo 1 → ×1; Combo 2 → ×1.1; Combo 3 → ×1.2.
Minimum Visible Combo controls only the text and does not affect scoring.

ComboSaveLoadService stores Combo and HasClearedInHand using PlayerPrefs.
ScoreMediator loads the state at startup and saves it after placements,
hand completion/replacement, and starting a new game.

## 3. Destruction Combo: simultaneous clears

DestructionComboSystem determines a bonus from the number of areas cleared
by **one piece placement**. It does not accumulate state between moves.

DestructionComboConfig defines Tiers. The system selects the eligible tier
with the highest Minimum Areas that does not exceed the number of cleared areas.
List order does not affect selection. Thresholds below two are ignored.
If no tier matches, the multiplier is one and no destruction effect is played.

Configuration: Assets/_Bludoku/Configs/DestructionComboConfig.asset.

| Minimum Areas | Multiplier | Text | Shake Strength | Shake Duration | Shake Vibrato |
| --- | --- | --- | --- | --- | --- |
| 2 | ×1.25 | Wow | 0.06 | 0.18 s | 12 |
| 3 | ×1.5 | Great | 0.09 | 0.22 s | 14 |
| 4 | ×2 | Perfect | 0.12 | 0.26 s | 16 |
| 5 | ×3 | Legendary | 0.15 | 0.30 s | 18 |

Six cleared areas also select Legendary until a higher threshold is added.
Each tier also has Text Color. The Animation block configures the shared text
animation. Multiplier is clamped to a minimum of ×1.

**Combined scoring example.** A move clears a row and a column sharing one cell:
two areas and 17 unique cells. If this move increases General Combo to 3, the
current settings produce:

    Floor(17 × 1 × 1.2 × 1.25) = Floor(25.5) = 25 points

## 4. Combo effects and settings

Effects use DOTween. Changing their appearance does not change combos or scoring.

### General Combo and Destruction Combo text

GeneralComboView displays "Combo N" near the camera center after a clearing move
once Minimum Visible Combo is reached. New text replaces the previous text.
Its component references are Text Prefab, Text Color, Camera, and General Combo Config.

DestructionComboView displays the selected tier's Text and Text Color near the
placed piece and starts CameraShakeView. Its references are Text Prefab,
Destruction Combo Config, and Camera Shake View.

Both use ComboTextView and ComboTextAnimationStruct:

| Animation field | Purpose | Current value in config assets |
| --- | --- | --- |
| Appear Duration | Text scaling and alpha appearance time | 0.2 s |
| Hold Duration | Time the text remains visible | 0.6 s |
| Fade Duration | Fade-out and upward movement time | 0.4 s |
| Start Scale | Initial scale relative to full scale | 0.6 |
| Offset | World-space offset from the spawn position | General: (0, 0, 0); Destruction: (0, 0.7, 0) |
| Rise Distance | Upward movement during fade-out | 0.4 |

### Sparks and light ring

ComboEffectsView plays after a clear when Combo ≥ 2.
This threshold is defined by the MinimumCombo constant. It is independent of
Minimum Visible Combo, which affects only the text.

Sparks fly along an arc from cleared cell positions toward a UI target, shrinking
and fading as they travel. A ring appears at the target when the last spark arrives.
Spark and ring objects are reused through internal pools.

Component references: Canvas, Effects Layer, Target, World Camera,
Spark Prefab, and Ring Prefab. Prefabs define the base size, scale, and color.
Spark Scale, Spark Color, and Ring Color multiply the corresponding prefab values;
color multiplication includes alpha.

Spark count and intensity:

    spark count = Clamp(Combo, Minimum Sparks, Maximum Sparks)
    x = (Combo − 2) × Intensity Step
    intensity = x / (1 + x)

Intensity approaches one smoothly. It interpolates the ring's size and alpha
between their minimum and maximum values.

The following values come from the component in
Assets/_Bludoku/Scenes/GameScene 1.unity and may differ from code defaults.

| Inspector field | Purpose | Scene value |
| --- | --- | --- |
| Minimum / Maximum Sparks | Spark count limits | 5 / 12 |
| Spark Scale | Prefab scale multiplier | 4 |
| Spark Color | Spark color multiplier | Light blue |
| Scatter Radius | Lateral deviation along the trajectory | 45 |
| Arc Height | Arc height | 90 |
| Flight Duration | Flight time | 0.45 s |
| Spark Delay | Delay between spark launches | 0.035 s |
| Spark Fade Duration | Fade-out time at the end of the flight | 0.12 s |
| Flight Ease | Movement easing | InQuad |
| Spark Scale Ease | Scale-down easing | InQuad |
| Ring Color | Ring color multiplier | Purple |
| Minimum / Maximum Ring Scale | Scale range based on intensity | 1 / 2.5 |
| Minimum / Maximum Ring Alpha | Alpha range based on intensity | 0.35 / 1 |
| Ring Duration | Total ring duration | 0.4 s |
| Ring Fade In Duration | Ring appearance time | 0.06 s |
| Initial / Final Ring Scale | Animation's initial and final scale factors | 0.65 / 1.3 |
| Ring Scale Ease | Expansion easing | OutCubic |
| Intensity Step | Intensity growth rate | 0.15 |
| Vibration Enabled | Whether this effect permits vibration | Enabled |

Scatter Radius and Arc Height use Effects Layer local coordinates.
The ring expands from its calculated scale × Initial Ring Scale to its
calculated scale × Final Ring Scale.

Vibration runs only in Android/iOS builds outside the Editor and also checks
SettingsManager.IsVibrationEnabled. Disabling the component or clearing its
effects stops animations and hides pooled objects.

### Camera shake

CameraShakeView moves the camera only along X/Y. Each destruction tier configures
Shake Strength, Shake Duration, and Shake Vibrato.
A new shake stops the previous one. Completion or stopping restores the camera's
original local position.

### Score bonus indicator

ScoreBoosterView appears at Combo ≥ 2 and pulses. It is a visual indicator;
ScoreMediator calculates the score multipliers.

The Inspector exposes the Booster reference. Appearance/disappearance timing
and pulse settings are constants in ScoreBoosterView rather than config assets:
Show Duration = 0.8 s, Hide Duration = 0.2 s, amplitude = 0.08–0.2,
Base Pulse Duration = 0.45 s, Minimum Pulse Duration = 0.18 s,
and Combo Intensity Step = 0.15. As Combo grows, the pulse becomes stronger
and faster.

## 5. Gameplay analytics

Event delivery uses the existing **Analytics** module in
Assets/_Bludoku/ThirdParty/Analytics. The project adds a gameplay event layer
and connects it to the game.

The current test integration uses **SampleAnalyticsModuleSystem** with the
existing **SampleAnalyticsProvider** implementations. There is no separate
console analytics provider.

### Integration components

| Project component | Responsibility |
| --- | --- |
| GameplayAnalyticsSystem | Builds typed events, tracks RunId, score, move/revive counts, and prevents repeated game over / revive / exit events |
| GameplayAnalyticsIntegrationSystem | Subscribes to domain notifications from GameController, FiguresController, and ScoreMediator |
| GameplayAnalyticsViewController | Lives in the Bootstrap scene; explicitly initialized; handles scene binding, delivery scheduling, and module shutdown |
| EGameplayAnalyticsEvent / EGameplayAnalyticsParameter | Stable numeric IDs for gameplay events and parameters |
| ScorePlacementDataStruct | Carries the confirmed move result after scoring |

Gameplay components publish notifications about completed actions.
They do not call analytics SDKs or construct SDK event names and parameters.

### Events

| Event | Trigger |
| --- | --- |
| LevelStarted / GameResumed | Starting a run / loading a saved board |
| PiecePlaced / PlacementRejected | Successful placement / snapping a piece back after an invalid drop |
| HandCompleted / HandReplaced | Placing the entire hand / successfully generating a replacement |
| ComboIncreased / ComboEnded | Increasing the combo / resetting the streak after a hand without clears |
| DestructionCombo | Applying a destruction tier |
| ResourceReceived | Actual bonus points awarded by combo multipliers |
| BoosterUsed | Replacing the hand during a second chance |
| GameOver / Revived | No available moves / a successful second chance |
| LevelExited | Returning to the menu, restarting, quitting, or changing scenes |

ApplicationStarted/ApplicationEnded and simulated AttributionReceived are
also used during the test module's lifecycle.

Gameplay events share RunId and the Score, Moves, and Revives context.
Placement events also include the piece ID, board coordinates, cleared
cells/areas, and awarded score. Invalid attempts do not increase Moves.

Bonus points are the difference between the awarded score and the score for
the same clear without multipliers. The game has no separate consumable power-up
inventory; its current power-up action is hand replacement. Typed BonusReceived
and PowerUpUsed methods are available for future mechanics.

A revive continues the same RunId. Game over does not finalize the run because
a second chance may follow. Exit is recorded once. Returning to a saved board
creates a new RunId for the new visit.
DurationSeconds in LevelExited measures the full visit, including dialogs and
application pauses. Additive scenes do not end the current run.

### Explicit startup and lifecycle

The entry scene is Assets/_Bludoku/Scenes/Bootstrap.unity (Build Settings index 0).
It already contains Game Bootstrap with two components:

- GameBootstrapViewController: Analytics reference and Initial Scene Index
  (currently 1, MainMenu).
- GameplayAnalyticsViewController: Config reference and Flush Interval Seconds
  (currently 1 second).

GameBootstrapViewController calls InitializeAsync, waits for initialization,
and opens the menu. Analytics failure does not block the game from starting.
The config is assigned directly in the Inspector; Resources.Load is not used.

There is no RuntimeInitializeOnLoadMethod, singleton, or GameObject creation
from code. The existing Bootstrap object persists through DontDestroyOnLoad,
so returning to the menu does not create a second analytics session.
Load Bootstrap only once at application startup.

ShutdownAsync explicitly removes subscriptions, stops the delivery loop, and
disposes the module. Repeated InitializeAsync/ShutdownAsync calls do not create
additional tasks or providers. OnDestroy also shuts it down.
A stopped controller cannot be restarted.

Opening MainMenu or GameScene directly does not silently create analytics.
Start Bootstrap to test the complete integration.
Build Settings order: Bootstrap → MainMenu → GameScene 1.

### Checking events in the Editor

1. Open Assets/_Bludoku/Scenes/Bootstrap.unity and enter Play Mode.
2. Open **GameBrewStudio → Core Template → Analytics Events**.
3. Make several moves and inspect Delivery history, Persistent queue,
   and the selected event's parameters.
4. Switch test provider Response between Success, Failure, and Unavailable
   to check successful delivery, failure, and unavailability.

The gameplay config is Assets/_Bludoku/Configs/Analytics/BludokuAnalytics.asset.
It references the module's existing sample route/provider assets.
No separate provider writes gameplay events to the Console.

Delivery history contains status records, not one row per gameplay action.
The same event sequence can appear as Queued and Delivered for each matching
provider. The window displays custom gameplay events as Custom; inspect
_customId in the selected event's JSON to identify the event.

### Extending the integration

For a new analytics event, reuse a domain notification, add a numeric ID and
parameter construction in GameplayAnalyticsSystem, and add a subscription in
GameplayAnalyticsIntegrationSystem if needed. Gameplay code changes are needed
only when a new mechanic has no existing notification.

See [Adding analytics events step by step](Assets/_Bludoku/Scripts/Analytics/ADDING_EVENTS.md)
for a concrete example and verification instructions.

To integrate another SDK, implement a provider adapter using the existing
Analytics contracts and add its config and route to the gameplay config.
Gameplay components and rules do not need changes.
Do not renumber existing event or parameter IDs.

## 6. Verification

Tests/Analytics/GameplayAnalyticsChecks.cs verifies gameplay event schemas,
invalid placements, bonus points, duplicate guards, revives, RunId correlation,
menu exits, duration, restore, and restart.

From the project root in PowerShell:

    ./Tests/Analytics/Run-Checks.ps1 -UnityData "D:/Unity/2022.3.62f3/Editor/Data"

Replace UnityData with the Editor/Data path of your Unity installation.