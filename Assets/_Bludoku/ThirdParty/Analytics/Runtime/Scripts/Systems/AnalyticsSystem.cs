using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    public sealed class AnalyticsSystem : IAnalyticsEventSink
    {


        private readonly AnalyticsConfig _config;
        private readonly IAnalyticsStorageService _storage;

        private readonly Func<int, IAnalyticsProvider> _provider;
        private readonly Func<DateTime> _utcNow;
        private readonly Func<string> _language;
        private readonly string _version;
        private readonly string _platform;
        private readonly List<AnalyticsDiagnosticModel> _history = new List<AnalyticsDiagnosticModel>();
        private AnalyticsSaveData _data;
        private readonly bool _storageBlocked;
        public Guid SessionId { get; } = Guid.NewGuid();
        public bool IsFlushing { get; private set; }
        public IReadOnlyList<AnalyticsQueueEntryData> Queue => _data.Queue;
        public IReadOnlyList<AnalyticsDiagnosticModel> History => _history.AsReadOnly();
        public AnalyticsSystem(AnalyticsConfig config, IAnalyticsStorageService storage, Func<int, IAnalyticsProvider> provider,
            string version, string platform, Func<string> language, Func<DateTime> utcNow = null)
        {
            _config = config != null ? config : throw new ArgumentNullException(nameof(config)); _config.Validate();
            _provider = provider ?? throw new ArgumentNullException(nameof(provider)); _utcNow = utcNow ?? (() => DateTime.UtcNow);
            _version = version; _platform = platform; _language = language ?? throw new ArgumentNullException(nameof(language));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            try { string payload = _storage.Load(); _data = string.IsNullOrEmpty(payload) ? new AnalyticsSaveData() : JsonUtility.FromJson<AnalyticsSaveData>(payload); _data.Validate(); }
            catch (Exception error)
            {
                _data = new AnalyticsSaveData(); _storageBlocked = true;
                Diagnose(null, 0, EAnalyticsDeliveryStatus.StorageFailure, error.Message); return;
            }
            var configured = new HashSet<int>(); foreach (AnalyticsRouteConfig route in _config.Routes) configured.Add(route.Provider.Id.Value);
            try { Change(next =>
            {
                foreach (AnalyticsQueueEntryData entry in next.MutableQueue)
                    entry.MutableDeliveries.RemoveAll(delivery => !configured.Contains(delivery.ProviderId));
                next.MutableQueue.RemoveAll(entry => entry.Deliveries.Count == 0);
                while (next.MutableQueue.Count > _config.Capacity) next.MutableQueue.RemoveAt(0);
            }); }
            catch (Exception error) { Diagnose(null, 0, EAnalyticsDeliveryStatus.StorageFailure, error.Message); }
        }
        public bool Track(EAnalyticsEvent kind, EAnalyticsGroup group, IEnumerable<AnalyticsParameterData> parameters = null, int customId = 0)
        {
            AnalyticsEventData data = null;
            try
            {
                data = new AnalyticsEventData(SessionId, checked(_data.Sequence + 1), _utcNow(), _version, _platform, _language(), kind, group, parameters, customId);
                var deliveries = new List<AnalyticsDeliveryData>();
                foreach (AnalyticsRouteConfig route in _config.Routes)
                    if ((route.Groups & group) != 0) deliveries.Add(new AnalyticsDeliveryData(route.Provider.Id.Value));
                AnalyticsEventData evicted = _data.Queue.Count >= _config.Capacity ? _data.Queue[0].Event : null;
                Change(next =>
                {
                    next.Sequence = data.Sequence;
                    if (deliveries.Count == 0) return;
                    while (next.MutableQueue.Count >= _config.Capacity) next.MutableQueue.RemoveAt(0);
                    next.MutableQueue.Add(new AnalyticsQueueEntryData(data, deliveries));
                });
                if (evicted != null && deliveries.Count > 0) Diagnose(evicted, 0, EAnalyticsDeliveryStatus.Evicted, "Queue capacity reached; oldest event removed.");
                foreach (AnalyticsDeliveryData delivery in deliveries) Diagnose(data, delivery.ProviderId, EAnalyticsDeliveryStatus.Queued, string.Empty);
                return true;
            }
            catch (Exception error) { Diagnose(data, 0, EAnalyticsDeliveryStatus.StorageFailure, error.Message); return false; }
        }
        public async Task FlushAsync(CancellationToken token = default)
        {
            if (IsFlushing) return;
            IsFlushing = true;
            try
            {
                var tasks = new List<Task>();
                foreach (AnalyticsRouteConfig route in _config.Routes) tasks.Add(FlushProviderAsync(route.Provider.Id.Value, token));
                await Task.WhenAll(tasks);
            }
            finally { IsFlushing = false; }
        }
        private async Task FlushProviderAsync(int providerId, CancellationToken token)
        {
            // Bounded work per pass; each provider keeps FIFO while other providers progress independently.
            for (int count = 0; count < _config.Capacity; count++)
            {
                token.ThrowIfCancellationRequested();
                AnalyticsQueueEntryData entry = FindFirst(providerId);
                if (entry == null) return;
                AnalyticsDeliveryData delivery = FindDelivery(entry, providerId);
                if (delivery.NextUtcTicks > _utcNow().Ticks) return;
                try
                {
                    if (delivery.Attempts >= _config.MaximumAttempts)
                    { Complete(entry.Event, providerId, EAnalyticsDeliveryStatus.Exhausted, "Retry budget exhausted."); continue; }
                    IAnalyticsProvider provider = _provider(providerId);
                    if (provider == null || !provider.IsAvailable)
                    {
                        Schedule(entry.Event.Id, providerId, false);
                        Diagnose(entry.Event, providerId, EAnalyticsDeliveryStatus.Unavailable, "Waiting for provider availability."); return;
                    }
                    Schedule(entry.Event.Id, providerId, true);
                    bool success = false; string detail = string.Empty;
                    using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))

                    {
                        timeout.CancelAfter(_config.Timeout);
                        try { success = await AnalyticsTaskService.WithCancellationAsync(provider.SendAsync(entry.Event, timeout.Token), timeout.Token); }
                        catch (OperationCanceledException) when (!token.IsCancellationRequested) { detail = "Provider timeout."; }
                        catch (Exception error) when (!(error is OperationCanceledException)) { detail = error.Message; }
                    }
                    token.ThrowIfCancellationRequested();
                    if (success) { Complete(entry.Event, providerId, EAnalyticsDeliveryStatus.Delivered, string.Empty); continue; }
                    AnalyticsDeliveryData current = FindDelivery(Find(entry.Event.Id), providerId);
                    if (current == null) return; // Event may have been evicted while the provider was awaiting.
                    if (current.Attempts >= _config.MaximumAttempts)
                    { Complete(entry.Event, providerId, EAnalyticsDeliveryStatus.Exhausted, detail); continue; }
                    Schedule(entry.Event.Id, providerId, false);
                    Diagnose(entry.Event, providerId, EAnalyticsDeliveryStatus.Retry, detail); return;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception error) { Diagnose(entry.Event, providerId, EAnalyticsDeliveryStatus.StorageFailure, error.Message); return; }
            }
        }
        private void Schedule(Guid id, int providerId, bool attempted)
        {
            Change(next =>
            {
                AnalyticsDeliveryData delivery = FindDelivery(next.MutableQueue.Find(entry => entry.Event.Id == id), providerId);
                if (delivery == null) return;
                if (attempted) delivery.Attempts++;
                double delay = _config.RetryDelay.TotalSeconds * Math.Max(1, delivery.Attempts);
                delivery.NextUtcTicks = _utcNow().AddSeconds(delay).Ticks;
            });
        }
        private void Complete(AnalyticsEventData data, int providerId, EAnalyticsDeliveryStatus status, string detail)
        {
            Change(next =>
            {
                AnalyticsQueueEntryData entry = next.MutableQueue.Find(item => item.Event.Id == data.Id);
                if (entry == null) return;
                entry.MutableDeliveries.RemoveAll(delivery => delivery.ProviderId == providerId);
                if (entry.Deliveries.Count == 0) next.MutableQueue.Remove(entry);
            });
            Diagnose(data, providerId, status, detail);
        }
        private AnalyticsQueueEntryData FindFirst(int providerId)
        { foreach (AnalyticsQueueEntryData entry in _data.Queue) if (FindDelivery(entry, providerId) != null) return entry; return null; }
        private AnalyticsQueueEntryData Find(Guid id) => _data.MutableQueue.Find(entry => entry.Event.Id == id);
        private static AnalyticsDeliveryData FindDelivery(AnalyticsQueueEntryData entry, int providerId)
        { if (entry == null) return null; foreach (AnalyticsDeliveryData delivery in entry.Deliveries) if (delivery.ProviderId == providerId) return delivery; return null; }
        private void Change(Action<AnalyticsSaveData> mutation)
        {
            if (_storageBlocked) throw new InvalidOperationException("Analytics save is unreadable; preserve it for explicit restoration.");
            AnalyticsSaveData next = JsonUtility.FromJson<AnalyticsSaveData>(JsonUtility.ToJson(_data));
            mutation(next); next.Validate(); _storage.Save(JsonUtility.ToJson(next)); _data = next;
        }
        private void Diagnose(AnalyticsEventData data, int providerId, EAnalyticsDeliveryStatus status, string detail)
        {
            while (_history.Count >= _config.Capacity) _history.RemoveAt(0);
            _history.Add(new AnalyticsDiagnosticModel(data, providerId, status, detail));
        }
    }
}
