using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxContentPagePresentation(Animated = false, NoHistory = true)]
    public partial class SplashScreenPage : BaseContentPage<SplashScreenViewModel>
    {
        public SplashScreenPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ChangeOrientationMessage(this, Orientation.Portait));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ChangeOrientationMessage(this, Orientation.Any));
        }

        private void ServerListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ViewModel.ChooseServerCommand.ExecuteAsync(e.Item as ServerItem);
        }
    }
}