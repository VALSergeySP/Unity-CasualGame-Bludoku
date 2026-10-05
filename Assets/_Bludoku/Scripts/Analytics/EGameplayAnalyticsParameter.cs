namespace _Bludoku.Scripts.Analytics
{
    public enum EGameplayAnalyticsParameter
    {
        PieceId = 1000,
        Column = 1001,
        Row = 1002,
        ClearedCells = 1003,
        ClearedAreas = 1004,
        Combo = 1005,
        GeneralMultiplier = 1006,
        DestructionMultiplier = 1007,
        Score = 1008,
        ScoreGained = 1009,
        Moves = 1010,
        Revives = 1011,
        BonusPoints = 1012,
        ExitReason = 1013,
        IsRestored = 1014
    }

    public enum EGameplayExitReason
    {
        MainMenu = 0,
        Restart = 1,
        ApplicationQuit = 2,
        SceneChanged = 3
    }

    public enum EGameplayBonus
    {
        ComboScore = 1
    }

    public enum EGameplayPowerUp
    {
        ReplaceHand = 1
    }
}