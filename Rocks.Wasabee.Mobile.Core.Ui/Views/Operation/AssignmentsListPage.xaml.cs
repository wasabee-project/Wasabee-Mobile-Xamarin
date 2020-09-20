using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxTabbedPagePresentation(Position = TabbedPosition.Tab, NoHistory = true)]
    public partial class AssignmentsListPage : BaseContentPage<AssignmentsListViewModel>
    {
        public AssignmentsListPage()
        {
            InitializeComponent();
        }
    }

    public class AssignmentsListDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LinkTemplate { get; set; }
        public DataTemplate MarkerTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((AssignmentData)item).Link != null ? LinkTemplate : MarkerTemplate;
        }
    }
}