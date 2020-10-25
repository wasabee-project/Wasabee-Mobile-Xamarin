using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Teams
{
    public class TeamDetailsNavigationParameter
    {
        public string TeamId { get; }
        public bool IsOwner { get; }

        public TeamDetailsNavigationParameter(string teamId, bool isOwner)
        {
            TeamId = teamId;
            IsOwner = isOwner;
        }
    }

    public class TeamDetailsViewModel : BaseViewModel, IMvxViewModel<TeamDetailsNavigationParameter>
    {
        private readonly TeamsDatabase _teamsDatabase;

        private string _teamId = string.Empty;
        private bool _isOwner = false;

        public TeamDetailsViewModel(TeamsDatabase teamsDatabase)
        {
            _teamsDatabase = teamsDatabase;
        }

        public void Prepare(TeamDetailsNavigationParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.TeamId))
                return;

            _teamId = parameter.TeamId;
            _isOwner = parameter.IsOwner;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            if (string.IsNullOrWhiteSpace(_teamId))
                return;

            var team = await _teamsDatabase.GetTeam(_teamId);
            Team = team ?? new TeamModel();
        }

        #region Properties

        public TeamModel? Team { get; set; }

        #endregion
    }
}