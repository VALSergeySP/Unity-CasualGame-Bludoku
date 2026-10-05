using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameBrewStudio.CoreTemplate.Analytics;
namespace GameBrewStudio.CoreTemplate.Samples
{
    public sealed class SampleAnalyticsProvider : IAnalyticsProvider, IAttributionProvider
    {
        private const int HistoryCapacity = 500;
        private bool _initialized;
        private readonly List<AnalyticsEventData> _received = new List<AnalyticsEventData>();
        public ESampleAnalyticsResponse Response { get; set; }
        public bool IsAvailable => _initialized && Response != ESampleAnalyticsResponse.Unavailable;
        public IReadOnlyList<AnalyticsEventData> Received => _received.AsReadOnly();
        public SampleAnalyticsProvider(ESampleAnalyticsResponse response) { Response = response; }
        public Task InitializeAsync(CancellationToken token) { token.ThrowIfCancellationRequested(); _initialized = true; return Task.CompletedTask; }
        public Task<bool> SendAsync(AnalyticsEventData data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!IsAvailable || Response == ESampleAnalyticsResponse.Failure) return Task.FromResult(false);
            if (!_received.Exists(previous => previous.Id == data.Id))
            { while (_received.Count >= HistoryCapacity) _received.RemoveAt(0); _received.Add(data); }
            return Task.FromResult(true);
        }
        public Task<AttributionData> GetAttributionAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!IsAvailable || Response == ESampleAnalyticsResponse.Failure) throw new InvalidOperationException("Simulated attribution unavailable.");
            return Task.FromResult(new AttributionData("test-source", "test-campaign", "test-ad-group", "test-creative"));
        }
        public void Dispose() { _initialized = false; }
    }
}
