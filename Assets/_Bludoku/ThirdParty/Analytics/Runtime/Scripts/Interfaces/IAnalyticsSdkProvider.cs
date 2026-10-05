using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameBrewStudio.CoreTemplate.Analytics
{
    public interface IAnalyticsSdkProvider : IDisposable
    {
        bool IsAvailable { get; }
        Task InitializeAsync(CancellationToken token);
    }
}