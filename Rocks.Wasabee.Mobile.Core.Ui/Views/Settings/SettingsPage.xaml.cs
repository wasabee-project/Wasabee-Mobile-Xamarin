using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Settings;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
    }
}