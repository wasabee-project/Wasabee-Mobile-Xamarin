using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using System;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Profile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : BaseContentPage<ProfileViewModel>, IMvxOverridePresentationAttribute
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

        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            var instanceRequest = request as MvxViewModelInstanceRequest;
            if (instanceRequest?.ViewModelInstance is BaseViewModel viewModel && viewModel.HasHistory)
            {
                return new MvxMasterDetailPagePresentationAttribute() { NoHistory = false };
            }
            return new MvxMasterDetailPagePresentationAttribute() { NoHistory = true };
        }

    }
}