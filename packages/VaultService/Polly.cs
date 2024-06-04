using Polly;
using Polly.RateLimit;
using Polly.Retry;

public static class PollyPolicy
{
    public static AsyncRetryPolicy GetRetryPolicy()
    {
        return Policy
            .Handle<RateLimitRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retry {retryAttempt} due to rate limiting. Waiting {timespan.TotalMilliseconds}ms before next retry.");
                }
            );
    }
}
