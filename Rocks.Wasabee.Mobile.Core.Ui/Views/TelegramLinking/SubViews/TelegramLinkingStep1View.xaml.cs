using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.TelegramLinking.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TelegramLinkingStep1View : BaseContentView<TelegramLinkingStep1SubViewModel>
    {
        public TelegramLinkingStep1View()
        {
            InitializeComponent();
        }

        private void DontAskAgainLabel_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsDontAskAgainChecked = !ViewModel.IsDontAskAgainChecked;
        }
    }
}