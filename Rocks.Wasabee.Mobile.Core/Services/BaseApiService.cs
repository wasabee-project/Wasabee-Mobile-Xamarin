using MvvmCross;
using Newtonsoft.Json;
using Polly;
using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

#if DEBUG_NETWORK_LOGS
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
#endif

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public abstract class BaseApiService
    {
        private static HttpClient? _httpClient = null;
        private static bool _reinstanciateHttpClient = false;

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3)
        {
            return Policy.Handle<Exception>(shouldHandleException).WaitAndRetryAsync(numRetries, retryAttempt).ExecuteAsync(token => action(), cancellationToken);

            static TimeSpan retryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

            static bool shouldHandleException(Exception exception)
            {
                if (exception is ApiException apiException)
                    return !isForbiddenOrUnauthorized(apiException);
                
                _reinstanciateHttpClient = true;

                return true;

                static bool isForbiddenOrUnauthorized(ApiException apiException) => apiException.StatusCode is System.Net.HttpStatusCode.Forbidden || apiException.StatusCode is System.Net.HttpStatusCode.Unauthorized;
            }
        }

        protected static HttpClient CreateHttpClient(string url)
        {
            if (_httpClient != null && !_reinstanciateHttpClient)
                return _httpClient;

            _reinstanciateHttpClient = false;

            var wasabeeRawCookie = Mvx.IoCProvider.Resolve<ISecureStorage>().GetAsync(SecureStorageConstants.WasabeeCookie).Result;
            var cookie = JsonConvert.DeserializeObject<Cookie>(wasabeeRawCookie);

            
#if DEBUG_NETWORK_LOGS
            var httpClientHandler = new HttpClientHandler() {CookieContainer = new CookieContainer()};
            httpClientHandler.CookieContainer.Add(cookie);

            var httpHandler = new HttpLoggingHandler(httpClientHandler);
#else
            var httpHandler = Mvx.IoCProvider.Resolve<IFactory>().CreateHandler(new CookieContainer());
            httpHandler.CookieContainer.Add(cookie);
#endif
            var appVersion = Mvx.IoCProvider.Resolve<IVersionTracking>().CurrentVersion;
            var device = Mvx.IoCProvider.Resolve<IDeviceInfo>();
            var client = new HttpClient(httpHandler)
            {
                Timeout = TimeSpan.FromSeconds(5),
                BaseAddress = new Uri(url),
                DefaultRequestHeaders =
                {
                    { "User-Agent", $"WasabeeMobile/{appVersion} ({device.Platform} {device.VersionString})" },
                    { "Connection", "keep-alive" }
                }
            };

            _httpClient = client;
            return _httpClient;
        }
    }

#if DEBUG_NETWORK_LOGS
    public class HttpLoggingHandler : DelegatingHandler
    {
        public HttpLoggingHandler(HttpMessageHandler innerHandler = null) : base(innerHandler ?? new HttpClientHandler())
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var req = request;
            var id = Guid.NewGuid().ToString();
            var msg = $"[{id} -   Request]";

            Debug.WriteLine($"{msg}========Start==========");
            Debug.WriteLine($"{msg} {req.Method} {req.RequestUri.PathAndQuery} {req.RequestUri.Scheme}/{req.Version}");
            Debug.WriteLine($"{msg} Host: {req.RequestUri.Scheme}://{req.RequestUri.Host}");

            foreach (var header in req.Headers)
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

            if (req.Content != null)
            {
                foreach (var header in req.Content.Headers)
                    Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

                if (req.Content is StringContent || IsTextBasedContentType(req.Headers) ||
                    this.IsTextBasedContentType(req.Content.Headers))
                {
                    var result = await req.Content.ReadAsStringAsync();

                    Debug.WriteLine($"{msg} Content:");
                    Debug.WriteLine($"{msg} {result}");
                }
            }

            var start = DateTime.Now;

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            var end = DateTime.Now;

            Debug.WriteLine($"{msg} Duration: {end - start}");
            Debug.WriteLine($"{msg}==========End==========");

            msg = $"[{id} - Response]";
            Debug.WriteLine($"{msg}=========Start=========");

            var resp = response;

            Debug.WriteLine(
                $"{msg} {req.RequestUri.Scheme.ToUpper()}/{resp.Version} {(int) resp.StatusCode} {resp.ReasonPhrase}");

            foreach (var header in resp.Headers)
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

            if (resp.Content != null)
            {
                foreach (var header in resp.Content.Headers)
                    Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

                if (resp.Content is StringContent || this.IsTextBasedContentType(resp.Headers) ||
                    this.IsTextBasedContentType(resp.Content.Headers))
                {
                    start = DateTime.Now;
                    var result = await resp.Content.ReadAsStringAsync();
                    end = DateTime.Now;

                    Debug.WriteLine($"{msg} Content:");
                    Debug.WriteLine($"{msg} {result}");
                    Debug.WriteLine($"{msg} Duration: {end - start}");
                }
            }

            Debug.WriteLine($"{msg}==========End==========");
            return response;
        }

        readonly string[] types = new[] {"html", "text", "xml", "json", "txt", "x-www-form-urlencoded"};

        bool IsTextBasedContentType(HttpHeaders headers)
        {
            IEnumerable<string> values;
            if (!headers.TryGetValues("Content-Type", out values))
                return false;
            var header = string.Join(" ", values).ToLowerInvariant();

            return types.Any(t => header.Contains(t));
        }
    }
#endif
}