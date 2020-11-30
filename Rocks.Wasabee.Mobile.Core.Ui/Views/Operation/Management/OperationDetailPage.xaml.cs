using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation.Management;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation.Management
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = false)]
    public partial class OperationDetailPage : BaseContentPage<OperationDetailViewModel>
    {
        public OperationDetailPage()
        {
            InitializeComponent();
        }

        private void TeamsList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            TeamsList.SelectedItem = null;

            if (e.SelectedItem is Team team)
            {
                ViewModel.ShowTeamDetailCommand.Execute(team);
            }
        }
    }
}