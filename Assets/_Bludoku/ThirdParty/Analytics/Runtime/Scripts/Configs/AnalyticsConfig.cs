using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [CreateAssetMenu(menuName = "GameBrewStudio/Core Template/Analytics/Module")]
    public sealed class AnalyticsConfig : ScriptableObject
    {
        private const int DefaultCapacity = 500;
        private const int DefaultAttempts = 3;
        private const float DefaultDelay = 5f;
        private const float DefaultTimeout = 10f;
        [SerializeField] private AnalyticsRouteConfig[] _routes = Array.Empty<AnalyticsRouteConfig>();
        [SerializeField] private AnalyticsProviderConfig _attributionProvider;
        [SerializeField, Min(1)] private int _capacity = DefaultCapacity;
        [SerializeField, Min(1)] private int _maximumAttempts = DefaultAttempts;
        [SerializeField, Min(0)] private float _retrySeconds = DefaultDelay;
        [SerializeField, Min(0.01f)] private float _timeoutSeconds = DefaultTimeout;
        public IReadOnlyList<AnalyticsRouteConfig> Routes => Array.AsReadOnly(_routes);
        public AnalyticsProviderConfig AttributionProvider => _attributionProvider;
        public int Capacity => _capacity;
        public int MaximumAttempts => _maximumAttempts;
        public TimeSpan RetryDelay => TimeSpan.FromSeconds(_retrySeconds);
        public TimeSpan Timeout => TimeSpan.FromSeconds(_timeoutSeconds);
        public void Validate()
        {
            if (_routes == null || _capacity <= 0 || _maximumAttempts <= 0 || !float.IsFinite(_retrySeconds) || _retrySeconds < 0 ||
                !float.IsFinite(_timeoutSeconds) || _timeoutSeconds <= 0) throw new ArgumentException("Invalid analytics configuration.");
            var ids = new HashSet<AnalyticsProviderIdStruct>();
            for (int index = 0; index < _routes.Length; index++)
            {
                AnalyticsRouteConfig route = _routes[index];
                if (route == null)
                    throw new ArgumentException($"Analytics config '{name}' has a missing route at index {index}.");
                if (route.Provider == null)
                    throw new ArgumentException($"Analytics route '{route.name}' has no provider.");
                if (!ids.Add(route.Provider.Id))
                    throw new ArgumentException($"Analytics route '{route.name}' duplicates provider ID {route.Provider.Id}.");
                if ((route.Groups & ~EAnalyticsGroup.All) != 0)
                    throw new ArgumentException($"Analytics route '{route.name}' has unsupported group flags: {route.Groups}.");
            }
        }
    }
}
