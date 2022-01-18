using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using Rocks.Wasabee.Mobile.Core.ViewModels.Settings;
using System;
using Xamarin.Forms;
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

            if (Device.RuntimePlatform == Device.Android)
            {
                AppSection.Add(CreateTextCell("Settings_Label_DarkMode", (_, _) => ViewModel.SwitchThemeCommand.Execute()));
                AppSection.Add(CreateTextCell("Settings_Label_ChangeLanguage", (_, _) => ViewModel.SwitchLanguageCommand.Execute()));
            }
        }

        private void AnalyticsCell_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsAnonymousAnalyticsEnabled = !ViewModel.IsAnonymousAnalyticsEnabled;
        }

        private void SendLogsCell_OnTapped(object sender, EventArgs e)
        {
            ViewModel.SendLogsCommand.Execute();
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

        private TextCell CreateTextCell(string textTranslationKey, EventHandler tappedAction)
        {
            var cell = new TextCell() { Text = TranslateExtension.GetValue(textTranslationKey) };
            cell.SetDynamicResource(TextCell.TextColorProperty, "PrimaryTextColor");
            cell.Tapped += tappedAction;

            return cell;
        }
    }
}