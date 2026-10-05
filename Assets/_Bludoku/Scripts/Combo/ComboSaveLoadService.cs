using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    public sealed class ComboSaveLoadService
    {
        private const string ComboKey = "GeneralCombo.Count";
        private const string HandClearKey = "GeneralCombo.HandCleared";

        public void Load(GeneralComboSystem system) => system.Restore(
            PlayerPrefs.GetInt(ComboKey), PlayerPrefs.GetInt(HandClearKey) != 0);

        public void Save(GeneralComboSystem system)
        {
            PlayerPrefs.SetInt(ComboKey, system.Combo);
            PlayerPrefs.SetInt(HandClearKey, system.HasClearedInHand ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
