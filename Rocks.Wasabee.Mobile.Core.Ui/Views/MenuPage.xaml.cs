using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(MasterDetailPosition.Master)]
    public partial class MenuPage : BaseContentPage<MenuViewModel>
    {
        private readonly MvxSubscriptionToken _toggleMenuToken;

        public MenuPage()
        {
            InitializeComponent();

            _toggleMenuToken = Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<MessageFrom<MenuViewModel>>(_ => ToggleMenu());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.ViewAppeared();
        }

        private void Logout_Clicked(object sender, EventArgs e)
        {
            ToggleMenu();
            ViewModel.LogoutCommand.Execute();
        }

        private void MenuList_OnItemTapped(object sender, EventArgs args)
        {
            ToggleMenu();
        }

        private void ToggleMenu()
        {
            if (Parent is FlyoutPage flyout)
            {
                flyout.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
                flyout.IsPresented = !flyout.IsPresented;
            }
        }

        private void ChangeOp_Clicked(object sender, EventArgs e)
        {
            ToggleMenu();
            ViewModel.ChangeSelectedOpCommand.Execute();
        }

        private void Refresh_Clicked(object sender, EventArgs e)
        {
            ViewModel.PullOpsFromServerCommand.Execute();
        }
    }
}