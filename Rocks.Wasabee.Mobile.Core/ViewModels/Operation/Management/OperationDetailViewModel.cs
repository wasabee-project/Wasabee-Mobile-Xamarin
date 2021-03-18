using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using MvvmCross;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.Management
{
    public class OperationDetailNavigationParameter
    {
        public string OpId { get; }

        public OperationDetailNavigationParameter(string opId)
        {
            OpId = opId;
        }
    }

    public class OperationDetailViewModel : BaseViewModel, IMvxViewModel<OperationDetailNavigationParameter>
    {
        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly UsersDatabase _usersDatabase;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IMvxNavigationService _navigationService;

        private OperationDetailNavigationParameter? _parameter;

        public OperationDetailViewModel(OperationsDatabase operationsDatabase, TeamsDatabase teamsDatabase, UsersDatabase usersDatabase,
            IUserSettingsService userSettingsService, IMvxNavigationService navigationService)
        {
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _usersDatabase = usersDatabase;
            _userSettingsService = userSettingsService;
            _navigationService = navigationService;
        }

        public void Prepare(OperationDetailNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            await base.Initialize();

            if (_parameter == null || string.IsNullOrWhiteSpace(_parameter.OpId))
            {
                await _navigationService.Close(this);
                return;
            }

            LoadOperationCommand.Execute();
        }

        #region Properties

        public OperationModel? Operation { get; set; }
        public MvxObservableCollection<Team> TeamsCollection { get; set; } = new MvxObservableCollection<Team>();

        #endregion

        #region Commands

        public IMvxCommand LoadOperationCommand => new MvxCommand(LoadOperationExecuted);
        private async void LoadOperationExecuted()
        {
            if (IsBusy)
                return;

            LoggingService.Trace("Executing OperationDetailViewModel.LoadOperationCommand");

            IsBusy = true;

            try
            {
                var op = await _operationsDatabase.GetOperationModel(_parameter!.OpId);
                if (op == null)
                    return;

                Operation = op;
                TeamsCollection.Clear();

                var loggedUserId = _userSettingsService.GetLoggedUserGoogleId();
                var userTeams = await _usersDatabase.GetUserTeams(loggedUserId);
                var teamsIds = Operation.TeamList.Select(x => x.TeamId).ToList();
                foreach (var teamId in teamsIds)
                {
                    var team = await _teamsDatabase.GetTeam(teamId);
                    if (team == null)
                    {
                        TeamsCollection.Add(new Team(teamId, "Error retrieving name"));
                    }
                    else
                    {
                        var isOwner = userTeams.Any(x => x.Id.Equals(team.Id) && x.Owner.Equals(loggedUserId));
                        TeamsCollection.Add(new Team(team.Id, team.Name) { IsOwner = isOwner });
                    }
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing OperationDetailViewModel.LoadOperationCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxCommand<Team> ShowTeamDetailCommand => new MvxCommand<Team>(ShowTeamDetailExecuted);
        private async void ShowTeamDetailExecuted(Team team)
        {
            if (IsBusy)
                return;

            LoggingService.Trace("Executing OperationDetailViewModel.ShowTeamDetailCommand");

            IsBusy = true;

            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<TeamDetailsViewModel>(), new TeamDetailsNavigationParameter(team.Id, team.Name, team.IsOwner));

            IsBusy = false;
        }

        #endregion
    }

    public class Team : MvxViewModel
    {
        public string Id { get; }
        public string Name { get; }

        public Team(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool IsOwner { get; set; } = false;
    }
}