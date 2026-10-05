# Adding analytics events step by step

This guide covers the Bludoku gameplay integration. Delivery and provider
contracts come from the existing Analytics module.

The flow is:

    Gameplay notification
        → GameplayAnalyticsIntegrationSystem
        → GameplayAnalyticsSystem
        → existing Analytics module
        → configured providers

Keep event schemas in GameplayAnalyticsSystem and SDK-specific mappings in
provider adapters. Gameplay should publish domain notifications only.

## Step 1: define the trigger and payload

Decide what completed action the event represents, when it fires, and which
parameters it needs. Emit after success, not on button press or every animation
frame. Decide whether repeated actions should produce separate events.

Example: **MultiAreaClear**, emitted once for a successful placement that clears
at least two areas. Its payload reuses ClearedCells and ClearedAreas, alongside
the shared RunId, Score, Moves, and Revives context.

This is an illustrative event to add; it is not already implemented.

## Step 2: reuse an existing event when it fits

Check the existing gameplay and Analytics enums first. BonusReceived,
PowerUpUsed, and Revived already cover their respective actions.

MultiAreaClear overlaps the existing DestructionCombo event. Add it only if
you need a separate metric independent of configured destruction tiers.
Otherwise, use DestructionCombo and its ClearedAreas parameter.

## Step 3: assign stable IDs

In EGameplayAnalyticsEvent.cs, append a new explicitly numbered value:

    MultiAreaClear = 10

At the time of writing, existing gameplay event IDs are 1–9. Check the current
file before using 10. Never renumber or reuse an existing ID.

No new parameters are required for this example. If your event needs a new
parameter, append it to EGameplayAnalyticsParameter.cs using a unique ID.
Custom parameter IDs must be at least 1000; the current values end at 1014.

Reuse existing parameter IDs only when their meaning and value type match.
Avoid arbitrary string keys and changing a parameter's type between releases.

## Step 4: build the event in GameplayAnalyticsSystem

Add the following constant and method inside GameplayAnalyticsSystem:

    private const int MinimumMultiAreaClearAreas = 2;

    private void TrackMultiAreaClear(ScorePlacementDataStruct data)
    {
        if (data.ClearedAreas < MinimumMultiAreaClearAreas) return;

        var parameters = Context();
        parameters.Add(Integer(
            EGameplayAnalyticsParameter.ClearedCells, data.ClearedCells));
        parameters.Add(Integer(
            EGameplayAnalyticsParameter.ClearedAreas, data.ClearedAreas));

        SendCustom(EGameplayAnalyticsEvent.MultiAreaClear, parameters);
    }

Context() includes RunId, Score, Moves, and Revives. Do not add those parameters
again: duplicate parameter IDs are rejected.

Use the existing Integer, Boolean, CustomId, and standard parameter helpers.
For a custom numeric floating-point value, construct AnalyticsParameterData
with CustomId and a double value. Choose a consistent type for each parameter.

## Step 5: connect it to the confirmed action

The current PiecePlaced method already receives ScorePlacementDataStruct
through the existing OnPlacementScored subscription. After its existing
PiecePlaced event is sent, add:

    TrackMultiAreaClear(data);

Place this call inside PiecePlaced, after the active-run/game-over guard and
after updating _moves and _score. The new event then contains the correct
context and requires no gameplay or subscription changes.

Do not also subscribe to OnPlacementScored for this event; doing both would
emit it twice.

## Step 6: add a domain notification only if one is missing

Skip this step for MultiAreaClear.

For a genuinely new mechanic, publish a typed C# event from the component or
system that owns the action. Invoke it only after the action succeeds.
Use properties or an immutable data struct for payloads; structs must have
the Struct suffix.

In GameplayAnalyticsIntegrationSystem:

1. Store the event source as a private field.
2. Subscribe once in the constructor.
3. Forward the payload to a method in GameplayAnalyticsSystem.
4. Remove the same subscription in Dispose.

Follow the existing OnPlacementScored and OnPlacementRejected patterns.
If a source is replaced during scene changes, dispose the old integration
before creating a new one. Do not call provider SDKs from gameplay components.

## Step 7: check the group and routes

SendCustom currently sends EAnalyticsEvent.Custom with EAnalyticsGroup.Gameplay.
The sample gameplay routes already include Gameplay, so no route change is
needed for this example.

An event's delivery group is the group passed to Track. A custom event can
belong to Gameplay; it does not have to belong to EAnalyticsGroup.Custom.

For Economy, UI, or another group, add/use a helper that passes the correct
group and verify it is enabled on the intended routes in
Assets/_Bludoku/Configs/Analytics/BludokuAnalytics.asset.

SampleAnalyticsProvider accepts generic event data and requires no change.
A real SDK adapter may need a mapping from the new numeric event/parameter IDs
to its SDK names. Keep that mapping out of gameplay code.

## Step 8: verify the event in Play Mode

1. Open Assets/_Bludoku/Scenes/Bootstrap.unity and enter Play Mode.
2. Open GameBrewStudio → Core Template → Analytics Events.
3. Perform the action once.
4. Select its Custom row and check _customId = 10 in the JSON
   (or the ID you assigned).
5. Check the payload, RunId, and delivery statuses.
6. Repeat with a nonqualifying action and confirm that no new event is emitted.

Delivery history shows Queued/Delivered status records separately for each
matching provider. Four rows with one sequence number can represent one event
delivered to two providers. Repeated sequence numbers are not duplicate actions.
Separate sequence numbers identify separate recorded events; compare their
custom IDs and triggers before treating them as duplicates.

## Step 9: document the schema

Add the event's trigger and payload to the event table in the project README.
Record any new IDs, units, types, and once-per-action rules.

New analytics events should usually require changes only to the gameplay
analytics schema and tests. Change gameplay code only to expose a missing
domain notification; change provider adapters only for SDK-specific mappings.