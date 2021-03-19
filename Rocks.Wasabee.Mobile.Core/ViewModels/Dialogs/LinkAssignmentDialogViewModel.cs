using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class LinkAssignmentDialogViewModel : BaseDialogViewModel, IMvxViewModel<LinkAssignmentData>
    {

        private readonly IUserDialogs _userDialogs;
        private readonly IClipboard _clipboard;
        private readonly IMap _map;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        public LinkAssignmentDialogViewModel(IDialogNavigationService dialogNavigationService, IUserDialogs userDialogs,
            IClipboard clipboard, IMap map, WasabeeApiV1Service wasabeeApiV1Service) : base(dialogNavigationService)
        {
            _userDialogs = userDialogs;
            _clipboard = clipboard;
            _map = map;
            _wasabeeApiV1Service = wasabeeApiV1Service;
        }

        public void Prepare(LinkAssignmentData parameter)
        {
            LinkAssignment = parameter;
        }

        #region Properties

        public string ButtonText { get; set; } = string.Empty;
        public LinkAssignmentData? LinkAssignment { get; set; }

        #endregion

        #region Commands

        public IMvxCommand<string> ShowOnMapCommand => new MvxCommand<string>(ShowOnMapExecuted);
        private void ShowOnMapExecuted(string fromOrToPortal)
        {
            if (IsBusy) return;

            IsBusy = true;

            switch (fromOrToPortal)
            {
                case "From":
                    if (LinkAssignment?.FromPortal != null)
                        Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ShowPortalOnMapMessage(this, LinkAssignment.FromPortal));

                    CloseCommand.Execute();
                    break;
                case "To":
                    if (LinkAssignment?.ToPortal != null)
                        Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ShowPortalOnMapMessage(this, LinkAssignment.ToPortal));

                    CloseCommand.Execute();
                    break;
            }

            IsBusy = false;
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
            if (IsBusy)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.CompleteCommand");

            IsBusy = true;

            try
            {
                var result = await _wasabeeApiV1Service.Operation_Link_Complete(LinkAssignment.OpId, LinkAssignment.Link.Id);
                if (result)
                    LinkAssignment.Link.Completed = !LinkAssignment.Link.Completed;
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
            if (IsBusy)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.IncompleteCommand");

            IsBusy = true;

            try
            {
                var result = await _wasabeeApiV1Service.Operation_Link_Incomplete(LinkAssignment.OpId, LinkAssignment.Link.Id);
                if (result)
                    LinkAssignment.Link.Completed = !LinkAssignment.Link.Completed;
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

        #endregion
    }
}