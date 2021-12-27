using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using System;
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

        private void DontAskAgainLabel_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsDontAskAgainChecked = !ViewModel.IsDontAskAgainChecked;
        }
    }
}