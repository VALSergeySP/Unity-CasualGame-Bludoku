using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    public sealed class GeneralComboView : MonoBehaviour
    {
        private const string ComboPrefix = "Combo ";
        private const float ViewportCenter = 0.5f;
        
        [SerializeField] private ComboTextView _textPrefab;
        [SerializeField] private Color _textColor;
        [SerializeField] private Camera _camera;
        
        [SerializeField] private GeneralComboConfig _config;
        
        private ComboTextView _activeText;

        public void Show(int combo)
        {
            Clear();
            
            if (combo < _config.MinimumVisibleCombo) 
                return;
            
            Vector3 center = _camera.ViewportToWorldPoint(new Vector3(ViewportCenter, ViewportCenter, -_camera.transform.position.z));
            
            _activeText = Instantiate(_textPrefab, center + _config.Animation.Offset, Quaternion.identity);
            _activeText.Play(ComboPrefix + combo, _textColor, _config.Animation);
        }

        public void Clear()
        {
            if (_activeText != null) Destroy(_activeText.gameObject);
        }

        private void OnDisable() => Clear();
    }
}
