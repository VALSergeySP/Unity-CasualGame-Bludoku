namespace _Bludoku.Scripts.Combo
{
    public sealed class DestructionComboSystem
    {
        private readonly DestructionComboConfig _config;
        public DestructionComboSystem(DestructionComboConfig config) => _config = config;
        public bool TryCalculate(int clearedAreas, out DestructionComboTierStruct tier) => _config.TryGetTier(clearedAreas, out tier);
    }
}
