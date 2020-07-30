using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Google;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee;
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

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public class LoginProvider : ILoginProvider
    {
        private readonly IAppSettings _appSettings;
        private readonly ISecureStorage _secureStorage;

        public LoginProvider(IAppSettings appSettings, ISecureStorage secureStorage)
        {
            _appSettings = appSettings;
            _secureStorage = secureStorage;
        }

        /// <summary>
        /// Runs the Google OAuth process in two steps :
        ///     - First step : retrieves the unique OAuth code from login UI web page
        ///     - Second step : send the unique OAuth code to Google's token server to get the OAuth token
        /// </summary>
        /// <returns>Returns a GoogleOAuthResponse containing the OAuth token</returns>
        public async Task<GoogleOAuthResponse> DoGoogleOAuthLoginAsync()
        {
            WebAuthenticatorResult authenticatorResult;

            try
            {
                authenticatorResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(_appSettings.GoogleAuthUrl),
                    new Uri(_appSettings.RedirectUrl));
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e);
                return null;
            }

            var code = authenticatorResult.Properties.FirstOrDefault(x => x.Key.Equals("code")).Value;
            if (string.IsNullOrWhiteSpace(code))
                return null;

            using var client = new HttpClient();
            var parameters = new Dictionary<string, string> {
                    { "code", code },
                    { "client_id", _appSettings.ClientId },
                    { "redirect_uri", _appSettings.RedirectUrl },
                    { "grant_type", "authorization_code" }

                };

            var encodedContent = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(_appSettings.GoogleTokenUrl, encodedContent).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var oauthResponse = JsonConvert.DeserializeObject<GoogleOAuthResponse>(responseContent);

            return oauthResponse;
        }

        /// <summary>
        /// Runs the Wasabee login process to retrieve Wasabeee data
        /// </summary>
        /// <param name="googleOAuthResponse">Google OAuth response object containing the AccessToken</param>
        /// <returns>Returns a WasabeeLoginResponse with account data</returns>
        public async Task<WasabeeLoginResponse> DoWasabeeLoginAsync(GoogleOAuthResponse googleOAuthResponse)
        {
            var cookieContainer = new CookieContainer();
            using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            using var client = new HttpClient(handler);

            var jsonToken = new JObject() { ["accessToken"] = googleOAuthResponse.AccessToken }.ToString();
            var postContent = new StringContent(jsonToken, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_appSettings.WasabeeTokenUrl, postContent).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wasabeeLoginResponse = JsonConvert.DeserializeObject<WasabeeLoginResponse>(responseContent);

            var uri = new Uri(_appSettings.WasabeeBaseUrl);
            var wasabeeCookie = cookieContainer.GetCookies(uri).Cast<Cookie>()
                .AsEnumerable()
                .FirstOrDefault();

            if (wasabeeCookie != null)
                await _secureStorage.SetAsync(SecureStorageConstants.WasabeeCookie, JsonConvert.SerializeObject(wasabeeCookie));

            return wasabeeLoginResponse;
        }

        /// <summary>
        /// Sends the FCM subscription token to Wasabee server in order to get notified when required
        /// </summary>
        /// <param name="token">FCM token</param>
        /// <returns></returns>
        public async Task SendFirebaseTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            var cookie = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeCookie);
            if (string.IsNullOrWhiteSpace(cookie))
                throw new KeyNotFoundException(SecureStorageConstants.WasabeeCookie);

            var wasabeeCookie = JsonConvert.DeserializeObject<Cookie>(cookie);

            var cookieContainer = new CookieContainer();

            using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            using var client = new HttpClient(handler);

            cookieContainer.Add(new Uri(_appSettings.WasabeeBaseUrl), wasabeeCookie);

            var url = $"{_appSettings.WasabeeBaseUrl}{WasabeeRoutesConstants.Firebase}";
            var postContent = new StringContent(token, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, postContent).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                // TODO handle response
            }
        }

        public Task RemoveTokenFromSecureStore()
        {
            return Task.FromResult(_secureStorage.Remove(SecureStorageConstants.WasabeeCookie));
        }

        public void ClearCookie()
        {
            // TODO
        }

        public Task RefreshTokenAsync()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}