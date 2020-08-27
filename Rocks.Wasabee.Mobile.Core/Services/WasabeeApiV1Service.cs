using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    [Headers("Accept: application/json")]
    public interface IWasabeeApiV1
    {
        [Get("/draw/{opId}")]
        Task<OperationModel> GetOperation(string opId);

    }

    public class WasabeeApiV1Service : BaseApiService
    {
        private readonly IAppSettings _appSettings;

        private Lazy<IWasabeeApiV1> _wasabeeApiClient => new Lazy<IWasabeeApiV1>(() => RestService.For<IWasabeeApiV1>(CreateHttpClient($"{_appSettings.WasabeeBaseUrl}{WasabeeRoutesConstants.BaseRoute}")));

        private IWasabeeApiV1 WasabeeApiClient => _wasabeeApiClient.Value;

        public WasabeeApiV1Service(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<OperationModel> GetOperation(string opId)
        {
            var operationModel = await AttemptAndRetry(() => WasabeeApiClient.GetOperation(opId), new CancellationToken()).ConfigureAwait(false);

            return operationModel;
        }

    }
}