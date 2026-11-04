using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Infra.Constants;
using Rocks.Wasabee.Core.Settings.Application;
using System.Net.Http.Headers;

namespace Rocks.Wasabee.Core.Infra.Security
{
    public class WasabeeTokenHandler : DelegatingHandler
    {
        private readonly ISecureStorageAdapter _secureStorage;
        private readonly IAppSettings _appSettings;

        public WasabeeTokenHandler(
            ISecureStorageAdapter secureStorage,
            IAppSettings appSettings)
        {
            _secureStorage = secureStorage;
            _appSettings = appSettings;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var wtoken = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeToken);

            if (!string.IsNullOrWhiteSpace(wtoken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", wtoken);
            else
                throw new Exception("Can't retrieve JWT");

            if (!string.IsNullOrEmpty(_appSettings.WasabeeBaseUrl) && request.RequestUri != null)
            {
                var baseUri = new Uri(_appSettings.WasabeeBaseUrl);
                request.RequestUri = new Uri(baseUri, $"{WasabeeRoutesConstants.BaseRoute}{request.RequestUri.PathAndQuery}");
            }
            else
            {
                throw new Exception("Can't retrieve Wasabee URL");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}