using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
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
    }
}