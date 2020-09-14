using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using System;
using System.Threading;
using System.Threading.Tasks;
using TeamModel = Rocks.Wasabee.Mobile.Core.Models.Teams.TeamModel;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    [Headers("Accept: application/json")]
    public interface IWasabeeApiV1
    {
        [Get("/me?json=y")]
        Task<string> User_GetUserInformations();

        [Get("/me/{teamId}?state={state}")]
        Task<string> User_ChangeTeamState(string teamId, string state);

        [Get("/draw/{opId}")]
        Task<OperationModel> Operations_GetOperation(string opId);

        [Get("/me?lat={lat}&lon={lon}")]
        Task<string> UpdateLocation(string lat, string lon);

        [Get("/team/{teamId}")]
        Task<TeamModel> GetTeam(string teamId);
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

        public async Task<string> User_GetUserInformations()
        {
            return await AttemptAndRetry(() => WasabeeApiClient.User_GetUserInformations(), new CancellationToken()).ConfigureAwait(false);
        }

        public async Task<string> User_ChangeTeamState(string teamId, string state)
        {
            if (state.Equals("On") || state.Equals("Off"))
                return await AttemptAndRetry(() => WasabeeApiClient.User_ChangeTeamState(teamId, state), new CancellationToken()).ConfigureAwait(false);

            throw new ArgumentException($"{nameof(state)} '{state}' is not a valid parameter");
        }

        public async Task<string> UpdateLocation(string lat, string lon)
        {
            return await AttemptAndRetry(() => WasabeeApiClient.UpdateLocation(lat, lon), new CancellationToken()).ConfigureAwait(false);
        }

        public async Task<OperationModel> Operations_GetOperation(string opId)
        {
            return await AttemptAndRetry(() => WasabeeApiClient.Operations_GetOperation(opId), new CancellationToken()).ConfigureAwait(false);
        }

        public async Task<TeamModel> GetTeam(string teamId)
        {
            return await AttemptAndRetry(() => WasabeeApiClient.GetTeam(teamId), new CancellationToken()).ConfigureAwait(false);
        }
    }
}