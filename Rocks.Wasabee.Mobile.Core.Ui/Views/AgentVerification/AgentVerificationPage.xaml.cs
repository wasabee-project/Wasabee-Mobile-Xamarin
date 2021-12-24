using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.AgentVerification
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxContentPagePresentation(Animated = false, WrapInNavigationPage = false)]
    public partial class AgentVerificationPage : MvxContentPage<AgentVerificationViewModel>
    {
        public AgentVerificationPage()
        {
            InitializeComponent();
        }
    }

    public class AgentVerificationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Step1Template { get; set; }
        public DataTemplate Step2Template { get; set; }
        public DataTemplate Step3Template { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                AgentVerificationStep1SubViewModel => Step1Template,
                AgentVerificationStep2SubViewModel => Step2Template,
                AgentVerificationStep3SubViewModel => Step3Template,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };
        }
    }
}