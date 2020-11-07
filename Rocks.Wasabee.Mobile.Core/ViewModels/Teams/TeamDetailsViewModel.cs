using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using System.Linq;
using System.Threading.Tasks;
using Device = Xamarin.Forms.Device;

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
        private readonly IMvxNavigationService _navigationService;
        private readonly IUserDialogs _userDialogs;

        private string _teamId = string.Empty;

        public TeamDetailsViewModel(TeamsDatabase teamsDatabase, IMvxNavigationService navigationService, IUserDialogs userDialogs)
        {
            _teamsDatabase = teamsDatabase;
            _navigationService = navigationService;
            _userDialogs = userDialogs;
        }

        public void Prepare(TeamDetailsNavigationParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.TeamId))
                return;

            _teamId = parameter.TeamId;
            IsOwner = parameter.IsOwner;
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

        public bool IsOwner { get; set; }
        public TeamModel Team { get; set; }

        #endregion

        #region Commands

        public IMvxAsyncCommand<TeamAgentModel> ShowAgentCommand => new MvxAsyncCommand<TeamAgentModel>(ShowAgentExecuted);
        private async Task ShowAgentExecuted(TeamAgentModel agent)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await _navigationService.Navigate<ProfileViewModel, ProfileViewModelNavigationParameter>(
                new ProfileViewModelNavigationParameter(agent.Id));

            IsBusy = false;
        }

        public IMvxCommand<string> AddAgentFromQrCodeCommand => new MvxCommand<string>(async (qrCodeData) => await AddAgentFromQrCodeExecuted(qrCodeData));
        private async Task AddAgentFromQrCodeExecuted(string qrCodeData)
        {
            if (string.IsNullOrEmpty(qrCodeData))
                return;
            else
            {
                if (qrCodeData.StartsWith("wasabee:"))
                {
                    var userId = qrCodeData.Substring(8, qrCodeData.Length - 8);
                    var user = Team.Agents.FirstOrDefault(x => x.Id.Equals(userId)) ?? null;
                    if (user != null)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _userDialogs.Toast($"{user.Name} is already in the team !");
                        });
                        return;
                    }

                    // TODO
                }
            }
        }

        #endregion
    }
}