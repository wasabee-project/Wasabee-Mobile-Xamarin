using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxTabbedPagePresentation(Position = TabbedPosition.Tab, NoHistory = true, Icon = "checklist.png")]
    public partial class ChecklistPage : BaseContentPage<ChecklistViewModel>
    {
        public ChecklistPage()
        {
            InitializeComponent();
        }

        private void ElementsListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is AssignmentData data)
                ViewModel.SelectElementCommand.ExecuteAsync(data);

            ElementsListView.SelectedItem = null;
        }
    }
}