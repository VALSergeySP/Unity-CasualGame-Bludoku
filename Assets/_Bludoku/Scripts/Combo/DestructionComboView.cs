using System.Collections.Generic;
using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    public sealed class DestructionComboView : MonoBehaviour
    {
        [SerializeField] private ComboTextView _textPrefab;
        [SerializeField] private DestructionComboConfig _config;
        [SerializeField] private CameraShakeView _cameraShakeView;
        private readonly List<ComboTextView> _activeTexts = new();

        public void Show(DestructionComboTierStruct tier, Vector3 position)
        {
            _activeTexts.RemoveAll(text => text == null);
            
            ComboTextView text = Instantiate(_textPrefab, position + _config.Animation.Offset, Quaternion.identity);
            
            _activeTexts.Add(text);
            
            text.Play(tier.Text, tier.TextColor, _config.Animation);
            
            _cameraShakeView.Play(tier);
        }

        public void Clear()
        {
            foreach (ComboTextView text in _activeTexts)
                if (text != null) Destroy(text.gameObject);
            _activeTexts.Clear();
            _cameraShakeView.Stop();
        }

        private void OnDisable() => Clear();
    }
}
