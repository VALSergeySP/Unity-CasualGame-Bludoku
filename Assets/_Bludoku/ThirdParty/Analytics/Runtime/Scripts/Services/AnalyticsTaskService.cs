using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameBrewStudio.CoreTemplate.Analytics
{
    public static class AnalyticsTaskService
    {
        // Also bounds SDK operations that ignore their cancellation token.
        public static async Task<T> WithCancellationAsync<T>(Task<T> operation, CancellationToken token)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            var cancelled = new TaskCompletionSource<bool>();
            using (token.Register(() => cancelled.TrySetResult(true)))
            {
                if (await Task.WhenAny(operation, cancelled.Task) != operation)
                {
                    ObserveFailure(operation);
                    token.ThrowIfCancellationRequested();
                }
                token.ThrowIfCancellationRequested();
                return await operation;
            }
        }
        private static void ObserveFailure(Task operation)
        {
            operation.ContinueWith(task => { var exception = task.Exception; },
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }
}