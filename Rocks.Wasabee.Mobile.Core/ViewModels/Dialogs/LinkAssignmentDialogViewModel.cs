using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
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
    public class LinkAssignmentDialogViewModel : BaseDialogViewModel, IMvxViewModel<LinkAssignmentData>
    {

        private readonly IUserDialogs _userDialogs;
        private readonly IClipboard _clipboard;
        private readonly IMap _map;
        private readonly IMvxMessenger _messenger;
        private readonly LinksDatabase _linksDatabase;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IUserSettingsService _userSettingsService;

        public LinkAssignmentDialogViewModel(IDialogNavigationService dialogNavigationService, IUserDialogs userDialogs, IClipboard clipboard,
            IMap map, IMvxMessenger messenger, LinksDatabase linksDatabase, WasabeeApiV1Service wasabeeApiV1Service, IUserSettingsService userSettingsService) : base(dialogNavigationService)
        {
            _userDialogs = userDialogs;
            _clipboard = clipboard;
            _map = map;
            _messenger = messenger;
            _linksDatabase = linksDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _userSettingsService = userSettingsService;
        }

        public void Prepare(LinkAssignmentData parameter)
        {
            LinkAssignment = parameter;
            Link = LinkAssignment.Link;
            
            IsSelfAssignment = _userSettingsService.GetLoggedUserGoogleId().Equals(Link?.AssignedTo);
            UpdateButtonsState();
        }

        #region Properties
        
        public bool IsSelfAssignment { get; set; }
        
        public bool CompletedEnabled { get; set; }
        public bool IncompleteEnabled { get; set; }
        public bool ClaimEnabled{ get; set; }
        public bool RejectEnabled { get; set; }

        public LinkAssignmentData? LinkAssignment { get; set; }
        public LinkModel? Link { get; set; }

        #endregion

        #region Commands

        public IMvxCommand<string> ShowOnMapCommand => new MvxCommand<string>(ShowOnMapExecuted);
        private void ShowOnMapExecuted(string fromOrToPortal)
        {
            if (IsBusy) return;
            
            switch (fromOrToPortal)
            {
                case "From":
                    if (LinkAssignment?.FromPortal != null)
                        _messenger.Publish(new ShowPortalOnMapMessage(this, LinkAssignment.FromPortal));

                    CloseCommand.Execute();
                    break;
                case "To":
                    if (LinkAssignment?.ToPortal != null)
                        _messenger.Publish(new ShowPortalOnMapMessage(this, LinkAssignment.ToPortal));

                    CloseCommand.Execute();
                    break;
            }
        }

        public IMvxCommand<string> OpenInNavigationAppCommand => new MvxCommand<string>(OpenInNavigationAppExecuted);
        private async void OpenInNavigationAppExecuted(string fromOrToPortal)
        {
            if (IsBusy) return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.OpenInNavigationAppCommand");

            IsBusy = true;

            if (LinkAssignment == null) return;

            try
            {
                var culture = CultureInfo.GetCultureInfo("en-US");
                string coordinates;
                Location location;

                switch (fromOrToPortal)
                {
                    case "From":
                        if (LinkAssignment.FromPortal == null)
                            return;
                        
                        coordinates = $"{LinkAssignment.FromPortal.Lat},{LinkAssignment.FromPortal.Lng}";

                        double.TryParse(LinkAssignment.FromPortal.Lat, NumberStyles.Float, culture, out var fromLat);
                        double.TryParse(LinkAssignment.FromPortal.Lng, NumberStyles.Float, culture, out var fromLng);
                        
                        location = new Location(fromLat, fromLng);
                        break;
                    case "To":
                        if (LinkAssignment.ToPortal == null)
                            return;
                        
                        coordinates = $"{LinkAssignment.ToPortal.Lat},{LinkAssignment.ToPortal.Lng}";

                        double.TryParse(LinkAssignment.ToPortal.Lat, NumberStyles.Float, culture, out var toLat);
                        double.TryParse(LinkAssignment.ToPortal.Lng, NumberStyles.Float, culture, out var toLng);
                        
                        location = new Location(toLat, toLng);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(fromOrToPortal), fromOrToPortal,
                            "Incorrect value");
                }

                if (coordinates.IsNullOrEmpty() is false)
                {
                    await _clipboard.SetTextAsync(coordinates);
                    if (_clipboard.HasText)
                        _userDialogs.Toast("Coordinates copied to clipboartd.");
                }
                
                await _map.OpenAsync(location);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing LinkAssignmentDialogViewModel.OpenInNavigationAppCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxAsyncCommand CompleteCommand => new MvxAsyncCommand(CompleteExecuted);
        private async Task CompleteExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.CompleteCommand");

            IsBusy = true;

            try
            {
                if (await _wasabeeApiV1Service.Operation_Link_Complete(LinkAssignment.OpId, LinkAssignment.Link.Id))
                    await UpdateLinkAndNotify();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing LinkAssignmentDialogViewModel.CompleteCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxAsyncCommand IncompleteCommand => new MvxAsyncCommand(IncompleteExecuted);
        private async Task IncompleteExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.IncompleteCommand");

            IsBusy = true;

            try
            {
                if (await _wasabeeApiV1Service.Operation_Link_Incomplete(LinkAssignment.OpId, LinkAssignment.Link.Id))
                    await UpdateLinkAndNotify();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing LinkAssignmentDialogViewModel.IncompleteCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxAsyncCommand ClaimCommand => new MvxAsyncCommand(ClaimExecuted);
        private async Task ClaimExecuted()
        {
            if (IsBusy || IsSelfAssignment)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.ClaimCommand");

            IsBusy = true;

            try
            {
                if (await _wasabeeApiV1Service.Operation_Link_Claim(LinkAssignment.OpId, LinkAssignment.Link.Id))
                    await UpdateLinkAndNotify();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing LinkAssignmentDialogViewModel.ClaimCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxAsyncCommand RejectCommand => new MvxAsyncCommand(RejectExecuted);
        private async Task RejectExecuted()
        {
            if (IsBusy || !IsSelfAssignment)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.RejectCommand");

            IsBusy = true;

            try
            {
                if (await _wasabeeApiV1Service.Operation_Link_Reject(LinkAssignment.OpId, LinkAssignment.Link.Id))
                {
                    _userDialogs.Toast("Assignment rejected");

                    await UpdateLinkAndNotify();
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing LinkAssignmentDialogViewModel.RejectCommand");
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
        private async Task UpdateLinkAndNotify()
        {
            if (LinkAssignment != null && Link != null)
            {
                var updated = await _wasabeeApiV1Service.Operations_GetLink(LinkAssignment.OpId, Link.Id);
                if (updated != null)
                {
                    Link = updated;
                    IsSelfAssignment = _userSettingsService.GetLoggedUserGoogleId().Equals(Link.AssignedTo);

                    UpdateButtonsState();

                    await _linksDatabase.SaveLinkModel(Link, LinkAssignment.OpId);

                    _messenger.Publish(new LinkDataChangedMessage(this, Link, LinkAssignment.OpId));
                }
                else
                {
                    IsBusy = false;
                    CloseCommand.Execute();
                }
            }
        }

        private void UpdateButtonsState()
        {
            if (Link == null)
                return;

            if (IsSelfAssignment)
            {
                CompletedEnabled = !Link.Completed;
                IncompleteEnabled = Link.Completed;
                RejectEnabled = !Link.Completed;
                ClaimEnabled = false;
            }
            else
            {
                CompletedEnabled = false;
                IncompleteEnabled = false;
                RejectEnabled = false;
                ClaimEnabled = true;
            }
        }
        #endregion
    }
}