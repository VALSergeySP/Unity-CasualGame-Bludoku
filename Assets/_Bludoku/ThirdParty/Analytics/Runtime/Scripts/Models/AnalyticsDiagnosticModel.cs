namespace GameBrewStudio.CoreTemplate.Analytics
{
    public sealed class AnalyticsDiagnosticModel
    {
        public AnalyticsEventData Event { get; }
        public int ProviderId { get; }
        public EAnalyticsDeliveryStatus Status { get; }
        public string Detail { get; }
        public AnalyticsDiagnosticModel(AnalyticsEventData data, int providerId, EAnalyticsDeliveryStatus status, string detail)
        { Event = data; ProviderId = providerId; Status = status; Detail = detail; }
    }
}
