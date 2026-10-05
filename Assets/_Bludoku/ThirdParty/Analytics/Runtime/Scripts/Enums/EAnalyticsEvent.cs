namespace GameBrewStudio.CoreTemplate.Analytics
{
    public enum EAnalyticsEvent
    {
        Custom = 0, ApplicationStarted = 1, ApplicationEnded = 2, LevelStarted = 3, LevelWon = 4,
        LevelLost = 5, LevelExited = 6, Revived = 7, ResourceReceived = 8, ResourceSpent = 9,
        BoosterUsed = 10, AdvertisementResult = 11, PurchaseResult = 12, ScreenOpened = 13,
        TutorialStarted = 14, TutorialCompleted = 15, AttributionReceived = 16
    }
}
