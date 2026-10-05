using System.Threading;
using System.Threading.Tasks;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    public interface IAttributionProvider : IAnalyticsSdkProvider
    {
        Task<AttributionData> GetAttributionAsync(CancellationToken token);
    }
}
