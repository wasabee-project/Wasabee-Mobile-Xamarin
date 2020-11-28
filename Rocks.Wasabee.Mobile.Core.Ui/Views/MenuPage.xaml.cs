using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(MasterDetailPosition.Master)]
    public partial class MenuPage : MvxContentPage<MenuViewModel>
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private void Logout_Clicked(object sender, EventArgs e)
        {
            CloseMenu();
            ViewModel.LogoutCommand.Execute();
        }

        private void MenuList_OnItemTapped(object sender, EventArgs args)
        {
            CloseMenu();
        }

        private void CloseMenu()
        {
            if (Parent is MasterDetailPage md)
            {
                md.MasterBehavior = MasterBehavior.Popover;
                md.IsPresented = !md.IsPresented;
            }
        }

        private void ChangeOp_Clicked(object sender, EventArgs e)
        {
            CloseMenu();
            ViewModel.ChangeSelectedOpCommand.Execute();
        }

        private void Refresh_Clicked(object sender, EventArgs e)
        {
            ViewModel.PullOpsFromServerCommand.Execute();
        }
    }
}