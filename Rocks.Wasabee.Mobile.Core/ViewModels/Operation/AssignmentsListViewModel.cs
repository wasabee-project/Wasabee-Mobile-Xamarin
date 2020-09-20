using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
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

        private readonly MvxSubscriptionToken _token;

        public AssignmentsListViewModel(OperationsDatabase operationsDatabase, IPreferences preferences, IUserSettingsService userSettingsService,
            IMvxMessenger messenger)
        {
            _operationsDatabase = operationsDatabase;
            _preferences = preferences;
            _userSettingsService = userSettingsService;
            _messenger = messenger;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg => await RefreshCommand.ExecuteAsync());
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await RefreshCommand.ExecuteAsync();
        }

        #region Properties

        public OperationModel Operation { get; set; }

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
                            FromPortalName = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(l.FromPortalId))?.Name ?? l.FromPortalId,
                            ToPortalName = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(l.ToPortalId))?.Name ?? l.ToPortalId
                        }).ToList();

                    if (!links.IsNullOrEmpty())
                        Assignments.AddRange(links);
                }

                if (!Operation.Markers.IsNullOrEmpty())
                {
                    var markers = Operation.Markers.Where(m => m.AssignedTo.Equals(userGid))
                        .Select(m => new MarkerAssignmentData()
                        {
                            Marker = m,
                            PortalName = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(m.PortalId))?.Name ?? m.PortalId
                        }).ToList();


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

        #endregion
    }

    public class AssignmentData
    {
        public LinkModel Link { get; set; }
        public MarkerModel Marker { get; set; }
    }

    public class LinkAssignmentData : AssignmentData
    {
        public string FromPortalName { get; set; }
        public string ToPortalName { get; set; }
    }

    public class MarkerAssignmentData : AssignmentData
    {
        public string PortalName { get; set; }
    }
}