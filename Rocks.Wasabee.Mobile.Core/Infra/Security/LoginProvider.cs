using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.HttpClientFactory;
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
using MvvmCross;
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
        private readonly IFactory _httpClientFactory;

        public LoginProvider(IAppSettings appSettings, ISecureStorage secureStorage, ILoggingService loggingService,
            IDeviceInfo deviceInfo, IVersionTracking versionTracking, IFactory httpClientFactory)
        {
            _appSettings = appSettings;
            _secureStorage = secureStorage;
            _loggingService = loggingService;
            _deviceInfo = deviceInfo;
            _versionTracking = versionTracking;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Runs the Google OAuth process in two steps :
        ///     - First step : retrieves the unique OAuth code from login UI web page
        ///     - Second step : send the unique OAuth code to Google's token server to get the OAuth token
        /// </summary>
        /// <returns>Returns a <see cref="GoogleToken"/> containing the OAuth token</returns>
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
        /// <returns>Returns a <see cref="UserModel"/> with account data if login is succesfull</returns>
        public async Task<UserModel?> DoWasabeeLoginAsync(GoogleToken googleToken)
        {
            _loggingService.Trace("Executing LoginProvider.DoWasabeeLoginAsync");

            HttpResponseMessage response;
            var cookieContainer = new CookieContainer();

#if DEBUG_NETWORK_LOGS
            var httpHandler = new HttpLoggingHandler(Mvx.IoCProvider.Resolve<IFactory>().CreateHandler(cookieContainer));
#else
            var httpHandler = _httpClientFactory.CreateHandler(cookieContainer);
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
            
            if (wasabeeUserModel != null && !string.IsNullOrWhiteSpace(wasabeeUserModel.Jwt))
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeJwt, wasabeeUserModel.Jwt);
            
            // TODO : Remove cookie when server can fully handle JWTs
            var uri = new Uri(_appSettings.WasabeeBaseUrl);
            var wasabeeCookie = cookieContainer.GetCookies(uri).Cast<Cookie>()
                .AsEnumerable()
                .FirstOrDefault();

            if (wasabeeCookie != null)
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeCookie, JsonConvert.SerializeObject(wasabeeCookie));

            return wasabeeUserModel;
        }

        /// <summary>
        /// Runs the Wasabee login process with one time token to retrieve Wasabeee data
        /// </summary>
        /// <param name="oneTimeToken">Wasabee one time token</param>
        /// <returns>Returns a <see cref="UserModel"/> with account data if login is succesfull</returns>
        public async Task<UserModel?> DoWasabeeOneTimeTokenLoginAsync(string oneTimeToken)
        {
            _loggingService.Trace("Executing LoginProvider.DoWasabeeOneTimeTokenLoginAsync");

            HttpResponseMessage response;
            var cookieContainer = new CookieContainer();

#if DEBUG_NETWORK_LOGS
            var httpHandler = new HttpLoggingHandler(new HttpLoggingHandler(Mvx.IoCProvider.Resolve<IFactory>().CreateHandler(cookieContainer)));
#else
            var httpHandler = _httpClientFactory.CreateHandler(cookieContainer);
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
            
            if (wasabeeUserModel != null && !string.IsNullOrWhiteSpace(wasabeeUserModel.Jwt))
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeJwt, wasabeeUserModel.Jwt);

            // TODO : Remove cookie when server can fully handle JWTs
            var uri = new Uri(_appSettings.WasabeeBaseUrl);
            var wasabeeCookie = cookieContainer.GetCookies(uri).Cast<Cookie>()
                .AsEnumerable()
                .FirstOrDefault();

            if (wasabeeCookie != null)
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeCookie, JsonConvert.SerializeObject(wasabeeCookie));
            
            return wasabeeUserModel;
        }
        
        /// <summary>
        /// Refreshes the GoogleToken using a RefreshToken
        /// </summary>
        /// <param name="refreshToken">Google's refresh token</param>
        /// <returns>Returns a <see cref="GoogleToken"/></returns>
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

        public Task RemoveTokenFromSecureStore()
        {
            _loggingService.Trace("Executing LoginProvider.RemoveTokenFromSecureStore");
            _secureStorage.Remove(SecureStorageConstants.WasabeeJwt);

            return Task.CompletedTask;
        }

        [Obsolete]
        public void ClearCookie()
        {
            _loggingService.Trace("Executing LoginProvider.ClearCookie");
            _secureStorage.Remove(SecureStorageConstants.WasabeeCookie);
        }
    }
}