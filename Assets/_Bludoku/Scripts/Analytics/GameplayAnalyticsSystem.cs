using System;
using System.Collections.Generic;
using System.Diagnostics;
using _Bludoku.Scripts.Score;
using GameBrewStudio.CoreTemplate.Analytics;

namespace _Bludoku.Scripts.Analytics
{
    // Event schema and run state live here, independently of Unity views and SDKs.
    public sealed class GameplayAnalyticsSystem
    {
        private readonly IAnalyticsEventSink _sink;
        private readonly Func<double> _seconds;
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        
        private Guid _runId;
        private double _startedAt;
        private int _moves;
        private int _revives;
        private int _score;
        private bool _active;
        private bool _gameOver;

        public GameplayAnalyticsSystem(IAnalyticsEventSink sink, Func<double> seconds = null)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _seconds = seconds ?? (() => _clock.Elapsed.TotalSeconds);
        }

        public void StartRun(bool restored, int score)
        {
            EndRun(EGameplayExitReason.Restart);
            
            _runId = Guid.NewGuid();
            _startedAt = _seconds();
            _moves = 0; _revives = 0; _score = score;
            _active = true; _gameOver = false;
            
            var parameters = Context();
            parameters.Add(Boolean(EGameplayAnalyticsParameter.IsRestored, restored));
            
            Send(EAnalyticsEvent.LevelStarted, EAnalyticsGroup.Gameplay, parameters);
            
            if (restored) 
                SendCustom(EGameplayAnalyticsEvent.GameResumed, Context());
        }

        public void PiecePlaced(ScorePlacementDataStruct data)
        {
            if (!_active || _gameOver) 
                return;
            
            _moves++; _score = data.Score;
            
            var parameters = Context();
            parameters.Add(Integer(EGameplayAnalyticsParameter.PieceId, data.PieceId));
            parameters.Add(Integer(EGameplayAnalyticsParameter.Column, data.Column));
            parameters.Add(Integer(EGameplayAnalyticsParameter.Row, data.Row));
            parameters.Add(Integer(EGameplayAnalyticsParameter.ClearedCells, data.ClearedCells));
            parameters.Add(Integer(EGameplayAnalyticsParameter.ClearedAreas, data.ClearedAreas));
            parameters.Add(Integer(EGameplayAnalyticsParameter.ScoreGained, data.ScoreGained));
            
            SendCustom(EGameplayAnalyticsEvent.PiecePlaced, parameters);
            
            if (data.Combo > data.PreviousCombo)
                SendCustom(EGameplayAnalyticsEvent.ComboIncreased, ComboParameters(data));
            
            if (data.HasDestructionCombo)
                SendCustom(EGameplayAnalyticsEvent.DestructionCombo, ComboParameters(data));
            
            if (data.BonusPoints > 0)
                BonusReceived(EGameplayBonus.ComboScore, data.BonusPoints);
        }

        public void PlacementRejected(int pieceId)
        {
            if (!_active || _gameOver) 
                return;
            
            var parameters = Context();
            parameters.Add(Integer(EGameplayAnalyticsParameter.PieceId, pieceId));
            
            SendCustom(EGameplayAnalyticsEvent.PlacementRejected, parameters);
        }

        public void HandCompleted() => TrackActive(EGameplayAnalyticsEvent.HandCompleted);

        public void HandReplaced()
        {
            if (_active) 
                SendCustom(EGameplayAnalyticsEvent.HandReplaced, Context());
        }

        public void ComboChanged(int previous, int current)
        {
            if (!_active || previous <= 0 || current != 0) 
                return;
            
            var parameters = Context();
            parameters.Add(Integer(EGameplayAnalyticsParameter.Combo, previous));
            
            SendCustom(EGameplayAnalyticsEvent.ComboEnded, parameters);
        }

        public void GameOver(int score)
        {
            if (!_active || _gameOver) 
                return;
            
            _score = score; _gameOver = true;
            
            SendCustom(EGameplayAnalyticsEvent.GameOver, Context());
        }

        public void Revived()
        {
            if (!_active || !_gameOver) 
                return;
            
            _gameOver = false; _revives++;
            
            Send(EAnalyticsEvent.Revived, EAnalyticsGroup.Gameplay, Context());
        }

        public void BonusReceived(EGameplayBonus bonus, long amount)
        {
            if (!_active || amount <= 0) 
                return;
            
            var parameters = Context();
            parameters.Add(StandardInteger(EAnalyticsParameter.ResourceId, (int)bonus));
            parameters.Add(StandardInteger(EAnalyticsParameter.Amount, amount));
            
            Send(EAnalyticsEvent.ResourceReceived, EAnalyticsGroup.Economy, parameters);
        }

        public void PowerUpUsed(EGameplayPowerUp powerUp)
        {
            if (!_active) 
                return;
            
            var parameters = Context();
            parameters.Add(StandardInteger(EAnalyticsParameter.ResourceId, (int)powerUp));
            
            Send(EAnalyticsEvent.BoosterUsed, EAnalyticsGroup.Economy, parameters);
        }

        public void EndRun(EGameplayExitReason reason)
        {
            if (!_active) 
                return;
            
            var parameters = Context();
            parameters.Add(Integer(EGameplayAnalyticsParameter.ExitReason, (int)reason));
            parameters.Add(Number(EAnalyticsParameter.DurationSeconds, Math.Max(0, _seconds() - _startedAt)));
            
            Send(EAnalyticsEvent.LevelExited, EAnalyticsGroup.Gameplay, parameters);
            
            _active = false;
        }

        private void TrackActive(EGameplayAnalyticsEvent kind)
        {
            if (_active && !_gameOver) 
                SendCustom(kind, Context());
        }

        private List<AnalyticsParameterData> Context() => new List<AnalyticsParameterData>
        {
            new AnalyticsParameterData(new AnalyticsParameterIdStruct(EAnalyticsParameter.RunId), _runId.ToString("N")),
            Integer(EGameplayAnalyticsParameter.Score, _score),
            Integer(EGameplayAnalyticsParameter.Moves, _moves),
            Integer(EGameplayAnalyticsParameter.Revives, _revives)
        };

        private List<AnalyticsParameterData> ComboParameters(ScorePlacementDataStruct data)
        {
            var parameters = Context();
            parameters.Add(Integer(EGameplayAnalyticsParameter.Combo, data.Combo));
            parameters.Add(Integer(EGameplayAnalyticsParameter.ClearedAreas, data.ClearedAreas));
            parameters.Add(new AnalyticsParameterData(CustomId(EGameplayAnalyticsParameter.GeneralMultiplier), (double)data.GeneralMultiplier));
            parameters.Add(new AnalyticsParameterData(CustomId(EGameplayAnalyticsParameter.DestructionMultiplier), (double)data.DestructionMultiplier));
           
            return parameters;
        }

        private void SendCustom(EGameplayAnalyticsEvent kind, List<AnalyticsParameterData> parameters) =>
            _sink.Track(EAnalyticsEvent.Custom, EAnalyticsGroup.Gameplay, parameters, (int)kind);

        private void Send(EAnalyticsEvent kind, EAnalyticsGroup group, List<AnalyticsParameterData> parameters) =>
            _sink.Track(kind, group, parameters);

        private static AnalyticsParameterIdStruct CustomId(EGameplayAnalyticsParameter key) =>
            AnalyticsParameterIdStruct.Custom((int)key);
        
        private static AnalyticsParameterData Integer(EGameplayAnalyticsParameter key, long value) =>
            new AnalyticsParameterData(CustomId(key), value);
        
        private static AnalyticsParameterData Boolean(EGameplayAnalyticsParameter key, bool value) =>
            new AnalyticsParameterData(CustomId(key), value);
        
        private static AnalyticsParameterData StandardInteger(EAnalyticsParameter key, long value) =>
            new AnalyticsParameterData(new AnalyticsParameterIdStruct(key), value);
        
        private static AnalyticsParameterData Number(EAnalyticsParameter key, double value) =>
            new AnalyticsParameterData(new AnalyticsParameterIdStruct(key), value);
    }
}