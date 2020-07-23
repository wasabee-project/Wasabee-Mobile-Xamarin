using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocks.Wasabee.Mobile.Core.Models.Wasabee;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Xamarin.Essentials;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public class LoginProvider : ILoginProvider
    {
        private readonly IAppSettings _appSettings;

        public LoginProvider(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

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
            if (!string.IsNullOrWhiteSpace(code))
            {
                using (var client = new HttpClient())
                {
                    var parameters = new Dictionary<string, string> {
                        { "code", code },
                        { "client_id", _appSettings.ClientId },
                        { "redirect_uri", _appSettings.RedirectUrl },
                        { "grant_type", "authorization_code" }

                    };
                    var encodedContent = new FormUrlEncodedContent (parameters);

                    var response = await client.PostAsync(_appSettings.GoogleTokenUrl, encodedContent).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var oauthResponse = JsonConvert.DeserializeObject<GoogleOAuthResponse>(responseContent);

                        return oauthResponse;
                    }
                }
            }

            return null;
        }

        public async Task<WasabeeLoginResponse> DoWasabeeLoginAsync(GoogleOAuthResponse googleOAuthResponse)
        {
            using (var client = new HttpClient())
            {
                var jsonToken = new JObject() { ["accessToken"] = googleOAuthResponse.AccessToken }.ToString();
                var postContent = new StringContent(jsonToken, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_appSettings.WasabeeTokenUrl, postContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var wasabeeLoginResponse = JsonConvert.DeserializeObject<WasabeeLoginResponse>(responseContent);

                    return wasabeeLoginResponse;
                }
            }

            return null;
        }

        public Task RemoveTokenFromSecureStore()
        {
            return Task.CompletedTask;
        }

        public void ClearCookie()
        {

        }

        public Task RefreshTokenAsync()
        {
            return Task.CompletedTask;
        }
    }
}