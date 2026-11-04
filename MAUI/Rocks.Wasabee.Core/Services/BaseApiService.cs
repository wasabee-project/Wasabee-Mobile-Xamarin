using Polly;
using Refit;

namespace Rocks.Wasabee.Core.Services
{
    public abstract class BaseApiService
    {
        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3)
        {
            return Policy
                .Handle<Exception>(ShouldHandleException)
                .WaitAndRetryAsync(numRetries, RetryAttempt)
                .ExecuteAsync(_ => action(), cancellationToken);

            static TimeSpan RetryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

            static bool ShouldHandleException(Exception exception)
            {
                if (exception is ApiException apiException)
                    return !IsForbiddenOrUnauthorized(apiException);

                return true;

                static bool IsForbiddenOrUnauthorized(ApiException apiException) =>
                    apiException.StatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized;
            }
        }
    }
}
