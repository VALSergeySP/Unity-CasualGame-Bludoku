using GameBrewStudio.CoreTemplate.Analytics;
using UnityEngine;

namespace GameBrewStudio.CoreTemplate.Samples
{
    [CreateAssetMenu(menuName = "GameBrewStudio/Core Template/Analytics/Simulated Provider")]
    public sealed class SampleAnalyticsProviderConfig : AnalyticsProviderConfig
    {
        [SerializeField] private ESampleAnalyticsResponse _response;
        public ESampleAnalyticsResponse Response => _response;
        public override IAnalyticsSdkProvider CreateProvider() => new SampleAnalyticsProvider(_response);
    }
}