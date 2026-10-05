using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    [CreateAssetMenu(menuName = "Bludoku/General Combo Config")]
    public sealed class GeneralComboConfig : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _baseMultiplier = 1f;
        [SerializeField, Min(0f)] private float _growth = 0.1f;
        [SerializeField, Min(0f)] private float _exponent = 1f;
        [SerializeField, Min(1)] private int _minimumVisibleCombo = 2;
        [SerializeField] private ComboTextAnimationStruct _animation = ComboTextAnimationStruct.Default;

        public int MinimumVisibleCombo => _minimumVisibleCombo;
        public ComboTextAnimationStruct Animation => _animation;

        // multiplier = base + growth * (combo - 1)^exponent; no combo gives x1.
        public float GetMultiplier(int combo) => combo <= 0 ? 1f :
            Mathf.Max(1f, _baseMultiplier + _growth * Mathf.Pow(Mathf.Max(0, combo - 1), _exponent));
    }
}
