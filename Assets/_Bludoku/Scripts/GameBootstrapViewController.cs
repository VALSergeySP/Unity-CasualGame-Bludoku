using System;
using _Bludoku.Scripts.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Bludoku.Scripts
{
    public sealed class GameBootstrapViewController : MonoBehaviour
    {
        private const int DefaultInitialSceneIndex = 1;
        [SerializeField] private GameplayAnalyticsViewController _analytics;
        [SerializeField, Min(0)] private int _initialSceneIndex = DefaultInitialSceneIndex;

        private async void Start()
        {
            try
            {
                if (_analytics == null) throw new InvalidOperationException("Assign the analytics controller.");
                await _analytics.InitializeAsync();
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                if (_analytics != null)
                {
                    try { await _analytics.ShutdownAsync(); }
                    catch (Exception shutdownError) { Debug.LogException(shutdownError); }
                }
            }

            // Analytics failure must not prevent the game from starting.
            if (this != null) SceneManager.LoadSceneAsync(_initialSceneIndex);
        }
    }
}