using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Bludoku.Scripts.Score
{
    public class ScoreBoosterView : MonoBehaviour
    {
        private const int MinimumActiveCombo = 2;
        private const float ShowDuration = 0.8f;
        private const float HideDuration = 0.2f;
        private const float MinimumPulseAmplitude = 0.08f;
        private const float MaximumPulseAmplitude = 0.2f;
        private const float BasePulseDuration = 0.45f;
        private const float MinimumPulseDuration = 0.18f;
        private const float ComboIntensityStep = 0.15f;

        [FormerlySerializedAs("booster"), SerializeField] private Transform _booster;

        private bool _isBoosterEnabled;
        private Tween _pulseTween;
        private Tween _visibilityTween;
        private int _combo;

        private void Awake()
        {
            _booster.localScale = Vector3.zero;
        }

        public void SetCombo(int combo)
        {
            bool comboChanged = _combo != combo;
            _combo = combo;
            if (!isActiveAndEnabled)
                return;

            bool boosterEnabled = combo >= MinimumActiveCombo;
            if (_isBoosterEnabled == boosterEnabled)
            {
                if (boosterEnabled && comboChanged && _visibilityTween == null)
                    StartPulse();
                return;
            }

            _visibilityTween?.Kill();
            _pulseTween?.Kill();
            _pulseTween = null;
            _isBoosterEnabled = boosterEnabled;

            if (boosterEnabled)
            {
                _visibilityTween = _booster.DOScale(Vector3.one, ShowDuration)
                    .SetEase(Ease.OutElastic)
                    .OnComplete(() =>
                    {
                        _visibilityTween = null;
                        StartPulse();
                    });
            }
            else
            {
                _visibilityTween = _booster.DOScale(Vector3.zero, HideDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => _visibilityTween = null);
            }
        }

        private void StartPulse()
        {
            if (!_isBoosterEnabled)
                return;

            _pulseTween?.Kill();
            float intensity = Mathf.Max(0, _combo - MinimumActiveCombo) * ComboIntensityStep;
            float amplitude = Mathf.Lerp(MinimumPulseAmplitude, MaximumPulseAmplitude,
                intensity / (1f + intensity));
            float duration = Mathf.Max(MinimumPulseDuration, BasePulseDuration / (1f + intensity));
            _booster.localScale = Vector3.one;
            _pulseTween = _booster.DOScale(1f + amplitude, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnEnable() => SetCombo(_combo);

        private void OnDisable()
        {
            _visibilityTween?.Kill();
            _visibilityTween = null;
            _pulseTween?.Kill();
            _pulseTween = null;
            _isBoosterEnabled = false;
            _booster.localScale = Vector3.zero;
        }
    }
}
