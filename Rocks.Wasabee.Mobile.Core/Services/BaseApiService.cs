using MvvmCross;
using Newtonsoft.Json;
using Polly;
using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public abstract class BaseApiService
    {
        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3)
        {
            return Policy.Handle<Exception>(shouldHandleException).WaitAndRetryAsync(numRetries, retryAttempt).ExecuteAsync(token => action(), cancellationToken);

            static TimeSpan retryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

            static bool shouldHandleException(Exception exception)
            {
                if (exception is ApiException apiException)
                    return !isForbiddenOrUnauthorized(apiException);

                return true;

                static bool isForbiddenOrUnauthorized(ApiException apiException) => apiException.StatusCode is System.Net.HttpStatusCode.Forbidden || apiException.StatusCode is System.Net.HttpStatusCode.Unauthorized;
            }
        }

        protected static HttpClient CreateHttpClient(string url)
        {
            var wasabeeRawCookie = Mvx.IoCProvider.Resolve<ISecureStorage>().GetAsync(SecureStorageConstants.WasabeeCookie).Result;
            var cookie = JsonConvert.DeserializeObject<Cookie>(wasabeeRawCookie);

            var httpClientHandler = new HttpClientHandler() { CookieContainer = new CookieContainer() };
            httpClientHandler.CookieContainer.Add(cookie);

            var client = new HttpClient(httpClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(5),
                BaseAddress = new Uri(url),
            };

            return client;
        }
    }
}