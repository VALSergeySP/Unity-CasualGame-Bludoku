using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [CreateAssetMenu(menuName = "GameBrewStudio/Core Template/Analytics/Route")]
    public sealed class AnalyticsRouteConfig : ScriptableObject
    {
        private const int EverythingMask = -1;
        [SerializeField] private AnalyticsProviderConfig _provider;
        [SerializeField] private EAnalyticsGroup _groups = EAnalyticsGroup.All;
        public AnalyticsProviderConfig Provider => _provider;
        // Unity's flags inspector represents Everything with all bits set.
        public EAnalyticsGroup Groups => (int)_groups == EverythingMask ? EAnalyticsGroup.All : _groups;
    }
}
