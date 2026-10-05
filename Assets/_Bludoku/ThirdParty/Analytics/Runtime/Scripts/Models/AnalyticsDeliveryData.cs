using System;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Serializable]
    public sealed class AnalyticsDeliveryData
    {
        [SerializeField] private int _providerId;
        [SerializeField] private int _attempts;
        [SerializeField] private long _nextUtcTicks;
        public int ProviderId => _providerId;
        public int Attempts { get => _attempts; internal set => _attempts = value; }
        public long NextUtcTicks { get => _nextUtcTicks; internal set => _nextUtcTicks = value; }
        internal AnalyticsDeliveryData(int providerId) { _providerId = providerId; }
    }
}
