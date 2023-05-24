using Flurl.Http;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http;

public class SemaphoreInterceptor : ICallInterceptor
{
    private readonly AsyncSemaphore _semaphore;

    public SemaphoreInterceptor(int degreeOfParallelism)
    {
        if (degreeOfParallelism < 1)
            throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));

        _semaphore = new AsyncSemaphore(degreeOfParallelism);
    }

    public async Task InterceptBeforeAsync(FlurlCall call)
    {
        await _semaphore.WaitAsync();
    }

    public Task InterceptAfterAsync(FlurlCall call)
    {
        _semaphore.Release();

        return Task.CompletedTask;
    }
}
