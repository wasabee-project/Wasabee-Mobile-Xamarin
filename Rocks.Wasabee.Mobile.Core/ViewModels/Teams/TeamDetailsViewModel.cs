using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
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

        public IMvxAsyncCommand<TeamAgentModel> ShowAgentCommand => new MvxAsyncCommand<TeamAgentModel>(ShowAgentExecuted);
        private async Task ShowAgentExecuted(TeamAgentModel agent)
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.ShowAgentCommand");

            IsAddingAgent = false;

            if (IsBusy)
                return;

            IsBusy = true;

            await _navigationService.Navigate<ProfileViewModel, ProfileViewModelNavigationParameter>(
                new ProfileViewModelNavigationParameter(agent.Id));

            IsBusy = false;
        }

        public IMvxAsyncCommand<TeamAgentModel> RemoveAgentCommand => new MvxAsyncCommand<TeamAgentModel>(RemoveAgentExecuted);
        private async Task RemoveAgentExecuted(TeamAgentModel agent)
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.RemoveAgentCommand");

            IsAddingAgent = false;

            if (IsBusy)
                return;

            if (await _userDialogs.ConfirmAsync($"Remove '{agent.Name}' from the team ?", string.Empty, "Yes", "Cancel"))
            {
                var result = await _wasabeeApiV1Service.Teams_RemoveAgentFromTeam(Team.Id, agent.Id);
                if (result)
                    RefreshCommand.Execute();
                else
                    _userDialogs.Toast("Error removing agent");
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
                var user = Team.Agents.FirstOrDefault(x => x.Id.Equals(userId)) ?? null;
                if (user != null)
                {
                    _userDialogs.Toast($"{user.Name} is already in the team !");
                    return;
                }

                var result = await _wasabeeApiV1Service.Teams_AddAgentToTeam(Team.Id, userId);
                if (result)
                    RefreshCommand.Execute();
                else
                    _userDialogs.Toast("Agent not found");
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
                OkText = "Add",
                CancelText = "Cancel",
                Title = "Agent name",
            });

            if (promptResult.Ok && !string.IsNullOrWhiteSpace(promptResult.Text))
            {
                var result = await _wasabeeApiV1Service.Teams_AddAgentToTeam(Team.Id, promptResult.Text);
                if (result)
                    RefreshCommand.Execute();
                else
                    _userDialogs.Toast("Agent not found or already in team");
            }
        }

        public IMvxCommand EditTeamNameCommand => new MvxCommand(EditTeamNameExecuted);
        private async void EditTeamNameExecuted()
        {
            LoggingService.Trace("Executing TeamDetailsViewModel.EditTeamNameCommand");

            var promptResult = await _userDialogs.PromptAsync(new PromptConfig()
            {
                InputType = InputType.Name,
                OkText = "Ok",
                CancelText = "Cancel",
                Title = "Change team name",
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
                    _userDialogs.Toast("Rename failed");
            }
        }

        #endregion
    }
}