using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    public sealed class ComboTextView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        private Sequence _sequence;

        public void Play(string message, Color textColor, ComboTextAnimationStruct animation)
        {
            _text.text = message;
            _text.color = textColor;
            
            Vector3 fullScale = transform.localScale;
            transform.localScale = fullScale * animation.StartScale;
            _text.alpha = 0f;
            
            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOScale(fullScale, animation.AppearDuration).SetEase(Ease.OutBack));
            _sequence.Join(DOTween.To(() => _text.alpha, value => _text.alpha = value, 1f, animation.AppearDuration));
            _sequence.AppendInterval(animation.HoldDuration);
            _sequence.Append(DOTween.To(() => _text.alpha, value => _text.alpha = value, 0f, animation.FadeDuration));
            _sequence.Join(transform.DOMoveY(transform.position.y + animation.RiseDistance, animation.FadeDuration));
            
            _sequence.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy() => _sequence?.Kill();
    }
}
