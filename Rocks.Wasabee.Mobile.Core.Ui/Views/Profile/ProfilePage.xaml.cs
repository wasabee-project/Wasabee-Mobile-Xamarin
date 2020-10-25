using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using System;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Profile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class ProfilePage : BaseContentPage<ProfileViewModel>
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        private void EnlRocksLink_Clicked(object sender, EventArgs eventArgs)
        {
            Launcher.OpenAsync(new Uri("https://enl.rocks"));
        }

        private void ProjectVLink_Clicked(object sender, EventArgs eventArgs)
        {
            Launcher.OpenAsync(new Uri("https://v.enl.one"));
        }

        private void EnlRocksCell_Tapped(object sender, EventArgs e)
        {
            ViewModel.OpenRocksProfileCommand.Execute();
        }

        private void ProjectVCell_Tapped(object sender, EventArgs e)
        {
            ViewModel.OpenVProfileCommand.Execute();
        }
    }
}