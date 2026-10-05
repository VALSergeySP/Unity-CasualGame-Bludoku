using System;
using _Bludoku.Scripts.Core;
using _Bludoku.Scripts.Score;

namespace _Bludoku.Scripts.Analytics
{
    // Add new subscriptions here. Gameplay publishes domain notifications, never SDK calls.
    public sealed class GameplayAnalyticsIntegrationSystem : IDisposable
    {
        private readonly GameController _game;
        private readonly FiguresController _figures;
        private readonly ScoreMediator _score;
        private readonly GameplayAnalyticsSystem _analytics;

        public GameplayAnalyticsIntegrationSystem(GameController game, GameplayAnalyticsSystem analytics)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _analytics = analytics ?? throw new ArgumentNullException(nameof(analytics));
            _figures = game.FiguresController;
            _score = game.ScoreMediator;
            
            _game.OnRunStarted += RunStarted;
            _game.OnGameOver += GameOver;
            _game.OnRevived += Revived;
            _score.OnPlacementScored += _analytics.PiecePlaced;
            _score.OnComboChanged += _analytics.ComboChanged;
            _figures.OnPlacementRejected += _analytics.PlacementRejected;
            _figures.OnHandCompleted += _analytics.HandCompleted;
            _figures.OnFiguresReplaced += HandReplaced;
            
            if (game.HasStarted)
            {
                RunStarted(game.WasRestored);
                
                if (game.IsGameOver) 
                    GameOver();
            }
        }

        private void RunStarted(bool restored) => _analytics.StartRun(restored, ScoreSystem.Score);
        private void GameOver() => _analytics.GameOver(ScoreSystem.Score);
        private void Revived() => _analytics.Revived();
        
        private void HandReplaced()
        {
            _analytics.HandReplaced();
            _analytics.PowerUpUsed(EGameplayPowerUp.ReplaceHand);
        }

        public void Dispose()
        {
            _game.OnRunStarted -= RunStarted;
            _game.OnGameOver -= GameOver;
            _game.OnRevived -= Revived;
            _score.OnPlacementScored -= _analytics.PiecePlaced;
            _score.OnComboChanged -= _analytics.ComboChanged;
            _figures.OnPlacementRejected -= _analytics.PlacementRejected;
            _figures.OnHandCompleted -= _analytics.HandCompleted;
            _figures.OnFiguresReplaced -= HandReplaced;
        }
    }
}