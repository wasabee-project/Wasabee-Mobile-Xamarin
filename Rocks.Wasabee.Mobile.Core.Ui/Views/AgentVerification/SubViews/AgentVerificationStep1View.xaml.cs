using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.AgentVerification.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgentVerificationStep1View : BaseContentView<AgentVerificationStep1SubViewModel>
    {
        public AgentVerificationStep1View()
        {
            InitializeComponent();
        }
    }
}