using System.Threading;
using System.Threading.Tasks;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    public interface IAnalyticsProvider : IAnalyticsSdkProvider
    {
        Task<bool> SendAsync(AnalyticsEventData data, CancellationToken token);
    }
}
