using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.QueryModels;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    [Headers("Accept: application/json")]
    public interface IWasabeeApiV1
    {
        #region User

        [Get("/me")]
        Task<ApiResponse<UserModel>> User_GetUserInformations();

        [Get("/me/{teamId}?state={state}")]
        Task<ApiResponse<string>> User_ChangeTeamState(string teamId, string state);

        [Get("/me?lat={lat}&lon={lon}")]
        Task<ApiResponse<string>> User_UpdateLocation(string lat, string lon);

        #endregion

        #region Agents

        [Get("/agent/{agentId}")]
        Task<ApiResponse<TeamAgentModel>> Agents_GetAgent(string agentId);

        #endregion

        #region Teams

        [Post("/teams")]
        Task<ApiResponse<IList<Models.Teams.TeamModel>>> Teams_GetTeams([Body] GetTeamsQuery getTeamsQuery);

        [Get("/team/{teamId}")]
        Task<ApiResponse<Models.Teams.TeamModel>> Teams_GetTeam(string teamId);

        [Post("/team/{teamId}/{key}")]
        Task<ApiResponse<string>> Teams_AddAgentToTeam(string teamId, string key);

        [Delete("/team/{teamId}/{key}")]
        Task<ApiResponse<string>> Teams_RemoveAgentFromTeam(string teamId, string key);

        #endregion

        #region Operations

        [Get("/draw/{opId}")]
        Task<ApiResponse<OperationModel>> Operations_GetOperation(string opId);

        [Get("/draw/{opId}/link/{linkId}")]
        Task<ApiResponse<LinkModel>> Operations_GetLink(string opId, string linkId);

        [Get("/draw/{opId}/marker/{markerId}")]
        Task<ApiResponse<MarkerModel>> Operations_GetMarker(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/acknowledge")]
        Task<ApiResponse<string>> Operation_Marker_Acknowledge(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/incomplete")]
        Task<ApiResponse<string>> Operation_Marker_Incomplete(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/complete")]
        Task<ApiResponse<string>> Operation_Marker_Complete(string opId, string markerId);

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

        #region User

        public async Task<UserModel?> User_GetUserInformations()
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_GetUserInformations(), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<bool> User_ChangeTeamState(string teamId, string state)
        {
            if (state.Equals("On") || state.Equals("Off"))
            {
                var result = await AttemptAndRetry(() => WasabeeApiClient.User_ChangeTeamState(teamId, state), new CancellationToken()).ConfigureAwait(false);
                return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
            }

            throw new ArgumentException($"{nameof(state)} '{state}' is not a valid parameter");
        }

        public async Task<bool> User_UpdateLocation(string lat, string lon)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_UpdateLocation(lat, lon), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        #endregion

        #region Agents

        public async Task<TeamAgentModel?> Agents_GetAgent(string agentId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Agents_GetAgent(agentId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        #endregion

        #region Teams

        public async Task<IList<Models.Teams.TeamModel>> Teams_GetTeams(GetTeamsQuery getTeamsQuery)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_GetTeams(getTeamsQuery), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : new List<Models.Teams.TeamModel>();
        }

        public async Task<Models.Teams.TeamModel?> Teams_GetTeam(string teamId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_GetTeam(teamId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<bool> Teams_AddAgentToTeam(string teamId, string key)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_AddAgentToTeam(teamId, key), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        public async Task<bool> Teams_RemoveAgentFromTeam(string teamId, string key)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_RemoveAgentFromTeam(teamId, key), new CancellationToken()).ConfigureAwait(false);
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

        public async Task<bool> Operation_Marker_Acknowledge(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Acknowledge(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        public async Task<bool> Operation_Marker_Incomplete(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Incomplete(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
        }

        public async Task<bool> Operation_Marker_Complete(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Complete(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content.Contains("\"status\":\"ok\"");
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