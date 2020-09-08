using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MenuItem = Rocks.Wasabee.Mobile.Core.ViewModels.MenuItem;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(MasterDetailPosition.Master)]
    public partial class MenuPage : MvxContentPage<MenuViewModel>
    {
        public MenuPage()
        {
            InitializeComponent();
            MenuList.ItemSelected += (sender, e) => { ((ListView)sender).SelectedItem = null; };
        }

        private void Logout_Clicked(object sender, EventArgs e)
        {
            CloseMenu();
            ViewModel.LogoutCommand.Execute();
        }

        private void MenuList_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (!(e.Item is MenuItem menuItem)) return;

            if (menuItem.ViewModelType == null)
                return;

            CloseMenu();

            ViewModel.SelectedMenuItem = menuItem;
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
    }
}