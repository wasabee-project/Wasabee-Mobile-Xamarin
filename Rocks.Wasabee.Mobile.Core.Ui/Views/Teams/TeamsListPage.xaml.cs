using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
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

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            TeamsList.SelectedItem = null;

            if (sender is BindableObject { BindingContext: Team team })
            {
                await ViewModel.ShowTeamDetailCommand.ExecuteAsync(team);
            }
        }
    }
}