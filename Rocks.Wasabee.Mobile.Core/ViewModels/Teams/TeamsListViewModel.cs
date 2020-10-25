using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Teams
{
    public class TeamsListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly UsersDatabase _usersDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        public TeamsListViewModel(IUserDialogs userDialogs, IUserSettingsService userSettingsService, IMvxNavigationService navigationService,
            UsersDatabase usersDatabase, TeamsDatabase teamsDatabase, WasabeeApiV1Service wasabeeApiV1Service)
        {
            _userDialogs = userDialogs;
            _userSettingsService = userSettingsService;
            _navigationService = navigationService;
            _usersDatabase = usersDatabase;
            _teamsDatabase = teamsDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;
        }

        public override async Task Initialize()
        {
            LoggingService.Trace("Navigated to ProfileViewModel");

            await base.Initialize();

            RefreshCommand.Execute();
        }

        #region Properties

        public bool IsRefreshing { get; set; }
        public MvxObservableCollection<Team> TeamsCollection { get; set; } = new MvxObservableCollection<Team>();

        #endregion

        #region Commands

        public IMvxCommand RefreshCommand => new MvxCommand(async () => await RefreshExecuted());
        private async Task RefreshExecuted()
        {
            LoggingService.Trace("Executing TeamsListViewModel.RefreshCommand");

            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;

                var userModel = await _wasabeeApiV1Service.User_GetUserInformations();
                if (userModel != null)
                {
                    await _usersDatabase.SaveUserModel(userModel);
                    TeamsCollection = new MvxObservableCollection<Team>(userModel.Teams.Select(x =>
                        new Team(this, x.Name, x.Id)
                        {
                            IsEnabled = x.State.Equals("On"),
                            IsOwner = x.Owner.Equals(_userSettingsService.GetLoggedUserGoogleId())
                        }
                    ));
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing TeamsListViewModel.RefreshCommand");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public IMvxAsyncCommand<Team> SwitchTeamStateCommand => new MvxAsyncCommand<Team>(SwitchTeamStateExecuted);
        private async Task SwitchTeamStateExecuted(Team team)
        {
            LoggingService.Trace("Executing TeamsListViewModel.SwitchTeamStateCommand");

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var result = await _wasabeeApiV1Service.User_ChangeTeamState(team.Id, team.IsEnabled ? "Off" : "On");
                if (result)
                {
                    team.IsEnabled = !team.IsEnabled;
                    _userDialogs.Toast($"Location sharing state changed for team {team.Name}", TimeSpan.FromSeconds(3));

                    var updatedTeam = await _wasabeeApiV1Service.Teams_GetTeam(team.Id);
                    if (updatedTeam != null)
                    {
                        await _teamsDatabase.SaveTeamModel(updatedTeam);
                    }
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing TeamsListViewModel.SwitchTeamStateCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxAsyncCommand<Team> ShowTeamDetailCommand => new MvxAsyncCommand<Team>(ShowTeamDetailExecuted);
        private async Task ShowTeamDetailExecuted(Team team)
        {
            LoggingService.Trace("Executing TeamsListViewModel.ShowTeamDetailCommand");

            await _navigationService.Navigate<TeamDetailsViewModel, TeamDetailsNavigationParameter>(
                new TeamDetailsNavigationParameter(team.Id, team.IsOwner));
        }

        #endregion
    }

    public class Team : MvxNotifyPropertyChanged
    {
        public TeamsListViewModel Parent { get; }
        public string Name { get; }
        public string Id { get; }

        public Team(TeamsListViewModel parent, string name, string id)
        {
            Parent = parent;
            Name = name;
            Id = id;
        }

        public bool IsEnabled { get; set; }
        public bool IsOwner { get; set; }
    }
}