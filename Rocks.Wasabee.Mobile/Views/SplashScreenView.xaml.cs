using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxContentPagePresentation(Animated = false)]
    public partial class SplashScreenView : BaseContentView<SplashScreenViewModel>
    {
        public SplashScreenView()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
        
        private void ServerListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ViewModel.ChooseServerCommand.ExecuteAsync(e.Item as ServerItem);
        }
    }
}