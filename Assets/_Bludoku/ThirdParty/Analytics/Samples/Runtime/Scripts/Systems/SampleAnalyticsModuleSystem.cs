using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameBrewStudio.CoreTemplate.Analytics;

namespace GameBrewStudio.CoreTemplate.Samples
{
    // The host owns initialization, periodic flushing, cancellation and disposal.
    public sealed class SampleAnalyticsModuleSystem : IDisposable
    {
        private readonly Dictionary<int, IAnalyticsSdkProvider> _providers = new Dictionary<int, IAnalyticsSdkProvider>();
        public static SampleAnalyticsModuleSystem Active { get; private set; }
        public AnalyticsSystem System { get; }
        public AnalyticsConfig Config { get; }
        public AttributionData Attribution { get; private set; }
        public string AttributionStatus { get; private set; } = "Waiting for provider";
        public SampleAnalyticsModuleSystem(AnalyticsConfig config, IAnalyticsStorageService storage,
            string version, string platform, Func<string> language)
        {
            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            Config.Validate();
            try
            {
                foreach (AnalyticsRouteConfig route in config.Routes) AddProvider(route.Provider);
                if (config.AttributionProvider != null) AddProvider(config.AttributionProvider);
                System = new AnalyticsSystem(config, storage,
                    id => _providers.TryGetValue(id, out IAnalyticsSdkProvider provider) ? provider as IAnalyticsProvider : null,
                    version, platform, language);
            }
            catch { Dispose(); throw; }
        }
        private void AddProvider(AnalyticsProviderConfig config)
        {
            if (_providers.ContainsKey(config.Id.Value)) return;
            _providers.Add(config.Id.Value, config.CreateProvider() ?? throw new InvalidOperationException("Provider factory returned null."));
        }
        public async Task InitializeAsync(CancellationToken token = default)
        {
            foreach (IAnalyticsSdkProvider provider in _providers.Values) await provider.InitializeAsync(token);
            Active = this;
            System.Track(EAnalyticsEvent.ApplicationStarted, EAnalyticsGroup.Application);
        }
        public SampleAnalyticsProvider GetProvider(int id) =>
            _providers.TryGetValue(id, out IAnalyticsSdkProvider provider) ? provider as SampleAnalyticsProvider : null;
        public Task FlushAsync(CancellationToken token = default) => System.FlushAsync(token);
        public async Task ReadAttributionAsync(CancellationToken token = default)
        {
            if (Config.AttributionProvider == null) return;
            if (!(_providers[Config.AttributionProvider.Id.Value] is IAttributionProvider provider) || !provider.IsAvailable) return;
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                timeout.CancelAfter(Config.Timeout);
                Attribution = await AnalyticsTaskService.WithCancellationAsync(provider.GetAttributionAsync(timeout.Token), timeout.Token);
                if (Attribution == null) throw new InvalidOperationException("No attribution data.");
                bool tracked = System.Track(EAnalyticsEvent.AttributionReceived, EAnalyticsGroup.Attribution, new[] {
                    SampleAnalyticsIntegrationSystem.Text(EAnalyticsParameter.Source, Attribution.Source),
                    SampleAnalyticsIntegrationSystem.Text(EAnalyticsParameter.Campaign, Attribution.Campaign),
                    SampleAnalyticsIntegrationSystem.Text(EAnalyticsParameter.AdGroup, Attribution.AdGroup),
                    SampleAnalyticsIntegrationSystem.Text(EAnalyticsParameter.Creative, Attribution.Creative) });
                AttributionStatus = tracked ? "Received attribution" : "Could not persist attribution event";
            }
        }
        public void Dispose()
        {
            foreach (IAnalyticsSdkProvider provider in _providers.Values) provider.Dispose();
            _providers.Clear();
            if (Active == this) Active = null;
        }
    }
}