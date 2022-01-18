using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.TelegramLinking.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TelegramLinkingStep2View : BaseContentView<TelegramLinkingStep2SubViewModel>
    {
        public TelegramLinkingStep2View()
        {
            InitializeComponent();
        }
    }
}