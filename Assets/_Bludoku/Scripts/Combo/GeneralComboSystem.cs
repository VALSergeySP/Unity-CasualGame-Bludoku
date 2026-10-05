namespace _Bludoku.Scripts.Combo
{
    public sealed class GeneralComboSystem
    {
        public int Combo { get; private set; }
        public bool HasClearedInHand { get; private set; }

        public void Restore(int combo, bool hasClearedInHand)
        {
            Combo = System.Math.Max(0, combo);
            HasClearedInHand = hasClearedInHand;
        }

        public void FigurePlaced(int clearedAreas)
        {
            if (clearedAreas <= 0) return;
            Combo++;
            HasClearedInHand = true;
        }

        public void CompleteHand()
        {
            if (!HasClearedInHand) Combo = 0;
            HasClearedInHand = false;
        }

        public void BeginReplacementHand() => HasClearedInHand = false;
        public void Reset() => Restore(0, false);
    }
}
