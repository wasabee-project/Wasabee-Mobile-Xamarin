using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
{
    public class AssignmentsListViewModel : BaseViewModel
    {
        private readonly OperationsDatabase _operationsDatabase;
        private readonly IPreferences _preferences;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IMvxMessenger _messenger;
        private readonly IDialogNavigationService _dialogNavigationService;
        private readonly IMvxNavigationService _navigationService;

        private readonly MvxSubscriptionToken _token;

        public AssignmentsListViewModel(OperationsDatabase operationsDatabase, IPreferences preferences, IUserSettingsService userSettingsService,
            IMvxMessenger messenger, IDialogNavigationService dialogNavigationService, IMvxNavigationService navigationService)
        {
            _operationsDatabase = operationsDatabase;
            _preferences = preferences;
            _userSettingsService = userSettingsService;
            _messenger = messenger;
            _dialogNavigationService = dialogNavigationService;
            _navigationService = navigationService;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg => await RefreshCommand.ExecuteAsync());
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await RefreshCommand.ExecuteAsync();
        }

        #region Properties

        public OperationModel? Operation { get; set; }

        public MvxObservableCollection<AssignmentData> Assignments { get; set; } = new MvxObservableCollection<AssignmentData>();

        #endregion

        #region Commands

        public IMvxAsyncCommand RefreshCommand => new MvxAsyncCommand(RefreshExecuted);
        private async Task RefreshExecuted()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            LoggingService.Trace("Executing AssignmentsListViewModel.RefreshCommand");

            try
            {
                var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
                if (string.IsNullOrWhiteSpace(selectedOpId))
                    return;

                Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
                if (Operation == null)
                    return;

                var userGid = _userSettingsService.GetLoggedUserGoogleId();
                if (string.IsNullOrWhiteSpace(userGid))
                    return;

                Assignments.Clear();

                if (!Operation.Links.IsNullOrEmpty())
                {
                    var links = Operation.Links.Where(l => l.AssignedTo.Equals(userGid))
                        .Select(l => new LinkAssignmentData()
                        {
                            Link = l,
                            FromPortal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(l.FromPortalId)),
                            ToPortal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(l.ToPortalId)),
                            Color = WasabeeColorsHelper.GetColorFromWasabeeName(l.Color, Operation.Color)
                        }).OrderBy(x => x.Link!.ThrowOrderPos).ToList();

                    if (!links.IsNullOrEmpty())
                        Assignments.AddRange(links);
                }

                if (!Operation.Markers.IsNullOrEmpty())
                {
                    var markers = Operation.Markers.Where(m => m.AssignedTo.Equals(userGid))
                        .Select(m => new MarkerAssignmentData()
                        {
                            Marker = m,
                            Portal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(m.PortalId))
                        }).OrderBy(x => x.Marker!.Order).ToList();


                    if (!markers.IsNullOrEmpty())
                        Assignments.AddRange(markers);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing AssignmentsListViewModel.RefreshCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => Assignments);

                IsBusy = false;
            }
        }

        public IMvxAsyncCommand<AssignmentData> SelectAssignmentCommand => new MvxAsyncCommand<AssignmentData>(SelectAssignmentExecuted);
        private async Task SelectAssignmentExecuted(AssignmentData data)
        {
            if (data is LinkAssignmentData linkAssignmentData)
                await _dialogNavigationService.Navigate<LinkAssignmentDialogViewModel, LinkAssignmentData>(linkAssignmentData);
        }

        #endregion
    }

    public class AssignmentData
    {
        public LinkModel? Link { get; set; }
        public MarkerModel? Marker { get; set; }
    }

    public class LinkAssignmentData : AssignmentData
    {
        public PortalModel? FromPortal { get; set; }
        public PortalModel? ToPortal { get; set; }

        public string FromPortalName => FromPortal?.Name ?? FromPortal?.Id ?? string.Empty;
        public string ToPortalName => ToPortal?.Name ?? ToPortal?.Id ?? string.Empty;

        public Color Color { get; set; }
    }

    public class MarkerAssignmentData : AssignmentData
    {
        public PortalModel? Portal { get; set; }

        public string PortalName => Portal?.Name ?? Portal?.Id ?? string.Empty;

        public IMvxCommand ShowMarkerCommand => new MvxCommand(ShowMarkerExecuted);
        private void ShowMarkerExecuted()
        {
            if (Marker != null)
                Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new ShowMarkerOnMapMessage(this, Marker));
        }
    }
}