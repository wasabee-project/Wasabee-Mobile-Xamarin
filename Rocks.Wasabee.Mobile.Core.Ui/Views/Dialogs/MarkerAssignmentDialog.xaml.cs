using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Dialogs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MarkerAssignmentDialog : BaseDialogPage<MarkerAssignmentDialogViewModel>
    {
        public MarkerAssignmentDialog()
        {
            InitializeComponent();
        }
    }
}