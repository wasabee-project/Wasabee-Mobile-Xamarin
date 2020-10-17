using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Models.Users;
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
        [Get("/team/{teamId}")]
        Task<ApiResponse<TeamModel>> GetTeam(string teamId);

        #region User

        [Get("/me?json=y")]
        Task<ApiResponse<UserModel>> User_GetUserInformations();

        [Get("/me/{teamId}?state={state}")]
        Task<string> User_ChangeTeamState(string teamId, string state);

        [Get("/me?lat={lat}&lon={lon}")]
        Task<ApiResponse<string>> User_UpdateLocation(string lat, string lon);

        #endregion

        #region Operations

        [Get("/draw/{opId}")]
        Task<ApiResponse<OperationModel>> Operations_GetOperation(string opId);

        [Get("/draw/{opId}/link/{linkId}")]
        Task<ApiResponse<LinkModel>> Operations_GetLink(string opId, string linkId);

        [Get("/draw/{opId}/marker/{markerId}")]
        Task<ApiResponse<MarkerModel>> Operations_GetMarker(string opId, string markerId);

        [Get("/draw/{opId}/link/{linkId}/complete")]
        Task<ApiResponse<string>> Operation_Link_Complete(string opId, string linkId);

        [Get("/draw/{opId}/link/{linkId}/incomplete")]
        Task<ApiResponse<string>> Operation_Link_Incomplete(string opId, string linkId);

        #endregion
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

        public async Task<TeamModel?> GetTeam(string teamId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.GetTeam(teamId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        #region User

        public async Task<UserModel?> User_GetUserInformations()
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_GetUserInformations(), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<string> User_ChangeTeamState(string teamId, string state)
        {
            if (state.Equals("On") || state.Equals("Off"))
                return await AttemptAndRetry(() => WasabeeApiClient.User_ChangeTeamState(teamId, state), new CancellationToken()).ConfigureAwait(false);

            throw new ArgumentException($"{nameof(state)} '{state}' is not a valid parameter");
        }

        public async Task<bool> User_UpdateLocation(string lat, string lon)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_UpdateLocation(lat, lon), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        #endregion

        #region Operations

        public async Task<OperationModel?> Operations_GetOperation(string opId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operations_GetOperation(opId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<LinkModel?> Operations_GetLink(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operations_GetLink(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<MarkerModel?> Operations_GetMarker(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operations_GetMarker(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<bool> Operation_Link_Complete(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Link_Complete(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        public async Task<bool> Operation_Link_Incomplete(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Link_Incomplete(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        #endregion
    }
}