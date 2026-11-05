using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Infra.Constants;
using Rocks.Wasabee.Core.Infra.Security;
using Rocks.Wasabee.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Core.Models.AuthTokens.Wasabee;
using Rocks.Wasabee.Core.Models.Users;
using Rocks.Wasabee.Core.Settings.Application;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Rocks.Wasabee.Maui.Core.Services
{
    public class LoginProvider : ILoginProvider
    {
        private readonly IAppSettings _appSettings;
        private readonly ISecureStorageAdapter _secureStorageService;

        public LoginProvider(
            IAppSettings appSettings,
            ISecureStorageAdapter secureStorageService)
        {
            _appSettings = appSettings;
            _secureStorageService = secureStorageService;
        }

        /// <summary>
        /// Runs the Google OAuth process in two steps :
        ///     - First step : retrieves the unique OAuth code from login UI web page
        ///     - Second step : send the unique OAuth code to Google's token server to get the OAuth token
        /// </summary>
        /// <returns>Returns a <see cref="GoogleToken"/> containing the OAuth token</returns>
        public async Task<GoogleToken?> DoGoogleOAuthLoginAsync()
        {
            WebAuthenticatorResult authenticatorResult = new();

            try
            {
                authenticatorResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(_appSettings.GoogleAuthUrl),
                    new Uri(_appSettings.RedirectUrl));
            }
            catch (TaskCanceledException)
            {
                return null;
            }

            var code = authenticatorResult.Properties.FirstOrDefault(x => x.Key.Equals("code")).Value;
            if (string.IsNullOrWhiteSpace(code))
            {
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
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var googleToken = JsonSerializer.Deserialize<GoogleToken>(responseContent);

            return googleToken;
        }

        /// <summary>
        /// Runs the Wasabee login process to retrieve Wasabeee data
        /// </summary>
        /// <param name="googleToken">Google OAuth response object containing the AccessToken</param>
        /// <returns>Returns a <see cref="UserModel"/> with account data if login is succesfull</returns>
        public async Task<UserModel?> DoWasabeeLoginAsync(GoogleToken googleToken)
        {
            HttpResponseMessage response;

            //#if DEBUG_NETWORK_LOGS
            //            var httpHandler = new HttpLoggingHandler(Mvx.IoCProvider.Resolve<IFactory>().CreateHandler());
            //#else
            //            var httpHandler = _httpClientFactory.CreateHandler();
            //#endif

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5),
                //DefaultRequestHeaders = { { "User-Agent", $"WasabeeMobile/{_versionTracking.CurrentVersion} ({_deviceInfo.Platform} {_deviceInfo.VersionString})" } }
            };

            try
            {
                var wasabeeToken = new WasabeeToken(googleToken.AccessToken);
                var postContent = new StringContent(JsonSerializer.Serialize(wasabeeToken), Encoding.UTF8, MediaTypeNames.Application.Json);
                response = await client.PostAsync(_appSettings.WasabeeTokenUrl, postContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wasabeeUserModel = JsonSerializer.Deserialize<UserModel?>(responseContent);

            if (wasabeeUserModel != null && !string.IsNullOrWhiteSpace(wasabeeUserModel.Jwt))
                await _secureStorageService.SetAsync(SecureStorageConstants.WasabeeToken, wasabeeUserModel.Jwt);
            else
                return null;

            return wasabeeUserModel;
        }

        /// <summary>
        /// Runs the Wasabee login process with one time token to retrieve Wasabeee data
        /// </summary>
        /// <param name="oneTimeToken">Wasabee one time token</param>
        /// <returns>Returns a <see cref="UserModel"/> with account data if login is succesfull</returns>
        public async Task<UserModel?> DoWasabeeOneTimeTokenLoginAsync(string oneTimeToken)
        {
            HttpResponseMessage response;

            //#if DEBUG_NETWORK_LOGS
            //                    var httpHandler = new HttpLoggingHandler(Mvx.IoCProvider.Resolve<IFactory>().CreateHandler());
            //#else
            //            var httpHandler = _httpClientFactory.CreateHandler();
            //#endif

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5),
                //DefaultRequestHeaders = { { "User-Agent", $"WasabeeMobile/{_versionTracking.CurrentVersion} ({_deviceInfo.Platform} {_deviceInfo.VersionString})" } }
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
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var wasabeeUserModel = JsonSerializer.Deserialize<UserModel?>(responseContent);

            if (wasabeeUserModel != null && !string.IsNullOrWhiteSpace(wasabeeUserModel.Jwt))
                await _secureStorageService.SetAsync(SecureStorageConstants.WasabeeToken, wasabeeUserModel.Jwt);
            else
                return null;

            return wasabeeUserModel;
        }

        /// <summary>
        /// Refreshes the GoogleToken using a RefreshToken
        /// </summary>
        /// <param name="refreshToken">Google's refresh token</param>
        /// <returns>Returns a <see cref="GoogleToken"/></returns>
        public async Task<GoogleToken?> RefreshGoogleTokenAsync(string refreshToken)
        {
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
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var googleToken = JsonSerializer.Deserialize<GoogleToken>(responseContent);

            return googleToken;
        }

        public Task RemoveTokensFromSecureStore()
        {
            _secureStorageService.Remove(SecureStorageConstants.WasabeeToken);
            _secureStorageService.Remove(SecureStorageConstants.GoogleToken);

            return Task.CompletedTask;
        }
    }
}
