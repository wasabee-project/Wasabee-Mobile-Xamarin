using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private bool _isPanelVisible = false;

        public TeamDetailsPage()
        {
            InitializeComponent();

            _addAgenToolbarItem = new ToolbarItem(string.Empty, "addpeople.png", () => ViewModel.IsAddingAgent = true);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

            if (ViewModel.IsOwner)
            {
                if (!ToolbarItems.Contains(_addAgenToolbarItem))
                    ToolbarItems.Add(_addAgenToolbarItem);
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
            if (ViewModel.IsAddingAgent)
            {
                if (_isPanelVisible) return;

                _isPanelVisible = true;
                await AddAgentPanel.TranslateTo(0, 0, 150); // Show
            }
            else
            {
                _isPanelVisible = false;
                await AddAgentPanel.TranslateTo(0, 180, 150); // Hide
            }
        }

        private void AgentsList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ViewModel.IsAddingAgent = false;

            if (e.SelectedItem == null)
                return;

            if (e.SelectedItem is TeamAgentModel item)
            {
                ViewModel.ShowAgentCommand.Execute(item);
            }

            AgentsList.SelectedItem = null;
        }

        private async void ScanQrCodeButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.IsAddingAgent = false;

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE }
            };
            var scanPage = new ZXingScannerPage(options);

            scanPage.OnScanResult += async result =>
            {
                Device.BeginInvokeOnMainThread(async () => await Navigation.PopModalAsync(false));

                if (result != null)
                    ViewModel.AddAgentFromQrCodeCommand.Execute(result.Text);
            };

            await Navigation.PushModalAsync(scanPage, false);
        }
    }
}