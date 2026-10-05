namespace _Bludoku.Scripts.Analytics
{
    // Persisted IDs: append new values; never renumber existing events.
    public enum EGameplayAnalyticsEvent
    {
        PiecePlaced = 1,
        PlacementRejected = 2,
        HandCompleted = 3,
        HandReplaced = 4,
        ComboIncreased = 5,
        ComboEnded = 6,
        DestructionCombo = 7,
        GameOver = 8,
        GameResumed = 9
    }
}