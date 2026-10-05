using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using _Bludoku.Scripts.MainMenu;
using GameBrewStudio.CoreTemplate.Analytics;
using GameBrewStudio.CoreTemplate.Samples;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Bludoku.Scripts.Analytics
{
    // Scene-owned lifetime. The bootstrap explicitly calls InitializeAsync.
    public sealed class GameplayAnalyticsViewController : MonoBehaviour
    {
        private const string QueueFileName = "analytics-queue.json";
        private const float DefaultFlushIntervalSeconds = 1f;
        
        [SerializeField] private AnalyticsConfig _config;
        [SerializeField, Min(0.01f)] private float _flushIntervalSeconds = DefaultFlushIntervalSeconds;

        private CancellationTokenSource _lifetime;
        private SampleAnalyticsModuleSystem _module;
        private GameplayAnalyticsIntegrationSystem _integration;
        private Task _initialization;
        private Task _pump = Task.CompletedTask;
        private Task _shutdown;
        private bool _applicationEnded;

        public GameplayAnalyticsSystem Gameplay { get; private set; }
        public AnalyticsSystem Analytics => _module?.System;

        public Task InitializeAsync()
        {
            if (_shutdown != null) 
                throw new InvalidOperationException("Analytics has already stopped.");
            
            return _initialization ?? (_initialization = InitializeModuleAsync());
        }

        private async Task InitializeModuleAsync()
        {
            if (_config == null) 
                throw new InvalidOperationException("Assign the analytics config in the Bootstrap scene.");
            
            _config.Validate();
            
            if (float.IsNaN(_flushIntervalSeconds) || float.IsInfinity(_flushIntervalSeconds) || _flushIntervalSeconds <= 0)
                throw new InvalidOperationException("Analytics flush interval must be positive.");

            _lifetime = new CancellationTokenSource();
            _module = new SampleAnalyticsModuleSystem(_config,
                new FileAnalyticsStorageService(Path.Combine(Application.persistentDataPath, QueueFileName)),
                Application.version, Application.platform.ToString(), () => Application.systemLanguage.ToString());
            Gameplay = new GameplayAnalyticsSystem(_module.System);
            await _module.InitializeAsync(_lifetime.Token);
            _lifetime.Token.ThrowIfCancellationRequested();

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneLoaded;
            _pump = PumpAsync(_lifetime.Token);
        }

        private async Task PumpAsync(CancellationToken token)
        {
            try
            {
                try
                {
                    await _module.ReadAttributionAsync(token);
                }
                catch (Exception error) when (!(error is OperationCanceledException))
                {
                    Debug.LogException(error);
                }

                while (!token.IsCancellationRequested)
                {
                    await _module.FlushAsync(token);
                    await Task.Delay(TimeSpan.FromSeconds(_flushIntervalSeconds), token);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) 
                return;
            
            _integration?.Dispose();
            _integration = null;
            GameController game = null;
            bool isMainMenu = false;
            
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (game == null) 
                    game = root.GetComponentInChildren<GameController>(true);
                
                isMainMenu |= root.GetComponentInChildren<MainMenuManager>(true) != null;
            }
            Gameplay.EndRun(isMainMenu ? EGameplayExitReason.MainMenu : EGameplayExitReason.SceneChanged);
            
            if (game != null) 
                _integration = new GameplayAnalyticsIntegrationSystem(game, Gameplay);
        }

        public Task ShutdownAsync()
        {
            return _shutdown ?? (_shutdown = StopModuleAsync());
        }

        private async Task StopModuleAsync()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            _integration?.Dispose();
            _integration = null;
            EndApplication();
            _lifetime?.Cancel();
            try
            {
                if (_initialization != null) 
                    await _initialization;
                
                await _pump;
            }
            finally
            {
                _module?.Dispose();
                _lifetime?.Dispose();
            }
        }

        private void EndApplication()
        {
            if (_applicationEnded || Analytics == null) 
                return;
            
            _applicationEnded = true;
            Gameplay.EndRun(EGameplayExitReason.ApplicationQuit);
            Analytics.Track(EAnalyticsEvent.ApplicationEnded, EAnalyticsGroup.Application);
        }

        private void OnApplicationQuit()
        {
            EndApplication();
            _lifetime?.Cancel();
        }

        private async void OnDestroy()
        {
            try
            {
                await ShutdownAsync();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
        }
    }
}