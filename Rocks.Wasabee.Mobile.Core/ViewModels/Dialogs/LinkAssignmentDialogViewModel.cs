using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class LinkAssignmentDialogViewModel : BaseDialogViewModel, IMvxViewModel<LinkAssignmentData>
    {
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        public LinkAssignmentDialogViewModel(IDialogNavigationService dialogNavigationService, WasabeeApiV1Service wasabeeApiV1Service) : base(dialogNavigationService)
        {
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
                string uri;
                switch (fromOrToPortal)
                {
                    case "From":
                        if (LinkAssignment.FromPortal == null)
                            return;

                        uri = Device.RuntimePlatform switch
                        {
                            Device.Android => "https://www.google.com/maps/search/?api=1&query=" +
                                              $"{LinkAssignment.FromPortal.Lat}," +
                                              $"{LinkAssignment.FromPortal.Lng}",

                            Device.iOS => "https://maps.apple.com/?ll=" +
                                          $"{LinkAssignment.FromPortal.Lat}," +
                                          $"{LinkAssignment.FromPortal.Lng}",
                            _ => throw new ArgumentOutOfRangeException(Device.RuntimePlatform)
                        };
                        break;
                    case "To":
                        if (LinkAssignment.ToPortal == null)
                            return;

                        uri = Device.RuntimePlatform switch
                        {
                            Device.Android => "https://www.google.com/maps/search/?api=1&query=" +
                                              $"{LinkAssignment.ToPortal.Lat}," +
                                              $"{LinkAssignment.ToPortal.Lng}",

                            Device.iOS => "https://maps.apple.com/?ll=" +
                                          $"{LinkAssignment.ToPortal.Lat}," +
                                          $"{LinkAssignment.ToPortal.Lng}",
                            _ => throw new ArgumentOutOfRangeException(Device.RuntimePlatform)
                        };
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(fromOrToPortal), fromOrToPortal,
                            "Incorrect value");
                }

                if (string.IsNullOrWhiteSpace(uri))
                    return;

                if (await Launcher.CanOpenAsync(uri))
                    await Launcher.OpenAsync(uri);
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

        public IMvxAsyncCommand ChangeStateCommand => new MvxAsyncCommand(ChangeStateExecuted);
        private async Task ChangeStateExecuted()
        {
            if (IsBusy)
                return;

            if (LinkAssignment?.Link == null)
                return;

            LoggingService.Trace("Executing LinkAssignmentDialogViewModel.ChangeStateCommand");

            IsBusy = true;

            try
            {
                if (LinkAssignment.Link.Completed)
                {
                    var result = await _wasabeeApiV1Service.Operation_Link_Incomplete(LinkAssignment.OpId, LinkAssignment.Link.Id);
                    if (result)
                        LinkAssignment.Link.Completed = !LinkAssignment.Link.Completed;
                }
                else
                {
                    var result = await _wasabeeApiV1Service.Operation_Link_Complete(LinkAssignment.OpId, LinkAssignment.Link.Id);
                    if (result)
                        LinkAssignment.Link.Completed = !LinkAssignment.Link.Completed;
                }

                UpdateButtonText();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing LinkAssignmentDialogViewModel.ChangeStateCommand");
            }
            finally
            {
                IsBusy = false;
            }

        }

        #endregion

        #region Private methods

        private void UpdateButtonText()
        {
            if (LinkAssignment?.Link == null)
                return;

            ButtonText = LinkAssignment.Link.Completed ? "Uncompleted" : "Completed";
        }

        #endregion
    }
}