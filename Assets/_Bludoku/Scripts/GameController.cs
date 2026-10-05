using System;
using _Bludoku.Scripts.Boards;
using _Bludoku.Scripts.Core;
using _Bludoku.Scripts.Score;
using _Bludoku.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Bludoku.Scripts
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        public event Action<bool> OnRunStarted;
        public event Action OnRevived;
        public event Action OnGameOver;

        [FormerlySerializedAs("scoreMediator"), SerializeField] private ScoreMediator _scoreMediator;
        [FormerlySerializedAs("uiMediator"), SerializeField] private UIMediator _uiMediator;
        [FormerlySerializedAs("board"), SerializeField] private Board _board;
        [FormerlySerializedAs("figuresController"), SerializeField] private FiguresController _figuresController;

        public ScoreMediator ScoreMediator => _scoreMediator;
        public FiguresController FiguresController => _figuresController;
        public bool HasStarted { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool WasRestored => _board.WasRestored;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            _figuresController.OnGameOver += HandleGameOver;
            _board.LoadGrid();
            HasStarted = true;
            OnRunStarted?.Invoke(_board.WasRestored);
            _figuresController.LoadFigures();
        }

        public void NewGame()
        {
            BoardSaveLoad.Delete();
            _board.ResetBoard();
            _uiMediator.HideGameOver();
            _scoreMediator.ResetScore();
            IsGameOver = false;
            OnRunStarted?.Invoke(false);
            _figuresController.ResetFigures();
        }

        public void SecondChance()
        {
            if (!IsGameOver) return;
            _uiMediator.HideGameOver();
            _figuresController.UpdateToEasyFigures();
            IsGameOver = false;
            OnRevived?.Invoke();
        }

        private void HandleGameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            _uiMediator.ShowGameOver();
            OnGameOver?.Invoke();
        }

        private void OnDestroy()
        {
            _figuresController.OnGameOver -= HandleGameOver;
            if (Instance == this) Instance = null;
        }
    }
}