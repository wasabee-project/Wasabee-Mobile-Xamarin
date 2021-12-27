using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.AgentVerification.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AgentVerificationStep3View : BaseContentView<AgentVerificationStep3SubViewModel>
    {
        public AgentVerificationStep3View()
        {
            InitializeComponent();
        }
    }
}