using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
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
    }
}