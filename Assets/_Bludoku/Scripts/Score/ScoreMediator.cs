using _Bludoku.Scripts.Boards;
using _Bludoku.Scripts.Combo;
using _Bludoku.Scripts.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Bludoku.Scripts.Score
{
    public class ScoreMediator : MonoBehaviour
    {
        [FormerlySerializedAs("scoreView"), SerializeField] private ScoreView _scoreView;
        [FormerlySerializedAs("board"), SerializeField] private Board _board;
        [FormerlySerializedAs("boosterView"), SerializeField] private ScoreBoosterView _boosterView;

        [SerializeField] private FiguresController _figuresController;

        [SerializeField] private GeneralComboConfig _generalComboConfig;
        [SerializeField] private DestructionComboConfig _destructionComboConfig;
        [SerializeField] private ScoreConfig _scoreConfig;

        [SerializeField] private GeneralComboView _generalComboView;
        [SerializeField] private DestructionComboView _destructionComboView;

        private readonly GeneralComboSystem _generalComboSystem = new();
        private readonly ComboSaveLoadService _comboSaveLoad = new();

        private DestructionComboSystem _destructionComboSystem;

        private void Awake()
        {
            ScoreSystem.LoadScore();
            _comboSaveLoad.Load(_generalComboSystem);

            _destructionComboSystem = new DestructionComboSystem(_destructionComboConfig);
        }

        private void OnEnable()
        {
            _board.OnFigurePlaced += FigurePlaced;
            _figuresController.OnHandCompleted += HandCompleted;
            _figuresController.OnHandReplaced += HandReplaced;
            _boosterView.SetCombo(_generalComboSystem.Combo);
        }

        private void Start()
        {
            _boosterView.SetCombo(_generalComboSystem.Combo);
            _scoreView.UpdateScore(false);
        }

        private void OnDisable()
        {
            _board.OnFigurePlaced -= FigurePlaced;
            _figuresController.OnHandCompleted -= HandCompleted;
            _figuresController.OnHandReplaced -= HandReplaced;
        }

        public void ResetScore()
        {
            _generalComboSystem.Reset();
            _comboSaveLoad.Save(_generalComboSystem);

            ScoreSystem.SetBoosterEnabled(false);
            ScoreSystem.ResetScore();

            _generalComboView.Clear();
            _destructionComboView.Clear();

            _boosterView.SetCombo(_generalComboSystem.Combo);
            _scoreView.UpdateScore(false);
        }

        private void FigurePlaced(ClearResult result)
        {
            _generalComboSystem.FigurePlaced(result.FiguresRemovedCount);
            _boosterView.SetCombo(_generalComboSystem.Combo);

            bool hasDestructionCombo = _destructionComboSystem.TryCalculate(result.FiguresRemovedCount, out var tier);
            float destructionMultiplier = hasDestructionCombo ? tier.Multiplier : 1f;

            ScoreSystem.AddScore(_scoreConfig.Calculate(result.ClearedCount, _generalComboConfig.GetMultiplier(_generalComboSystem.Combo), destructionMultiplier));

            _comboSaveLoad.Save(_generalComboSystem);
            _scoreView.UpdateScore();

            if (result.FiguresRemovedCount > 0)
                _generalComboView.Show(_generalComboSystem.Combo);

            if (hasDestructionCombo)
                _destructionComboView.Show(tier, result.PlacementPosition);
        }

        private void HandCompleted()
        {
            _generalComboSystem.CompleteHand();
            _boosterView.SetCombo(_generalComboSystem.Combo);
            _comboSaveLoad.Save(_generalComboSystem);

            if (_generalComboSystem.Combo == 0)
                _generalComboView.Clear();
        }

        private void HandReplaced()
        {
            _generalComboSystem.BeginReplacementHand();
            _comboSaveLoad.Save(_generalComboSystem);
        }
    }
}
