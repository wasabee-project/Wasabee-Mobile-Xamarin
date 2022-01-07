using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Cache;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Globalization;
using System.Linq;
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
        private readonly AgentsDatabase _agentsDatabase;
        private readonly IUserSettingsService _userSettingsService;

        public MarkerAssignmentDialogViewModel(IDialogNavigationService dialogNavigationService, IMvxMessenger messenger,
            IUserDialogs userDialogs, IMap map, IClipboard clipboard, WasabeeApiV1Service wasabeeApiV1Service,
            MarkersDatabase markersDatabase, AgentsDatabase agentsDatabase, IUserSettingsService userSettingsService) : base(dialogNavigationService)
        {
            _messenger = messenger;
            _userDialogs = userDialogs;
            _map = map;
            _clipboard = clipboard;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _markersDatabase = markersDatabase;
            _agentsDatabase = agentsDatabase;
            _userSettingsService = userSettingsService;
        }

        public void Prepare(MarkerAssignmentData parameter)
        {
            MarkerAssignment = parameter;
            Marker = MarkerAssignment.Marker;

            UpdateAssignments();
            UpdateButtonsState();
        }

        public override Task Initialize()
        {
            UpdateButtonsState();

            if (Marker is not null)
                Goal = GetGoalFromMarkerType(Marker.Type);

            return base.Initialize();
        }

        #region Properties

        public bool IsSelfAssignment { get; set; }

        public bool AcknowledgedEnabled { get; set; }
        public bool CompletedEnabled { get; set; }
        public bool IncompleteEnabled { get; set; }
        public bool ClaimEnabled { get; set; }
        public bool RejectEnabled { get; set; }

        public string Goal { get; set; } = string.Empty;

        public MarkerAssignmentData? MarkerAssignment { get; set; }
        public MarkerModel? Marker { get; set; }

        public string Assignments { get; set; } = string.Empty;

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
                        _userDialogs.Toast(Strings.Toast_RejectedAssignment);

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
                        _userDialogs.Toast(Strings.Toast_CoordinatesCopied);
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

        private async void UpdateAssignments()
        {
            if (Marker!.Assignments.IsNullOrEmpty())
                return;
            
            IsSelfAssignment = Marker.Assignments.Contains(_userSettingsService.GetLoggedUserGoogleId());

            var assignedAgents = await _agentsDatabase.GetAgents(Marker!.Assignments);
            Assignments = string.Join(", ", assignedAgents.Select(x => x.Name).OrderBy(x => x));
        }

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

                    UpdateAssignments();
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

            switch (Marker.State)
            {
                case TaskState.Pending:
                case TaskState.Completed:
                    AcknowledgedEnabled = false;
                    CompletedEnabled = false;
                    IncompleteEnabled = true;
                    RejectEnabled = false;
                    ClaimEnabled = false;
                    break;
                case TaskState.Assigned:
                    AcknowledgedEnabled = true;
                    CompletedEnabled = true;
                    IncompleteEnabled = false;
                    RejectEnabled = true;
                    ClaimEnabled = false;
                    break;
                case TaskState.Acknowledged:
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

        private string GetGoalFromMarkerType(MarkerType markerType)
        {
            return markerType.ToFriendlyString();
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