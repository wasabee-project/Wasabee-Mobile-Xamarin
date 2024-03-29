using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.Models.Agent;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
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
        Task<ApiResponse<WasabeeApiResponse>> User_ChangeTeamState(string teamId, string state);

        [Get("/me?lat={lat}&lon={lon}")]
        Task<ApiResponse<WasabeeApiResponse>> User_UpdateLocation(string lat, string lon);

        [Post("/me/firebase")]
        Task<ApiResponse<WasabeeApiResponse>> User_UpdateFirebaseToken([Body] string token);

        [Get("/me/jwtrefresh")]
        Task<ApiResponse<WasabeeJwtApiResponse>> User_RefreshWasabeeToken();

        [Get("/me/commproof?name={name}")]
        Task<ApiResponse<WasabeeJwtApiResponse>> User_GetVerificationToken(string name);

        [Get("/me/commverify?name={name}")]
        Task<ApiResponse<WasabeeApiResponse>> User_GetVerificationStatus(string name);

        #endregion

        #region Agents

        [Get("/agent/{agentId}")]
        Task<ApiResponse<AgentModel>> Agents_GetAgent(string agentId);

        #endregion

        #region Teams

        [Post("/teams")]
        Task<ApiResponse<IList<Models.Teams.TeamModel>>> Teams_GetTeams([Body] GetTeamsQuery getTeamsQuery);

        [Get("/team/{teamId}")]
        Task<ApiResponse<Models.Teams.TeamModel>> Teams_GetTeam(string teamId);

        [Post("/team/{teamId}/{agentId}")]
        Task<ApiResponse<WasabeeApiResponse>> Teams_AddAgentToTeam(string teamId, string agentId);

        [Delete("/team/{teamId}/{agentId}")]
        Task<ApiResponse<WasabeeApiResponse>> Teams_RemoveAgentFromTeam(string teamId, string agentId);

        [Multipart]
        [Put("/team/{teamId}/rename")]
        Task<ApiResponse<WasabeeApiResponse>> Teams_RenameTeam(string teamId, [AliasAs("teamname")] string name);

        [Delete("/team/{teamId}")]
        Task<ApiResponse<WasabeeApiResponse>> Teams_DeleteTeam(string teamId);

        #endregion

        #region Operations

        [Get("/draw/{opId}")]
        Task<ApiResponse<OperationModel>> Operations_GetOperation(string opId);

        #region Link

        [Get("/draw/{opId}/link/{linkId}")]
        Task<ApiResponse<LinkModel>> Operations_GetLink(string opId, string linkId);

        [Get("/draw/{opId}/link/{linkId}/complete")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Link_Complete(string opId, string linkId);

        [Get("/draw/{opId}/link/{linkId}/incomplete")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Link_Incomplete(string opId, string linkId);

        [Post("/draw/{opId}/link/{linkId}/claim")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Link_Claim(string opId, string linkId);

        [Post("/draw/{opId}/link/{linkId}/reject")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Link_Reject(string opId, string linkId);

        #endregion

        #region Marker

        [Get("/draw/{opId}/marker/{markerId}")]
        Task<ApiResponse<MarkerModel>> Operations_GetMarker(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/acknowledge")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Marker_Acknowledge(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/incomplete")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Marker_Incomplete(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/complete")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Marker_Complete(string opId, string markerId);

        [Post("/draw/{opId}/marker/{markerId}/claim")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Marker_Claim(string opId, string markerId);

        [Get("/draw/{opId}/marker/{markerId}/reject")]
        Task<ApiResponse<WasabeeOpUpdateApiResponse>> Operation_Marker_Reject(string opId, string markerId);

        #endregion

        #endregion
    }

    public class WasabeeApiV1Service : BaseApiService
    {
        private readonly IAppSettings _appSettings;

        private Lazy<IWasabeeApiV1> _wasabeeApiClient => new Lazy<IWasabeeApiV1>(() => RestService.For<IWasabeeApiV1>(CreateHttpClient($"{_appSettings.WasabeeBaseUrl}{WasabeeRoutesConstants.BaseRoute}"), GetNewtonsoftJsonRefitSettings()));
        private IWasabeeApiV1 WasabeeApiClient => _wasabeeApiClient.Value;

        private static RefitSettings GetNewtonsoftJsonRefitSettings() => new RefitSettings(new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
        
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
                return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
            }

            throw new ArgumentException($"{nameof(state)} '{state}' is not a valid parameter");
        }

        public async Task<bool> User_UpdateLocation(string lat, string lon)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_UpdateLocation(lat, lon), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        public async Task<bool> User_UpdateFirebaseToken(string token)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_UpdateFirebaseToken(token), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        public async Task<string> User_RefreshWasabeeToken()
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_RefreshWasabeeToken(), new CancellationToken()).ConfigureAwait(false);
            if (result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess())
                return result.Content.Token;

            return string.Empty;
        }

        public async Task<string> User_GetVerificationToken(string name)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_GetVerificationToken(name), new CancellationToken()).ConfigureAwait(false);
            if (result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess())
                return result.Content.Token;

            return string.Empty;
        }

        public async Task<bool> User_GetVerificationStatus(string name)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.User_GetVerificationStatus(name), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        #endregion

        #region Agents

        public async Task<AgentModel?> Agents_GetAgent(string agentId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Agents_GetAgent(agentId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        #endregion

        #region Teams

        public async Task<IList<Models.Teams.TeamModel>> Teams_GetTeams(GetTeamsQuery getTeamsQuery)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_GetTeams(getTeamsQuery), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null ? result.Content : new List<Models.Teams.TeamModel>();
        }

        public async Task<Models.Teams.TeamModel?> Teams_GetTeam(string teamId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_GetTeam(teamId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<bool> Teams_AddAgentToTeam(string teamId, string agentId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_AddAgentToTeam(teamId, agentId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        public async Task<bool> Teams_RemoveAgentFromTeam(string teamId, string agentId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_RemoveAgentFromTeam(teamId, agentId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        public async Task<bool> Teams_RenameTeam(string teamId, string name)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_RenameTeam(teamId, name), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        public async Task<bool> Teams_DeleteTeam(string teamId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Teams_DeleteTeam(teamId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess();
        }

        #endregion

        #region Operations

        public async Task<OperationModel?> Operations_GetOperation(string opId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operations_GetOperation(opId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        #region Link

        public async Task<LinkModel?> Operations_GetLink(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operations_GetLink(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Link_Complete(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Link_Complete(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Link_Incomplete(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Link_Incomplete(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Link_Claim(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Link_Claim(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Link_Reject(string opId, string linkId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Link_Reject(opId, linkId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        #endregion

        #region Markers

        public async Task<MarkerModel?> Operations_GetMarker(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operations_GetMarker(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Marker_Acknowledge(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Acknowledge(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Marker_Incomplete(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Incomplete(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Marker_Complete(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Complete(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Marker_Claim(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Claim(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        public async Task<WasabeeOpUpdateApiResponse?> Operation_Marker_Reject(string opId, string markerId)
        {
            var result = await AttemptAndRetry(() => WasabeeApiClient.Operation_Marker_Reject(opId, markerId), new CancellationToken()).ConfigureAwait(false);
            return result.IsSuccessStatusCode && result.Content != null && result.Content.IsSuccess() ? result.Content : null;
        }

        #endregion

        #endregion
    }
}