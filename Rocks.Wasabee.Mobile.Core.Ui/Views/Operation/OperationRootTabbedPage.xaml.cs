using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(Position = MasterDetailPosition.Detail, NoHistory = true)]
    public partial class OperationRootTabbedPage : MvxTabbedPage<OperationRootTabbedViewModel>
    {
        private bool _firstTime = true;

        public OperationRootTabbedPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, true);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_firstTime)
            {
                ViewModel.ShowInitialViewModelsCommand.ExecuteAsync();
                _firstTime = false;
            }
        }
    }
}