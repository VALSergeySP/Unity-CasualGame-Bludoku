using UnityEngine;

namespace GameBrewStudio.CoreTemplate.Analytics
{
    public abstract class AnalyticsProviderConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _id = 1;
        public AnalyticsProviderIdStruct Id => new AnalyticsProviderIdStruct(_id);
        public abstract IAnalyticsSdkProvider CreateProvider();
    }
}