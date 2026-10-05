using System.Collections.Generic;

namespace GameBrewStudio.CoreTemplate.Analytics
{
    public interface IAnalyticsEventSink
    {
        bool Track(EAnalyticsEvent kind, EAnalyticsGroup group,
            IEnumerable<AnalyticsParameterData> parameters = null, int customId = 0);
    }
}