using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Wasabee;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

#if DEBUG_NETWORK_LOGS
using Rocks.Wasabee.Mobile.Core.Services;
#endif

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public class LoginProvider : ILoginProvider
    {
        private readonly IAppSettings _appSettings;
        private readonly ISecureStorage _secureStorage;
        private readonly ILoggingService _loggingService;
        private readonly IDeviceInfo _deviceInfo;
        private readonly IVersionTracking _versionTracking;

        public LoginProvider(IAppSettings appSettings, ISecureStorage secureStorage, ILoggingService loggingService,
            IDeviceInfo deviceInfo, IVersionTracking versionTracking)
        {
            _appSettings = appSettings;
            _secureStorage = secureStorage;
            _loggingService = loggingService;
            _deviceInfo = deviceInfo;
            _versionTracking = versionTracking;
        }

        /// <summary>
        /// Runs the Google OAuth process in two steps :
        ///     - First step : retrieves the unique OAuth code from login UI web page
        ///     - Second step : send the unique OAuth code to Google's token server to get the OAuth token
        /// </summary>
        /// <returns>Returns a GoogleOAuthResponse containing the OAuth token</returns>
        public async Task<GoogleToken?> DoGoogleOAuthLoginAsync()
        {
            _loggingService.Trace("Executing LoginProvider.DoGoogleOAuthLoginAsync");

            WebAuthenticatorResult authenticatorResult;

            try
            {
                authenticatorResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(_appSettings.GoogleAuthUrl),
                    new Uri(_appSettings.RedirectUrl));
            }
            catch (TaskCanceledException)
            {
                _loggingService.Trace("Task Canceled : DoGoogleOAuthLoginAsync");

                return null;
            }

            var code = authenticatorResult.Properties.FirstOrDefault(x => x.Key.Equals("code")).Value;
            if (string.IsNullOrWhiteSpace(code))
            {
                _loggingService.Trace("No 'code' property found in authResult. Returning.");

                return null;
            }

            using var client = new HttpClient();
            var parameters = new Dictionary<string, string> {
                    { "code", code },
                    { "client_id", _appSettings.ClientId },
                    { "redirect_uri", _appSettings.RedirectUrl },
                    { "grant_type", "authorization_code" }

                };

            HttpResponseMessage response;
            var encodedContent = new FormUrlEncodedContent(parameters);
            try
            {
                response = await client.PostAsync(_appSettings.GoogleTokenUrl, encodedContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error Executing LoginProvider.DoGoogleOAuthLoginAsync");

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var googleToken = JsonConvert.DeserializeObject<GoogleToken>(responseContent);

            return googleToken;
        }

        /// <summary>
        /// Runs the Wasabee login process to retrieve Wasabeee data
        /// </summary>
        /// <param name="googleToken">Google OAuth response object containing the AccessToken</param>
        /// <returns>Returns a WasabeeLoginResponse with account data</returns>
        public async Task<UserModel?> DoWasabeeLoginAsync(GoogleToken googleToken)
        {
            _loggingService.Trace("Executing LoginProvider.DoWasabeeLoginAsync");

            HttpResponseMessage response;
            var cookieContainer = new CookieContainer();

#if DEBUG_NETWORK_LOGS
            var httpHandler = new HttpLoggingHandler(new HttpClientHandler() { CookieContainer = cookieContainer });
#else
            var httpHandler = new HttpClientHandler() { CookieContainer = cookieContainer };
            httpHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif

            using var client = new HttpClient(httpHandler)
            {
                DefaultRequestHeaders = { { "User-Agent", $"WasabeeMobile/{_versionTracking.CurrentVersion} ({_deviceInfo.Platform} {_deviceInfo.VersionString})" } }
            };

            try
            {
                var wasabeeToken = new WasabeeToken(googleToken.AccessToken);
                var postContent = new StringContent(JsonConvert.SerializeObject(wasabeeToken), Encoding.UTF8, "application/json");
                response = await client.PostAsync(_appSettings.WasabeeTokenUrl, postContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error Executing LoginProvider.DoWasabeeLoginAsync");

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wasabeeUserModel = JsonConvert.DeserializeObject<UserModel?>(responseContent);

            var uri = new Uri(_appSettings.WasabeeBaseUrl);
            var wasabeeCookie = cookieContainer.GetCookies(uri).Cast<Cookie>()
                .AsEnumerable()
                .FirstOrDefault();

            if (wasabeeCookie != null)
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeCookie, JsonConvert.SerializeObject(wasabeeCookie));

            return wasabeeUserModel;
        }

        /// <summary>
        /// Sends the FCM subscription token to Wasabee server in order to get notified when required
        /// </summary>
        /// <param name="token">FCM token</param>
        /// <returns></returns>
        public async Task<bool> SendFirebaseTokenAsync(string token)
        {
            _loggingService.Trace("Executing LoginProvider.SendFirebaseTokenAsync");

            if (string.IsNullOrWhiteSpace(token))
            {
                _loggingService.Info("Token is null, returning");

                return false;
            }

            var cookie = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeCookie);
            if (string.IsNullOrWhiteSpace(cookie))
                return false;
            //TODO : catch this : throw new KeyNotFoundException(SecureStorageConstants.WasabeeCookie);

            try
            {
                var wasabeeCookie = JsonConvert.DeserializeObject<Cookie>(cookie);
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(_appSettings.WasabeeBaseUrl), wasabeeCookie);

                using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
                handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using var client = new HttpClient(handler);

                var url = $"{_appSettings.WasabeeBaseUrl}{WasabeeRoutesConstants.Firebase}";
                var postContent = new StringContent(token, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, postContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return responseContent.Contains("\"status\":\"ok\"");
                }
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error Executing LoginProvider.SendFirebaseTokenAsync");

                return false;
            }

            return false;
        }

        /// <summary>
        /// Runs the Wasabee login process with one time token to retrieve Wasabeee data
        /// </summary>
        /// <param name="oneTimeToken">Wasabee one time token</param>
        /// <returns>Returns a WasabeeLoginResponse with account data</returns>
        public async Task<UserModel?> DoWasabeeOneTimeTokenLoginAsync(string oneTimeToken)
        {
            _loggingService.Trace("Executing LoginProvider.DoWasabeeOneTimeTokenLoginAsync");

            HttpResponseMessage response;
            var cookieContainer = new CookieContainer();

#if DEBUG_NETWORK_LOGS
            var httpHandler = new HttpLoggingHandler(new HttpClientHandler() { CookieContainer = cookieContainer });
#else
            var httpHandler = new HttpClientHandler() { CookieContainer = cookieContainer };
            httpHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif

            using var client = new HttpClient(httpHandler)
            {
                DefaultRequestHeaders = { { "User-Agent", $"WasabeeMobile/{_versionTracking.CurrentVersion} ({_deviceInfo.Platform} {_deviceInfo.VersionString})" } }
            };

            try
            {
                MultipartFormDataContent form = new MultipartFormDataContent
                {
                    { new StringContent(oneTimeToken), "token" }
                };
                
                response = await client.PostAsync(_appSettings.WasabeeOneTimeTokenUrl, form).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error Executing LoginProvider.DoWasabeeLoginAsync");

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wasabeeUserModel = JsonConvert.DeserializeObject<UserModel?>(responseContent);

            var uri = new Uri(_appSettings.WasabeeBaseUrl);
            var wasabeeCookie = cookieContainer.GetCookies(uri).Cast<Cookie>()
                .AsEnumerable()
                .FirstOrDefault();

            if (wasabeeCookie != null)
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeCookie, JsonConvert.SerializeObject(wasabeeCookie));

            return wasabeeUserModel;
        }

        public Task RemoveTokenFromSecureStore()
        {
            _loggingService.Trace("Executing LoginProvider.RemoveTokenFromSecureStore");

            return Task.CompletedTask;
        }

        public void ClearCookie()
        {
            _loggingService.Trace("Executing LoginProvider.ClearCookie");
            _secureStorage.Remove(SecureStorageConstants.WasabeeCookie);
        }

        public async Task<GoogleToken?> RefreshTokenAsync(string refreshToken)
        {
            _loggingService.Trace("Executing LoginProvider.RefreshTokenAsync");

            using var client = new HttpClient();
            var parameters = new Dictionary<string, string> {
                { "refresh_token", refreshToken },
                { "client_id", _appSettings.ClientId },
                { "grant_type", "refresh_token"}

            };

            HttpResponseMessage response;
            var encodedContent = new FormUrlEncodedContent(parameters);
            try
            {
                response = await client.PostAsync(_appSettings.GoogleTokenUrl, encodedContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error Executing LoginProvider.RefreshTokenAsync");

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var googleToken = JsonConvert.DeserializeObject<GoogleToken>(responseContent);

            return googleToken;
        }
    }
}