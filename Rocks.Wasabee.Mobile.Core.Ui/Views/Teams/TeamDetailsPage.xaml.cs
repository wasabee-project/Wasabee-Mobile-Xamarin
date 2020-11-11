using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
using System.Collections.Generic;
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
        public TeamDetailsPage()
        {
            InitializeComponent();
        }

        private void AgentsList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            if (e.SelectedItem is TeamAgentModel item)
            {
                ViewModel.ShowAgentCommand.Execute(item);
            }

            AgentsList.SelectedItem = null;
        }

        private async void AddAgentButton_OnClicked(object sender, EventArgs e)
        {
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