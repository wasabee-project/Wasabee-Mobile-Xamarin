using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Teams
{
    public class TeamsListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly IMvxMessenger _messenger;
        private readonly UsersDatabase _usersDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        private MvxSubscriptionToken? _token;

        public TeamsListViewModel(IUserDialogs userDialogs, IUserSettingsService userSettingsService, IMvxNavigationService navigationService, IMvxMessenger messenger,
            UsersDatabase usersDatabase, TeamsDatabase teamsDatabase, OperationsDatabase operationsDatabase, WasabeeApiV1Service wasabeeApiV1Service)
        {
            _userDialogs = userDialogs;
            _userSettingsService = userSettingsService;
            _navigationService = navigationService;
            _messenger = messenger;
            _usersDatabase = usersDatabase;
            _teamsDatabase = teamsDatabase;
            _operationsDatabase = operationsDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            await base.Initialize();

            LoggingService.Trace("Navigated to TeamsListViewModel");
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            _token ??= _messenger.Subscribe<MessageFor<TeamsListViewModel>>(msg => RefreshCommand.Execute());

            RefreshCommand.Execute();
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            _token?.Dispose();
            _token = null;
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

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                _userDialogs.Toast(Strings.Global_NoInternet);
                return;
            }

            try
            {
                IsRefreshing = true;

                var userModel = await _wasabeeApiV1Service.User_GetUserInformations();
                if (userModel != null)
                {
                    await _usersDatabase.SaveUserModel(userModel);
                    if (userModel.Teams != null && userModel.Teams.Any())
                    {
                        TeamsCollection = new MvxObservableCollection<Team>(userModel.Teams.Select(x =>
                            new Team(x.Name, x.Id)
                            {
                                IsEnabled = x.State.Equals("On"),
                                IsOwner = x.Owner.Equals(_userSettingsService.GetLoggedUserGoogleId())
                            }
                        ));
                    }
                    else
                    {
                        TeamsCollection.Clear();
                        await RaisePropertyChanged(() => TeamsCollection);

                        await _operationsDatabase.DeleteAllExceptOwnedBy(_userSettingsService.GetLoggedUserGoogleId());

                        var operationsCount = await _operationsDatabase.CountLocalOperations();
                        if (operationsCount == 0)
                        {
                            // Leaves app
                            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<SplashScreenViewModel>(), new SplashScreenNavigationParameter(doDataRefreshOnly: true));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing TeamsListViewModel.RefreshCommand");

                _userDialogs.Toast(Strings.TeamsList_Label_ErrorLoadingTeams);
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
                    _userDialogs.Toast(string.Format(Strings.TeamsList_Toast_LocationSharingStateChanged, team.Name), TimeSpan.FromSeconds(3));

                    RefreshCommand.Execute();
                }
                else
                {
                    _userDialogs.Toast(Strings.Global_ErrorOccuredPleaseRetry, TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing TeamsListViewModel.SwitchTeamStateCommand");

                _userDialogs.Toast(Strings.Global_ErrorOccuredPleaseRetry, TimeSpan.FromSeconds(3));
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
            
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<TeamDetailsViewModel>(), new TeamDetailsNavigationParameter(team.Id, team.Name, team.IsOwner));
        }

        public IMvxAsyncCommand<Team> EditTeamNameCommand => new MvxAsyncCommand<Team>(EditTeamNameExecuted);
        private async Task EditTeamNameExecuted(Team team)
        {
            var promptResult = await _userDialogs.PromptAsync(new PromptConfig()
            {
                InputType = InputType.Name,
                OkText = Strings.Global_Ok,
                CancelText = Strings.Global_Cancel,
                Title = Strings.Teams_Prompt_ChangeTeamNameTitle,
            });

            if (promptResult.Ok && !string.IsNullOrWhiteSpace(promptResult.Text))
            {
                var result = await _wasabeeApiV1Service.Teams_RenameTeam(team.Id, promptResult.Text);
                if (result)
                    RefreshCommand.Execute();
                else
                    _userDialogs.Toast(Strings.Global_ErrorOccuredPleaseRetry);
            }

        }

        public IMvxAsyncCommand<Team> DeleteTeamCommand => new MvxAsyncCommand<Team>(DeleteTeamExecuted);
        private async Task DeleteTeamExecuted(Team team)
        {
            LoggingService.Trace("Executing TeamsListViewModel.DeleteTeamCommand");

            if (IsBusy)
                return;

            IsBusy = true;

            if (team.IsOwner is false)
                return;

            if (await _userDialogs.ConfirmAsync(
                string.Format(Strings.TeamsList_Warning_TeamWillDelete, team.Name),
                Strings.TeamsList_Warning_DeleteTeamTitle,
                Strings.Global_Yes,
                Strings.Global_Cancel))
            {
                if (await _userDialogs.ConfirmAsync(Strings.TeamsList_Warning_TeamDeleteSecondWarning, string.Empty, Strings.Global_Yes, Strings.Global_Cancel))
                {
                    if (await _userDialogs.ConfirmAsync(Strings.TeamsList_Warning_TeamDeleteLastWarning, string.Empty, Strings.Global_Delete, Strings.Global_Cancel))
                    {
                        var result = await _wasabeeApiV1Service.Teams_DeleteTeam(team.Id);
                        _userDialogs.Toast(result ? string.Format(Strings.TeamsList_Toast_TeamDeleted, team.Name) : Strings.Global_ErrorOccuredPleaseRetry);

                        if (result)
                        {
                            await _teamsDatabase.DeleteTeam(team.Id);

                            TeamsCollection.Remove(team);
                            await RaisePropertyChanged(() => TeamsCollection); 
                        }
                    }
                }
            }

            IsBusy = false;
        }

        #endregion

    }

    public class Team : MvxNotifyPropertyChanged
    {
        public string Name { get; }
        public string Id { get; }

        public Team(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public bool IsEnabled { get; set; }
        public bool IsOwner { get; set; }
    }
}