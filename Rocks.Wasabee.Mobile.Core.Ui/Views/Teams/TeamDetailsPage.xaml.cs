using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Teams
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = false)]
    public partial class TeamDetailsPage : BaseContentPage<TeamDetailsViewModel>
    {
        public TeamDetailsPage()
        {
            InitializeComponent();
        }
        private void AgentsList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            if (e.SelectedItem is TeamAgentModel item)
            {
                ViewModel.ShowAgentCommand.Execute(item);
            }

            AgentsList.SelectedItem = null;
        }
    }
}