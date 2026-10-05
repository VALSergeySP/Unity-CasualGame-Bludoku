using System;
using _Bludoku.Scripts.Boards;
using _Bludoku.Scripts.Combo;
using _Bludoku.Scripts.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Bludoku.Scripts.Score
{
    public class ScoreMediator : MonoBehaviour
    {
        public event Action<ScorePlacementDataStruct> OnPlacementScored;
        public event Action<int, int> OnComboChanged;

        private const float BaseMultiplier = 1f;
        [FormerlySerializedAs("scoreView"), SerializeField] private ScoreView _scoreView;
        [FormerlySerializedAs("board"), SerializeField] private Board _board;
        [FormerlySerializedAs("boosterView"), SerializeField] private ScoreBoosterView _boosterView;

        [SerializeField] private FiguresController _figuresController;

        [SerializeField] private GeneralComboConfig _generalComboConfig;
        [SerializeField] private DestructionComboConfig _destructionComboConfig;
        [SerializeField] private ScoreConfig _scoreConfig;

        [SerializeField] private GeneralComboView _generalComboView;
        [SerializeField] private DestructionComboView _destructionComboView;
        [SerializeField] private ComboEffectsView _comboEffectsView;

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
            _comboEffectsView.Clear();
        }

        public void ResetScore()
        {
            _generalComboSystem.Reset();
            _comboSaveLoad.Save(_generalComboSystem);

            ScoreSystem.SetBoosterEnabled(false);
            ScoreSystem.ResetScore();

            _generalComboView.Clear();
            _destructionComboView.Clear();
            _comboEffectsView.Clear();

            _boosterView.SetCombo(_generalComboSystem.Combo);
            _scoreView.UpdateScore(false);
        }

        private void FigurePlaced(ClearResult result)
        {
            int previousCombo = _generalComboSystem.Combo;
            _generalComboSystem.FigurePlaced(result.FiguresRemovedCount);
            _boosterView.SetCombo(_generalComboSystem.Combo);

            bool hasDestructionCombo = _destructionComboSystem.TryCalculate(result.FiguresRemovedCount, out var tier);
            float destructionMultiplier = hasDestructionCombo ? tier.Multiplier : BaseMultiplier;

            float generalMultiplier = _generalComboConfig.GetMultiplier(_generalComboSystem.Combo);
            int scoreGained = _scoreConfig.Calculate(result.ClearedCount, generalMultiplier, destructionMultiplier);
            int baseScore = _scoreConfig.Calculate(result.ClearedCount, BaseMultiplier, BaseMultiplier);
            ScoreSystem.AddScore(scoreGained);

            _comboSaveLoad.Save(_generalComboSystem);
            _scoreView.UpdateScore();

            if (result.FiguresRemovedCount > 0)
            {
                _generalComboView.Show(_generalComboSystem.Combo);
                _comboEffectsView.Play(_generalComboSystem.Combo, result.ClearedPositions);
            }

            if (hasDestructionCombo)
                _destructionComboView.Show(tier, result.PlacementPosition);

            OnPlacementScored?.Invoke(new ScorePlacementDataStruct(
                result.PieceId, result.Column, result.Row, result.ClearedCount, result.FiguresRemovedCount,
                previousCombo, _generalComboSystem.Combo, generalMultiplier, destructionMultiplier,
                hasDestructionCombo, ScoreSystem.Score, scoreGained, Math.Max(0, scoreGained - baseScore)));
        }

        private void HandCompleted()
        {
            int previousCombo = _generalComboSystem.Combo;
            _generalComboSystem.CompleteHand();
            _boosterView.SetCombo(_generalComboSystem.Combo);
            _comboSaveLoad.Save(_generalComboSystem);

            OnComboChanged?.Invoke(previousCombo, _generalComboSystem.Combo);

            if (_generalComboSystem.Combo == 0)
            {
                _generalComboView.Clear();
                _comboEffectsView.Clear();
            }
        }

        private void HandReplaced()
        {
            _generalComboSystem.BeginReplacementHand();
            _comboSaveLoad.Save(_generalComboSystem);
        }
    }
}
