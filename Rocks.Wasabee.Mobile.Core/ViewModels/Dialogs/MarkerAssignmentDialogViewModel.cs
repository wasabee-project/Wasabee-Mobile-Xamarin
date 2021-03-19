using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class MarkerAssignmentDialogViewModel : BaseDialogViewModel, IMvxViewModel<MarkerAssignmentData>
    {
        private readonly IDialogNavigationService _dialogNavigationService;
        private readonly IMvxMessenger _messenger;
        private readonly IUserDialogs _userDialogs;
        private readonly IClipboard _clipboard;
        private readonly ILauncher _launcher;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly MarkersDatabase _markersDatabase;

        public MarkerAssignmentDialogViewModel(IDialogNavigationService dialogNavigationService, IMvxMessenger messenger, IUserDialogs userDialogs,
            IClipboard clipboard, ILauncher launcher, WasabeeApiV1Service wasabeeApiV1Service, MarkersDatabase markersDatabase) : base(dialogNavigationService)
        {
            _dialogNavigationService = dialogNavigationService;
            _messenger = messenger;
            _userDialogs = userDialogs;
            _clipboard = clipboard;
            _launcher = launcher;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _markersDatabase = markersDatabase;
        }

        public void Prepare(MarkerAssignmentData parameter)
        {
            MarkerAssignment = parameter;
            Marker = MarkerAssignment.Marker;
        }

        public override Task Initialize()
        {
            UpdateButtonsState();

            Goal = GetGoalFromMarkerType(Marker?.Type);

            return base.Initialize();
        }

        #region Properties

        public bool AcknowledgedEnabled { get; set; }
        public bool CompletedEnabled { get; set; }
        public bool IncompleteEnabled { get; set; }

        public string Goal { get; set; } = string.Empty;

        public MarkerAssignmentData? MarkerAssignment { get; set; }
        public MarkerModel? Marker { get; set; }

        #endregion

        #region Commands

        public IMvxAsyncCommand AckCommand => new MvxAsyncCommand(AckExecuted, () => AcknowledgedEnabled);
        private async Task AckExecuted()
        {
            if (IsBusy) return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.AckCommand");

                IsBusy = true;

                if (await _wasabeeApiV1Service.Operation_Marker_Acknowledge(MarkerAssignment.OpId, Marker.Id))
                    await UpdateMarkerAndNotify();

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand DoneCommand => new MvxAsyncCommand(DoneExecuted, () => CompletedEnabled);
        private async Task DoneExecuted()
        {
            if (IsBusy) return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.DoneCommand");

                IsBusy = true;

                if (await _wasabeeApiV1Service.Operation_Marker_Complete(MarkerAssignment.OpId, Marker.Id))
                    await UpdateMarkerAndNotify();

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand IncompleteCommand => new MvxAsyncCommand(IncompleteExecuted, () => IncompleteEnabled);
        private async Task IncompleteExecuted()
        {
            if (IsBusy) return;

            if (MarkerAssignment != null && Marker != null)
            {
                LoggingService.Trace("Executing MarkerAssignmentDialogViewModel.IncompleteCommand");

                IsBusy = true;

                if (await _wasabeeApiV1Service.Operation_Marker_Incomplete(MarkerAssignment.OpId, Marker.Id))
                    await UpdateMarkerAndNotify();

                IsBusy = false;
            }
        }

        public IMvxCommand<string> ShowOnMapCommand => new MvxCommand<string>(ShowOnMapExecuted);
        private void ShowOnMapExecuted(string fromOrToPortal)
        {
            if (IsBusy) return;

            IsBusy = true;

            if (Marker != null)
                Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ShowMarkerOnMapMessage(this, Marker));

            CloseCommand.Execute();

            IsBusy = false;
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
                var coordinates = $"{MarkerAssignment.Portal.Lat},{MarkerAssignment.Portal.Lng}";
                var uri = Device.RuntimePlatform switch
                {
                    Device.Android => $"https://www.google.com/maps/search/?api=1&query={coordinates}", 
                    Device.iOS => "https://maps.apple.com/?ll={coordinates}",
                    _ => throw new ArgumentOutOfRangeException(Device.RuntimePlatform)
                };

                if (string.IsNullOrWhiteSpace(uri))
                    return;

                await _clipboard.SetTextAsync(coordinates);
                _userDialogs.Toast("Coordinates copied to clipboard.");

                if (await _launcher.CanOpenAsync(uri))
                    await _launcher.OpenAsync(uri);
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

        private async Task UpdateMarkerAndNotify()
        {
            if (MarkerAssignment != null && Marker != null)
            {
                var updated = await _wasabeeApiV1Service.Operations_GetMarker(MarkerAssignment.OpId, Marker.Id);
                if (updated != null)
                {
                    Marker = updated;
                    UpdateButtonsState();

                    await _markersDatabase.SaveMarkerModel(Marker, MarkerAssignment.OpId);
                    _messenger.Publish(new OperationDataChangedMessage(this));
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
                    break;
                case "assigned":
                    AcknowledgedEnabled = true;
                    CompletedEnabled = true;
                    IncompleteEnabled = false;
                    break;
                case "acknowledged":
                    AcknowledgedEnabled = false;
                    CompletedEnabled = true;
                    IncompleteEnabled = false;
                    break;
                default:
                    break;
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


        #endregion
    }
}