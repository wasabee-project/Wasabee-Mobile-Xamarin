using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads;
using Rocks.Wasabee.Mobile.Core.Services;
using System;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class TargetWarningDialogNavigationParameter
    {
        public TargetPayload Payload { get; }

        public TargetWarningDialogNavigationParameter(TargetPayload payload)
        {
            Payload = payload;
        }
    }

    public class TargetWarningDialogViewModel : BaseDialogViewModel, IMvxViewModel<TargetWarningDialogNavigationParameter>
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IClipboard _clipboard;
        private readonly IMap _map;

        public TargetWarningDialogViewModel(IDialogNavigationService dialogNavigationService, IUserDialogs userDialogs,
            IClipboard clipboard, IMap map) : base(dialogNavigationService)
        {
            _userDialogs = userDialogs;
            _clipboard = clipboard;
            _map = map;
        }
        
        public void Prepare(TargetWarningDialogNavigationParameter parameter)
        {
            Payload = parameter.Payload;

            TargetGoal = GetTargetGoalFromPayloadType(Payload.Type);
        }

        #region Properties

        public TargetPayload? Payload { get; private set; }

        public string TargetGoal { get; set; } = string.Empty;
        
        #endregion

        #region Commands

        public IMvxCommand OpenInNavigationAppCommand => new MvxCommand(OpenInNavigationAppExecuted);
        private async void OpenInNavigationAppExecuted()
        {
            if (IsBusy || Payload is null) return;

            LoggingService.Trace("Executing TargetWarningDialogViewModel.OpenInNavigationAppCommand");

            try
            {
                var culture = CultureInfo.GetCultureInfo("en-US");
                double.TryParse(Payload.Latitude, NumberStyles.Float, culture, out var lat);
                double.TryParse(Payload.Longitude, NumberStyles.Float, culture, out var lng);
                        
                var location = new Location(lat, lng);
                var coordinates = $"{Payload.Latitude},{Payload.Longitude}";
                
                await _clipboard.SetTextAsync(coordinates);
                if (_clipboard.HasText)
                    _userDialogs.Toast("Coordinates copied to clipboartd.");

                await _map.OpenAsync(location);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Executing TargetWarningDialogViewModel.OpenInNavigationAppCommand");
            }
        }

        public IMvxCommand IgnoreCommand => new MvxCommand(IgnoreExecuted);
        private async void IgnoreExecuted()
        {
            if (IsBusy) return;

            LoggingService.Trace("Executing TargetWarningDialogViewModel.IgnoreCommand");

            await CloseCommand.ExecuteAsync();
        }

        #endregion

        #region Private methods

        private static string GetTargetGoalFromPayloadType(string payloadType)
        {
            return payloadType switch
            {
                "DestroyPortalAlert" => "Destroy portal",
                "UseVirusPortalAlert" => "Use virus",
                "CapturePortalMarker" => "Capture portal",
                "FarmPortalMarker" => "Farm keys",
                "LetDecayPortalAlert" => "Let decay",
                "MeetAgentPortalMarker" => "Meet Agent",
                "OtherPortalAlert" => "Other",
                "RechargePortalAlert" => "Recharge portal",
                "UpgradePortalAlert" => "Upgrade portal",
                "CreateLinkAlert" => "Create link",
                "ExcludeMarker" => "Exclude Marker",
                "GetKeyPortalMarker" => "Get Key",
                "GotoPortalMarker" => "Go to portal",
                "anchor"  => "OP Anchor",
                _ => "Unknown"
            };
        }

        #endregion
    }
}