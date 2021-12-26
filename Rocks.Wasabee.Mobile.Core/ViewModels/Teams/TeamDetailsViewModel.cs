using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Agent;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using System.Linq;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Teams
{
    public class TeamDetailsNavigationParameter
    {
        public string TeamId { get; }
        public string TeamName { get; }
        public bool IsOwner { get; }

        public TeamDetailsNavigationParameter(string teamId, string teamName, bool isOwner)
        {
            TeamId = teamId;
            TeamName = teamName;
            IsOwner = isOwner;
        }
    }

    public class TeamDetailsViewModel : BaseViewModel, IMvxViewModel<TeamDetailsNavigationParameter>
    {
        private readonly TeamsDatabase _teamsDatabase;
        private readonly IMvxNavigationService _navigationService;
        private readonly IUserDialogs _userDialogs;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IMvxMessenger _messenger;

        private string _teamId = string.Empty;

        public TeamDetailsViewModel(TeamsDatabase teamsDatabase, IMvxNavigationService navigationService, IUserDialogs userDialogs,
            WasabeeApiV1Service wasabeeApiV1Service, IMvxMessenger messenger)
        {
            _teamsDatabase = teamsDatabase;
            _navigationService = navigationService;
            _userDialogs = userDialogs;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _messenger = messenger;
        }

        public void Prepare(TeamDetailsNavigationParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.TeamId))
                return;

            Title = parameter.TeamName;

            _teamId = parameter.TeamId;
            IsOwner = parameter.IsOwner;
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            await base.Initialize();

            // force PropertyChanged as UI not updated when done in Prepare()
            await RaisePropertyChanged(() => IsOwner);

            if (string.IsNullOrWhiteSpace(_teamId))
                return;

            RefreshCommand.Execute();
        }

        #region Properties

        public bool IsAddingAgent { get; set; }
        public bool IsOwner { get; set; }
        public bool IsRefreshing { get; set; }
        public TeamModel Team { get; set; } = new TeamModel();

        #endregion

        #region Commands

        public IMvxCommand RefreshCommand => new MvxCommand(async () => await RefreshExecuted());
        private async Task RefreshExecuted()
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.RefreshCommand");

            if (IsRefreshing)
                return;

            IsRefreshing = true;

            var updatedTeam = await _wasabeeApiV1Service.Teams_GetTeam(_teamId);
            if (updatedTeam != null)
            {
                await _teamsDatabase.SaveTeamModel(updatedTeam);
                Team = updatedTeam;
            }
            else
            {
                var localData = await _teamsDatabase.GetTeam(_teamId);
                if (localData != null)
                    Team = localData;
            }

            Title = Team.Name;
            IsRefreshing = false;
        }

        public IMvxAsyncCommand<AgentModel> ShowAgentCommand => new MvxAsyncCommand<AgentModel>(ShowAgentExecuted);
        private async Task ShowAgentExecuted(AgentModel agent)
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.ShowAgentCommand");

            IsAddingAgent = false;

            if (IsBusy)
                return;

            IsBusy = true;
            
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<ProfileViewModel>(), new ProfileViewModelNavigationParameter(agent.Id));

            IsBusy = false;
        }

        public IMvxAsyncCommand<AgentModel> RemoveAgentCommand => new MvxAsyncCommand<AgentModel>(RemoveAgentExecuted);
        private async Task RemoveAgentExecuted(AgentModel agent)
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.RemoveAgentCommand");

            IsAddingAgent = false;

            if (IsBusy)
                return;

            if (await _userDialogs.ConfirmAsync(
                string.Format(Strings.TeamDetail_Warning_RemoveAgent, agent.Name), 
                string.Empty, 
                Strings.Global_Yes,
                Strings.Global_Cancel))
            {
                var result = await _wasabeeApiV1Service.Teams_RemoveAgentFromTeam(Team.Id, agent.Id);
                if (result)
                    RefreshCommand.Execute();
                else
                    _userDialogs.Toast(Strings.Global_ErrorOccuredPleaseRetry);
            }
        }

        public IMvxCommand<string> AddAgentFromQrCodeCommand => new MvxCommand<string>(async (qrCodeData) => await AddAgentFromQrCodeExecuted(qrCodeData));
        private async Task AddAgentFromQrCodeExecuted(string qrCodeData)
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.AddAgentFromQrCodeCommand");

            IsAddingAgent = false;

            if (string.IsNullOrEmpty(qrCodeData))
                return;

            if (qrCodeData.StartsWith("wasabee:"))
            {
                var userId = qrCodeData.Substring(8, qrCodeData.Length - 8);
                await AddAgentByNameOrIdToTeam(userId);
            }
        }

        public IMvxCommand PromptAddUserAgentCommand => new MvxCommand(PromptAddUserAgentExecuted);
        private async void PromptAddUserAgentExecuted()
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.PromptAddUserAgentCommand");

            IsAddingAgent = false;

            var promptResult = await _userDialogs.PromptAsync(new PromptConfig()
            {
                InputType = InputType.Name,
                OkText = Strings.Global_Add,
                CancelText = Strings.Global_Cancel,
                Title = Strings.TeamDetail_Prompt_AddAgentTitle,
            });

            if (promptResult.Ok && !string.IsNullOrWhiteSpace(promptResult.Text))
                await AddAgentByNameOrIdToTeam(promptResult.Text);
        }

        public IMvxCommand EditTeamNameCommand => new MvxCommand(EditTeamNameExecuted);
        private async void EditTeamNameExecuted()
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.EditTeamNameCommand");

            var promptResult = await _userDialogs.PromptAsync(new PromptConfig()
            {
                InputType = InputType.Name,
                OkText = Strings.Global_Ok,
                CancelText = Strings.Global_Cancel,
                Title = Strings.Teams_Prompt_ChangeTeamNameTitle
            });

            if (promptResult.Ok && !string.IsNullOrWhiteSpace(promptResult.Text))
            {
                var result = await _wasabeeApiV1Service.Teams_RenameTeam(Team.Id, promptResult.Text);
                if (result)
                {
                    RefreshCommand.Execute();
                    _messenger.Publish(new MessageFor<TeamsListViewModel>(this));
                }
                else
                    _userDialogs.Toast(Strings.Global_ErrorOccuredPleaseRetry);
            }
        }

        #endregion

        #region Private methods

        private async Task AddAgentByNameOrIdToTeam(string nameOrId)
        {
            var user = Team.Agents.FirstOrDefault(x => x.Id.Equals(nameOrId) || x.Name.Equals(nameOrId)) ?? null;
            if (user != null)
            {
                _userDialogs.Toast(string.Format(Strings.TeamDetail_Warning_AgentAlreadyInTeam, nameOrId));
                return;
            }

            var result = await _wasabeeApiV1Service.Teams_AddAgentToTeam(Team.Id, nameOrId);
            if (result)
                RefreshCommand.Execute();
            else
                _userDialogs.Toast(Strings.TeamDetail_Warning_AgentNotFound);
        }

        #endregion
    }
}