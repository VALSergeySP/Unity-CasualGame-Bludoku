using System;
using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    [Serializable]
    public struct ComboTextAnimationStruct
    {
        [SerializeField, Min(0.01f)] private float _appearDuration;
        [SerializeField, Min(0f)] private float _holdDuration;
        [SerializeField, Min(0.01f)] private float _fadeDuration;
        [SerializeField, Min(0.01f)] private float _startScale;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private float _riseDistance;

        public float AppearDuration => Mathf.Max(0.01f, _appearDuration);
        public float HoldDuration => Mathf.Max(0f, _holdDuration);
        public float FadeDuration => Mathf.Max(0.01f, _fadeDuration);
        public float StartScale => Mathf.Max(0.01f, _startScale);
        public Vector3 Offset => _offset;
        public float RiseDistance => _riseDistance;

        public static ComboTextAnimationStruct Default => new ComboTextAnimationStruct
        {
            _appearDuration = 0.2f, _holdDuration = 0.6f, _fadeDuration = 0.4f,
            _startScale = 0.6f, _riseDistance = 0.4f
        };
    }
}
