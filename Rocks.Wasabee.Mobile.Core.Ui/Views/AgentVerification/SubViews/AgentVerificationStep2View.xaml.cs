using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.AgentVerification.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgentVerificationStep2View : BaseContentView<AgentVerificationStep2SubViewModel>
    {
        public AgentVerificationStep2View()
        {
            InitializeComponent();
        }
    }
}