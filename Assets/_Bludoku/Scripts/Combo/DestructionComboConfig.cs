using System;
using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    [Serializable]
    public struct DestructionComboTierStruct
    {
        [SerializeField, Min(2)] private int _minimumAreas;
        [SerializeField, Min(1f)] private float _multiplier;
        
        [SerializeField] private string _text;
        [SerializeField] private Color _textColor;
        
        [SerializeField, Min(0f)] private float _shakeStrength;
        [SerializeField, Min(0.01f)] private float _shakeDuration;
        [SerializeField, Min(1)] private int _shakeVibrato;

        public int MinimumAreas => _minimumAreas;
        public float Multiplier => Mathf.Max(1f, _multiplier);
        public string Text => _text;
        public Color TextColor => _textColor;
        public float ShakeStrength => Mathf.Max(0f, _shakeStrength);
        public float ShakeDuration => Mathf.Max(0.01f, _shakeDuration);
        public int ShakeVibrato => Mathf.Max(1, _shakeVibrato);
    }

    [CreateAssetMenu(menuName = "Bludoku/Destruction Combo Config")]
    public sealed class DestructionComboConfig : ScriptableObject
    {
        [SerializeField] private DestructionComboTierStruct[] _tiers = Array.Empty<DestructionComboTierStruct>();
        [SerializeField] private ComboTextAnimationStruct _animation = ComboTextAnimationStruct.Default;
        
        public ComboTextAnimationStruct Animation => _animation;

        public bool TryGetTier(int areas, out DestructionComboTierStruct tier)
        {
            const int MinimumComboAreas = 2;
            tier = default;
            bool found = false;
            
            if (areas < MinimumComboAreas) 
                return false;
            
            foreach (DestructionComboTierStruct candidate in _tiers)
            {
                if (candidate.MinimumAreas < MinimumComboAreas || candidate.MinimumAreas > areas) 
                    continue;
                
                if (found && candidate.MinimumAreas <= tier.MinimumAreas) 
                    continue;
                
                tier = candidate;
                found = true;
            }
            
            return found;
        }
    }
}
