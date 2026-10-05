using System.Collections.Generic;
using _Bludoku.Scripts.MainMenu;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Bludoku.Scripts.Combo
{
    public sealed class ComboEffectsView : MonoBehaviour
    {
        private const int MinimumCombo = 2;
        private const float MinimumDuration = 0.01f;

        [Header("References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _effectsLayer;
        [SerializeField] private RectTransform _target;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Image _sparkPrefab;
        [SerializeField] private Image _ringPrefab;

        [Header("Sparks")]
        [SerializeField, Min(1)] private int _minimumSparks = 3;
        [SerializeField, Min(1)] private int _maximumSparks = 6;
        [Tooltip("Multiplies the scale configured on the spark prefab.")]
        [SerializeField, Min(0f)] private float _sparkScale = 1f;
        [Tooltip("Multiplies the color configured on the spark prefab, including alpha.")]
        [SerializeField] private Color _sparkColor = Color.white;
        [SerializeField, Min(0f)] private float _scatterRadius = 45f;
        [SerializeField, Min(0f)] private float _arcHeight = 90f;
        [SerializeField, Min(MinimumDuration)] private float _flightDuration = 0.45f;
        [SerializeField, Min(0f)] private float _sparkDelay = 0.035f;
        [SerializeField, Min(0f)] private float _sparkFadeDuration = 0.12f;
        [SerializeField] private Ease _flightEase = Ease.InQuad;
        [SerializeField] private Ease _sparkScaleEase = Ease.InQuad;

        [Header("Light ring")]
        [Tooltip("Multiplies the color configured on the ring prefab, including alpha.")]
        [SerializeField] private Color _ringColor = Color.white;
        [Tooltip("Size multiplier relative to the ring prefab at the first active combo.")]
        [SerializeField, Min(0f)] private float _minimumRingScale = 1f;
        [SerializeField, Min(0f)] private float _maximumRingScale = 1.91f;
        [SerializeField, Range(0f, 1f)] private float _minimumRingAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _maximumRingAlpha = 1f;
        [SerializeField, Min(MinimumDuration)] private float _ringDuration = 0.4f;
        [SerializeField, Min(0f)] private float _ringFadeInDuration = 0.06f;
        [SerializeField, Min(0f)] private float _initialRingScale = 0.65f;
        [SerializeField, Min(0f)] private float _finalRingScale = 1.3f;
        [SerializeField] private Ease _ringScaleEase = Ease.OutCubic;

        [Header("Combo intensity")]
        [SerializeField, Min(0f)] private float _intensityStep = 0.15f;
        [SerializeField] private bool _vibrationEnabled = true;

        private readonly List<Image> _sparks = new();
        private readonly List<Image> _rings = new();
        private readonly List<Tween> _animations = new();
        

        public void Play(int combo, IReadOnlyList<Vector3> clearedPositions)
        {
            if (combo < MinimumCombo || !isActiveAndEnabled || clearedPositions == null || clearedPositions.Count == 0)
                return;

            _animations.RemoveAll(animation => !animation.IsActive());
            
            int count = Mathf.Clamp(combo, _minimumSparks, _maximumSparks);
            float intensity = (combo - MinimumCombo) * _intensityStep;
            intensity /= 1f + intensity;
            
            for (int index = 0; index < count; index++)
            {
                Vector3 worldPosition = clearedPositions[index * clearedPositions.Count / count];
                PlaySpark(WorldToLayer(worldPosition), index * _sparkDelay, index == count - 1, intensity);
            }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (_vibrationEnabled && SettingsManager.IsVibrationEnabled)
                Handheld.Vibrate();
#endif
        }

        public void Clear()
        {
            foreach (Tween animation in _animations)
                animation.Kill();
            
            _animations.Clear();
            
            foreach (Image spark in _sparks)
                spark.gameObject.SetActive(false);
            
            foreach (Image ring in _rings)
                ring.gameObject.SetActive(false);
        }

        private Vector2 WorldToLayer(Vector3 position)
        {
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(_worldCamera, position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_effectsLayer, screenPosition, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera, out Vector2 localPosition);
            
            return localPosition;
        }

        private Vector2 TargetPosition()
        {
            Camera camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(camera, _target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_effectsLayer, screenPosition, camera, out Vector2 position);
            
            return position;
        }

        private void PlaySpark(Vector2 start, float delay, bool showRing, float intensity)
        {
            Image spark = Rent(_sparks, _sparkPrefab);
            RectTransform rect = spark.rectTransform;
            
            rect.sizeDelta = _sparkPrefab.rectTransform.sizeDelta;
            rect.localScale = _sparkPrefab.rectTransform.localScale * _sparkScale;
            spark.color = _sparkPrefab.color * _sparkColor;
            
            Vector2 scatter = Random.insideUnitCircle * _scatterRadius;
            Vector2 destination = TargetPosition();
            
            float duration = Mathf.Max(MinimumDuration, _flightDuration);
            float fadeDuration = Mathf.Clamp(_sparkFadeDuration, 0f, duration);
            Sequence animation = DOTween.Sequence();
            
            animation.AppendInterval(delay);
            animation.Append(DOTween.To(() => 0f, progress =>
            {
                Vector2 position = Vector2.Lerp(start, destination, progress);
                position += scatter * Mathf.Sin(progress * Mathf.PI);
                position.y += _arcHeight * Mathf.Sin(progress * Mathf.PI);
                rect.anchoredPosition = position;
            }, 1f, duration).SetEase(_flightEase));
            
            animation.Insert(delay, rect.DOScale(Vector3.zero, duration).SetEase(_sparkScaleEase));
            animation.Insert(delay + duration - fadeDuration, spark.DOFade(0f, fadeDuration));
            rect.anchoredPosition = start;
            
            animation.OnComplete(() =>
            {
                spark.gameObject.SetActive(false);
                if (showRing) PlayRing(intensity);
            });
            
            _animations.Add(animation);
        }

        private void PlayRing(float intensity)
        {
            Image ring = Rent(_rings, _ringPrefab);
            RectTransform rect = ring.rectTransform;
            
            rect.anchoredPosition = TargetPosition();
            rect.sizeDelta = _ringPrefab.rectTransform.sizeDelta;
            
            Vector3 scale = _ringPrefab.rectTransform.localScale * Mathf.Lerp(_minimumRingScale, _maximumRingScale, intensity);
            rect.localScale = scale * _initialRingScale;
            Color color = _ringPrefab.color * _ringColor;
            float alpha = color.a * Mathf.Lerp(_minimumRingAlpha, _maximumRingAlpha, intensity);
            color.a = 0f;
            ring.color = color;
            
            float duration = Mathf.Max(MinimumDuration, _ringDuration);
            float fadeInDuration = Mathf.Clamp(_ringFadeInDuration, 0f, duration);
            
            Sequence animation = DOTween.Sequence();
            
            animation.Append(ring.DOFade(alpha, fadeInDuration));
            animation.Append(ring.DOFade(0f, duration - fadeInDuration));
            animation.Insert(0f, rect.DOScale(scale * _finalRingScale, duration).SetEase(_ringScaleEase));
            animation.OnComplete(() => ring.gameObject.SetActive(false));
            
            _animations.Add(animation);
        }

        private Image Rent(List<Image> pool, Image prefab)
        {
            foreach (Image image in pool)
            {
                if (image.gameObject.activeSelf) 
                    continue;
                
                image.gameObject.SetActive(true);
                
                return image;
            }

            Image created = Instantiate(prefab, _effectsLayer, false);
            
            created.gameObject.SetActive(true);
            created.raycastTarget = false;
            pool.Add(created);
            
            return created;
        }

        private void OnDisable() => Clear();

        private void OnValidate()
        {
            _minimumSparks = Mathf.Max(1, _minimumSparks);
            _maximumSparks = Mathf.Max(_minimumSparks, _maximumSparks);
            _maximumRingScale = Mathf.Max(_minimumRingScale, _maximumRingScale);
            _maximumRingAlpha = Mathf.Max(_minimumRingAlpha, _maximumRingAlpha);
            _sparkFadeDuration = Mathf.Clamp(_sparkFadeDuration, 0f, _flightDuration);
            _ringFadeInDuration = Mathf.Clamp(_ringFadeInDuration, 0f, _ringDuration);
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
