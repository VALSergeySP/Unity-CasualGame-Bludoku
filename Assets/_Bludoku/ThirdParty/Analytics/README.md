# Independent Unity analytics

The runtime assembly depends only on Unity and the standard .NET libraries.
No SDKIntegration, Saves, DI, game systems or UniTask packages are required.

## SDK adapters

Implement IAnalyticsProvider (SendAsync) and optionally IAttributionProvider
(GetAttributionAsync). Both inherit IAnalyticsSdkProvider, which defines
IsAvailable, InitializeAsync and Dispose. Implement AnalyticsProviderConfig
and its CreateProvider method to expose an SDK adapter in the Inspector.
Give each provider a stable positive numeric ID; routes use that ID for persisted deliveries.

The host owns SDK initialization and disposal. AnalyticsSystem receives a
Func<int, IAnalyticsProvider> resolver; return null while an adapter is unavailable.
Call Track to queue typed events and await FlushAsync periodically.
Retries, bounded queue capacity, per-provider FIFO, independent delivery, diagnostics
and cancellation timeouts are handled by AnalyticsSystem.

## Storage

Pass IAnalyticsStorageService to the AnalyticsSystem constructor.
MemoryAnalyticsStorageService provides session-only storage.
FileAnalyticsStorageService accepts an explicit path, such as a path under
Application.persistentDataPath, and atomically replaces saved queue JSON.
A custom storage adapter must commit a whole payload or throw without changing
the previous payload. Unreadable storage is preserved and blocks new writes.

Example construction:

    var analytics = new AnalyticsSystem(config,
        new FileAnalyticsStorageService(queuePath),
        id => ResolveInitializedProvider(id),
        Application.version, Application.platform.ToString(),
        () => Application.systemLanguage.ToString());

## Hosting and samples

Create and call the system on Unity's main thread; await it from Unity's
synchronization context. Its mutable queue and Unity JSON serialization are
not intended for concurrent calls from worker threads.
SDK adapters that require Unity APIs must marshal callbacks to that context.
A timed-out adapter may continue its SDK operation if the SDK ignores cancellation;
adapters should deduplicate deliveries by AnalyticsEventData.Id.

Samples have separate runtime/editor assemblies. SampleAnalyticsModuleSystem
is an optional plain C# host: initialize it, call FlushAsync periodically, call
ReadAttributionAsync when needed, and dispose after outstanding operations finish.
It no longer subscribes to game-specific Economy, GameFlow, UI or Lifecycle systems.
Game integrations call Track directly using enums and AnalyticsParameterData.
The Analytics Events editor window inspects the active sample module.

The constructor previously accepting SaveSystem now accepts IAnalyticsStorageService.
SDK provider configs now derive from AnalyticsProviderConfig. Existing provider _id
values and sample asset references are retained. Importing old SaveSystem envelopes
requires a storage adapter that extracts the analytics JSON payload.