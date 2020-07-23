using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation()]
    public partial class HomeView : BaseContentView<HomeViewModel>
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}