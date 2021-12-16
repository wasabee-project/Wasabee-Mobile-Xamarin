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

        private void AgentsOnMapCell_OnTapped(object sender, EventArgs e)
        {
            ViewModel.ShowAgentsFromAnyTeam = !ViewModel.ShowAgentsFromAnyTeam;
        }

        private void HideComletedMarkersCell_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsHideCompletedMarkersEnabled = !ViewModel.IsHideCompletedMarkersEnabled;
        }

        private void ShowDebugToastsCell_OnTapped(object sender, EventArgs e)
        {
            ViewModel.ShowDebugToasts = !ViewModel.ShowDebugToasts;
        }
    }
}