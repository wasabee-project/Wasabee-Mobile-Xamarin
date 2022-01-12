using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.TelegramLinking.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TelegramLinkingStep3View : BaseContentView<TelegramLinkingStep3SubViewModel>
    {
        public TelegramLinkingStep3View()
        {
            InitializeComponent();
        }
    }
}