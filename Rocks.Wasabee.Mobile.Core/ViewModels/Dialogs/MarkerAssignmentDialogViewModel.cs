using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Cache;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class MarkerAssignmentDialogViewModel : BaseDialogViewModel, IMvxViewModel<MarkerAssignmentData>
    {
        private readonly IMvxMessenger _messenger;
        private readonly IUserDialogs _userDialogs;
        private readonly IMap _map;
        private readonly IClipboard _clipboard;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly MarkersDatabase _markersDatabase;
        private readonly IUserSettingsService _userSettingsService;

        public MarkerAssignmentDialogViewModel(IDialogNavigationService dialogNavigationService, IMvxMessenger messenger,
            IUserDialogs userDialogs, IMap map, IClipboard clipboard, WasabeeApiV1Service wasabeeApiV1Service,
            MarkersDatabase markersDatabase, IUserSettingsService userSettingsService) : base(dialogNavigationService)
        {
            _messenger = messenger;
            _userDialogs = userDialogs;
            _map = map;
            _clipboard = clipboard;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _markersDatabase = markersDatabase;
            _userSettingsService = userSettingsService;
        }

        public void Prepare(MarkerAssignmentData parameter)
        {
            MarkerAssignment = parameter;
            Marker = MarkerAssignment.Marker;
            
            IsSelfAssignment = _userSettingsService.GetLoggedUserGoogleId().Equals(Marker?.AssignedTo);
            UpdateButtonsState();
        }

        public override Task Initialize()
        {
            UpdateButtonsState();

            Goal = GetGoalFromMarkerType(Marker?.Type);

            return base.Initialize();
        }

        #region Properties

        public bool IsSelfAssignment { get; set; }

        public bool AcknowledgedEnabled { get; set; }
        public bool CompletedEnabled { get; set; }
        public bool IncompleteEnabled { get; set; }
        public bool ClaimEnabled{ get; set; }
        public bool RejectEnabled { get; set; }

        public string Goal { get; set; } = string.Empty;

        public MarkerAssignmentData? MarkerAssignment { get; set; }
        public MarkerModel? Marker { get; set; }

        #endregion

        #region Commands

        public IMvxAsyncCommand AckCommand => new MvxAsyncCommand(AckExecuted, () => AcknowledgedEnabled);
        private async Task AckExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.AckCommand");

                IsBusy = true;

                try
                {
                    var response = await _wasabeeApiV1Service.Operation_Marker_Acknowledge(MarkerAssignment.OpId, Marker.Id);
                    if (response != null)
                    {
                        StoreResponseUpdateId(response);
                        await UpdateMarkerAndNotify(response);
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error Executing MarkerAssignmentDialogViewModel.AckCommand");
                }

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand CompleteCommand => new MvxAsyncCommand(CompleteExecuted, () => CompletedEnabled);
        private async Task CompleteExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.CompleteCommand");

                IsBusy = true;
                
                try
                {
                    var response = await _wasabeeApiV1Service.Operation_Marker_Complete(MarkerAssignment.OpId, Marker.Id);
                    if (response != null)
                    {
                        StoreResponseUpdateId(response);
                        await UpdateMarkerAndNotify(response);

                        IsBusy = false;
                        await CloseCommand.ExecuteAsync();
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error Executing MarkerAssignmentDialogViewModel.CompleteCommand");
                }

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand IncompleteCommand => new MvxAsyncCommand(IncompleteExecuted, () => IncompleteEnabled);
        private async Task IncompleteExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.IncompleteCommand");

                IsBusy = true;
                
                try
                {
                    var response = await _wasabeeApiV1Service.Operation_Marker_Incomplete(MarkerAssignment.OpId, Marker.Id);
                    if (response != null)
                    {
                        StoreResponseUpdateId(response);
                        await UpdateMarkerAndNotify(response);
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error Executing MarkerAssignmentDialogViewModel.IncompleteCommand");
                }

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand ClaimCommand => new MvxAsyncCommand(ClaimExecuted, () => ClaimEnabled);
        private async Task ClaimExecuted()
        {
            if (IsBusy || IsSelfAssignment)
                return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.ClaimCommand");

                IsBusy = true;
                
                try
                {
                    var response = await _wasabeeApiV1Service.Operation_Marker_Claim(MarkerAssignment.OpId, Marker.Id);
                    if (response != null)
                    {
                        StoreResponseUpdateId(response);
                        await UpdateMarkerAndNotify(response);
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error Executing MarkerAssignmentDialogViewModel.ClaimCommand");
                }

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand RejectCommand => new MvxAsyncCommand(RejectExecuted, () => RejectEnabled);
        private async Task RejectExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.RejectCommand");

                IsBusy = true;
                
                try
                {
                    var response = await _wasabeeApiV1Service.Operation_Marker_Reject(MarkerAssignment.OpId, Marker.Id);
                    if (response != null)
                    {
                        _userDialogs.Toast("Assignment rejected");

                        StoreResponseUpdateId(response);
                        await UpdateMarkerAndNotify(response);

                        IsBusy = false;
                        await CloseCommand.ExecuteAsync();
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error Executing MarkerAssignmentDialogViewModel.RejectCommand");
                }

                IsBusy = false;
            }
        }

        public IMvxCommand<string> ShowOnMapCommand => new MvxCommand<string>(ShowOnMapExecuted);
        private async void ShowOnMapExecuted(string fromOrToPortal)
        {
            if (IsBusy) return;

            IsBusy = true;

            if (Marker != null)
                Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ShowMarkerOnMapMessage(this, Marker));
            
            IsBusy = false;
            await CloseCommand.ExecuteAsync();
        }

        public IMvxCommand<string> OpenInNavigationAppCommand => new MvxCommand<string>(OpenInNavigationAppExecuted);
        private async void OpenInNavigationAppExecuted(string fromOrToPortal)
        {
            if (IsBusy) return;

            LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.OpenInNavigationAppCommand");

            IsBusy = true;

            if (MarkerAssignment?.Portal == null) return;

            try
            {
                if (MarkerAssignment.Portal == null)
                    return;
                        
                var culture = CultureInfo.GetCultureInfo("en-US");
                string coordinates = $"{MarkerAssignment.Portal.Lat},{MarkerAssignment.Portal.Lng}";

                double.TryParse(MarkerAssignment.Portal.Lat, NumberStyles.Float, culture, out var lat);
                double.TryParse(MarkerAssignment.Portal.Lng, NumberStyles.Float, culture, out var lng);
                        
                Location location = new Location(lat, lng);

                if (string.IsNullOrEmpty(coordinates) is false)
                {
                    await _clipboard.SetTextAsync(coordinates);
                    if (_clipboard.HasText)
                        _userDialogs.Toast("Coordinates copied to clipboartd.");
                }
                
                await _map.OpenAsync(location);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MarkerAssignmentDialogViewModel.OpenInNavigationAppCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Local data updates to ensure Operation is always up-to-date, even if FCM is not working.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateMarkerAndNotify(WasabeeOpUpdateApiResponse response)
        {
            if (MarkerAssignment != null && Marker != null)
            {
                // Flags UpdatedId as done
                OperationsUpdatesCache.Data[response.UpdateId] = true;

                var updated = await _wasabeeApiV1Service.Operations_GetMarker(MarkerAssignment.OpId, Marker.Id);
                if (updated != null)
                {
                    Marker = updated;
                    IsSelfAssignment = _userSettingsService.GetLoggedUserGoogleId().Equals(Marker.AssignedTo);
                    
                    UpdateButtonsState();

                    await _markersDatabase.SaveMarkerModel(Marker, MarkerAssignment.OpId);

                    _messenger.Publish(new MarkerDataChangedMessage(this, Marker, MarkerAssignment.OpId));
                }
                else
                {
                    IsBusy = false;
                    await CloseCommand.ExecuteAsync();
                }
            }
        }

        private void UpdateButtonsState()
        {
            if (Marker == null)
                return;

            switch (Marker.State.ToLower())
            {
                case "pending":
                case "completed":
                    AcknowledgedEnabled = false;
                    CompletedEnabled = false;
                    IncompleteEnabled = true;
                    RejectEnabled = false;
                    ClaimEnabled = false;
                    break;
                case "assigned":
                    AcknowledgedEnabled = true;
                    CompletedEnabled = true;
                    IncompleteEnabled = false;
                    RejectEnabled = true;
                    ClaimEnabled = false;
                    break;
                case "acknowledged":
                    AcknowledgedEnabled = false;
                    CompletedEnabled = true;
                    IncompleteEnabled = false;
                    RejectEnabled = true;
                    ClaimEnabled = false;
                    break;
            }

            if (!IsSelfAssignment)
            {
                AcknowledgedEnabled = false;
                CompletedEnabled = false;
                IncompleteEnabled = false;
                RejectEnabled = false;
                ClaimEnabled = true;
            }
        }

        private string GetGoalFromMarkerType(string? markerType)
        {
            if (markerType == null)
                return string.Empty;

            switch (markerType)
            {
                case "CapturePortalMarker":
                    return "Capture";
                case "LetDecayPortalAlert":
                    return "Let Decay";
                case "DestroyPortalAlert":
                    return "Destroy";
                case "FarmPortalMarker":
                    return "Farm";
                case "GotoPortalMarker":
                    return "Go to";
                case "GetKeyPortalMarker":
                    return "Get key";
                case "CreateLinkAlert":
                    return "Create Link";
                case "MeetAgentPortalMarker":
                    return "Meet Agent";
                case "OtherPortalAlert":
                    return "Other";
                case "RechargePortalAlert":
                    return "Recharge";
                case "UpgradePortalAlert":
                    return "Upgrade";
                case "UseVirusPortalAlert":
                    return "Use virus";
            }

            return string.Empty;
        }

        private static void StoreResponseUpdateId(WasabeeOpUpdateApiResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.UpdateId))
                return;
            
            var updateId = response.UpdateId;
            OperationsUpdatesCache.Data.Add(updateId, false);
        }

        #endregion
    }
}