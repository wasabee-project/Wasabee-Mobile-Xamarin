using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Teams
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class TeamsListPage : BaseContentPage<TeamsListViewModel>
    {
        public TeamsListPage()
        {
            InitializeComponent();
        }

        private async void TeamsList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            TeamsList.SelectedItem = null;

            if (e.SelectedItem != null && e.SelectedItem is Team team)
            {
                await ViewModel.ShowTeamDetailCommand.ExecuteAsync(team);
            }
        }

        private async void TeamsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            
            TeamsList.SelectedItem = null;
            
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var team = e.CurrentSelection.First() as Team;
                await ViewModel.ShowTeamDetailCommand.ExecuteAsync(team);
            }
        }

    }
}