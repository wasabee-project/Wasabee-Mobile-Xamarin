using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Views
{
    public partial class OperationMapPage : BaseContentPage<OperationMapViewModel>
    {
        public OperationMapPage(OperationMapViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}