namespace _Bludoku.Scripts.Score
{
    public readonly struct ScorePlacementDataStruct
    {
        public int PieceId { get; }
        public int Column { get; }
        public int Row { get; }
        public int ClearedCells { get; }
        public int ClearedAreas { get; }
        public int PreviousCombo { get; }
        public int Combo { get; }
        public float GeneralMultiplier { get; }
        public float DestructionMultiplier { get; }
        public bool HasDestructionCombo { get; }
        public int Score { get; }
        public int ScoreGained { get; }
        public int BonusPoints { get; }

        public ScorePlacementDataStruct(int pieceId, int column, int row, int clearedCells, int clearedAreas,
            int previousCombo, int combo, float generalMultiplier, float destructionMultiplier,
            bool hasDestructionCombo, int score, int scoreGained, int bonusPoints)
        {
            PieceId = pieceId; Column = column; Row = row;
            ClearedCells = clearedCells; ClearedAreas = clearedAreas;
            PreviousCombo = previousCombo; Combo = combo;
            GeneralMultiplier = generalMultiplier; DestructionMultiplier = destructionMultiplier;
            HasDestructionCombo = hasDestructionCombo; Score = score; ScoreGained = scoreGained;
            BonusPoints = bonusPoints;
        }
    }
}