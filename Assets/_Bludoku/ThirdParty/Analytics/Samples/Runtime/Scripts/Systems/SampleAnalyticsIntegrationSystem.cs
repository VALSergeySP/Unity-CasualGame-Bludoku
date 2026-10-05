using GameBrewStudio.CoreTemplate.Analytics;

namespace GameBrewStudio.CoreTemplate.Samples
{
    // Game-specific integrations call Track with their own events and typed parameters.
    public sealed class SampleAnalyticsIntegrationSystem
    {
        public static AnalyticsParameterData Text(EAnalyticsParameter key, string value) =>
            new AnalyticsParameterData(new AnalyticsParameterIdStruct(key), value);
        public static AnalyticsParameterData Integer(EAnalyticsParameter key, long value) =>
            new AnalyticsParameterData(new AnalyticsParameterIdStruct(key), value);
    }
}