using DG.Tweening;
using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    public sealed class CameraShakeView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        private Tween _tween;
        private Vector3 _originalPosition;

        public void Play(DestructionComboTierStruct tier)
        {
            Stop();
            _originalPosition = _camera.transform.localPosition;
            Vector3 strength = new Vector3(tier.ShakeStrength, tier.ShakeStrength, 0f);
            _tween = _camera.transform.DOShakePosition(tier.ShakeDuration, strength, tier.ShakeVibrato)
                .OnComplete(Stop);
        }

        public void Stop()
        {
            if (_tween == null) return;
            _tween.Kill();
            _tween = null;
            _camera.transform.localPosition = _originalPosition;
        }

        private void OnDisable() => Stop();
    }
}
