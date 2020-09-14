using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Settings;
using System;
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

        private void AnalyticsCell_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsAnonymousAnalyticsEnabled = !ViewModel.IsAnonymousAnalyticsEnabled;
        }
    }
}