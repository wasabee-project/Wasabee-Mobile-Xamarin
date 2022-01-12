using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking;
using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.TelegramLinking
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxModalPresentation(Animated = true, WrapInNavigationPage = false)]
    public partial class TelegramLinkingPage : MvxContentPage<TelegramLinkingViewModel>
    {
        public TelegramLinkingPage()
        {
            InitializeComponent();
        }
    }

    public class TelegramLinkingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Step1Template { get; set; }
        public DataTemplate Step2Template { get; set; }
        public DataTemplate Step3Template { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                TelegramLinkingStep1SubViewModel => Step1Template,
                TelegramLinkingStep2SubViewModel => Step2Template,
                TelegramLinkingStep3SubViewModel => Step3Template,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };
        }
    }
}