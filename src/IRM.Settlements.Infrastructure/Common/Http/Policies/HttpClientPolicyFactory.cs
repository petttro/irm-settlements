using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Polly.Wrap;

namespace IRM.Settlements.Infrastructure.Common.Http.Policies;

public static class HttpClientPolicyFactory
{
    public static AsyncPolicyWrap<HttpResponseMessage> GetRetryPolicy(TimeSpan timeout)
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeout, TimeoutStrategy.Optimistic);
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        return retryPolicy.WrapAsync(timeoutPolicy);
    }
}
