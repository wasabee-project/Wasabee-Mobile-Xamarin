using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation.Management;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation.Management
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class OperationsListPage : BaseContentPage<OperationsListViewModel>
    {
        private readonly ToolbarItem _showHiddenOpsToolbarItem;

        public OperationsListPage()
        {
            InitializeComponent();

            _showHiddenOpsToolbarItem = new ToolbarItem("Show/Hide hidden OPs", "eyeoff.png", () => ShowHideLocalOps());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!ToolbarItems.Contains(_showHiddenOpsToolbarItem))
                ToolbarItems.Add(_showHiddenOpsToolbarItem);
        }

        private void ShowHideLocalOps()
        {
            ViewModel.ShowHiddenOps = !ViewModel.ShowHiddenOps;
            _showHiddenOpsToolbarItem.IconImageSource = ViewModel.ShowHiddenOps switch
            {
                true => "eye.png",
                false => "eyeoff.png"
            };
        }

        private void OperationsList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            OperationsList.SelectedItem = null;

            if (e.SelectedItem is ViewModels.Operation.Management.Operation operation)
            {
                ViewModel.ShowOperationDetailCommand.Execute(operation);
            }
        }
    }
}