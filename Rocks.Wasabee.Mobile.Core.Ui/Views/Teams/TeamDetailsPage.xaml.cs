using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.Ui.Helpers.Extensions;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using ZXing.Net.Mobile.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Teams
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = false)]
    public partial class TeamDetailsPage : BaseContentPage<TeamDetailsViewModel>
    {
        private readonly ToolbarItem _addAgenToolbarItem;
        private readonly ToolbarItem _editTeamNameToolbarItem;

        private bool _isPanelVisible = false;

        public TeamDetailsPage()
        {
            InitializeComponent();

            _addAgenToolbarItem = new ToolbarItem(TranslateExtension.GetValue("TeamDetail_Button_AddAgent"), "addpeople.png", () => ViewModel.IsAddingAgent = true);
            _editTeamNameToolbarItem = new ToolbarItem(TranslateExtension.GetValue("TeamDetail_Button_RenameTeam"), "pencil.png", () => ViewModel.EditTeamNameCommand.Execute());
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

            if (ViewModel.IsOwner)
            {
                if (!ToolbarItems.Contains(_addAgenToolbarItem))
                    ToolbarItems.Add(_addAgenToolbarItem);
                if (!ToolbarItems.Contains(_editTeamNameToolbarItem))
                    ToolbarItems.Add(_editTeamNameToolbarItem);
            }
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsAddingAgent")
                AnimatePanel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AnimatePanel();
        }

        protected override bool OnBackButtonPressed()
        {
            if (_isPanelVisible)
            {
                _isPanelVisible = false;
                AnimatePanel();
            }

            return base.OnBackButtonPressed();
        }

        private async void AnimatePanel()
        {
            const uint duration = 150u;
            if (ViewModel.IsAddingAgent)
            {
                if (_isPanelVisible) return;

                _isPanelVisible = true;
                PanelBackground.IsVisible = true;

                await Task.WhenAll(
                    // Show
                    PanelBackground.ColorTo(Color.FromRgba(0, 0, 0, 0x00), Color.FromRgba(0, 0, 0, 0xBB), color => PanelBackground.BackgroundColor = color, duration),
                    AddAgentPanel.TranslateTo(0, 0, duration));
            }
            else
            {
                _isPanelVisible = false;
                await Task.WhenAll(
                    // Hide
                    PanelBackground.ColorTo(Color.FromRgba(0, 0, 0, 0xBB), Color.FromRgba(0, 0, 0, 0x00), color => PanelBackground.BackgroundColor = color, duration),
                    AddAgentPanel.TranslateTo(0, 180, duration));

                PanelBackground.IsVisible = false;
            }
        }

        private async void ScanQrCodeButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.IsAddingAgent = false;

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE }
            };
            var scanPage = new ZXingScannerPage(options);

            scanPage.OnScanResult += result =>
            {
                Device.BeginInvokeOnMainThread(async () => await Navigation.PopModalAsync(false));

                if (result != null)
                    ViewModel.AddAgentFromQrCodeCommand.Execute(result.Text);
            };

            await Navigation.PushModalAsync(scanPage, false);
        }

        private void PanelBackground_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsAddingAgent = false;
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            ViewModel.IsAddingAgent = false;

            AgentsList.SelectedItem = null;

            if (sender is BindableObject { BindingContext: TeamAgentModel agent })
            {
                await ViewModel.ShowAgentCommand.ExecuteAsync(agent);
            }
        }
    }
}