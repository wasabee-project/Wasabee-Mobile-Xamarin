using MvvmCross.Forms.Presenters.Attributes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(MasterDetailPosition.Root, Animated = false)]
    public partial class RootPage
    {
        public RootPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        }
    }
}