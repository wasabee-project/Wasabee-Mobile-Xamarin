using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Agent;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
{
    public class AssignmentsListViewModel : BasePageInTabbedPageViewModel
    {
        private readonly OperationsDatabase _operationsDatabase;
        private readonly AgentsDatabase _agentsDatabase;
        private readonly IPreferences _preferences;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IMvxMessenger _messenger;
        private readonly IDialogNavigationService _dialogNavigationService;
        private readonly IMvxNavigationService _navigationService;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IUserDialogs _userDialogs;

        private MvxSubscriptionToken? _token;
        private MvxSubscriptionToken? _tokenFromMap;
        private MvxSubscriptionToken? _tokenRefresh;
        private MvxSubscriptionToken? _tokenRefreshLink;
        private MvxSubscriptionToken? _tokenRefreshMarker;

        private int _pendingRefreshCount = 0;

        public AssignmentsListViewModel(OperationsDatabase operationsDatabase, AgentsDatabase agentsDatabase, IPreferences preferences,
            IUserSettingsService userSettingsService, IMvxMessenger messenger, IDialogNavigationService dialogNavigationService,
            IMvxNavigationService navigationService, WasabeeApiV1Service wasabeeApiV1Service, IUserDialogs userDialogs)
        {
            _operationsDatabase = operationsDatabase;
            _agentsDatabase = agentsDatabase;
            _preferences = preferences;
            _userSettingsService = userSettingsService;
            _messenger = messenger;
            _dialogNavigationService = dialogNavigationService;
            _navigationService = navigationService;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _userDialogs = userDialogs;
        }
        
        public override async void ViewAppearing()
        {
            base.ViewAppearing();
            
            _token ??= _messenger.Subscribe<SelectedOpChangedMessage>(async msg => await RefreshCommand.ExecuteAsync());
            _tokenFromMap ??= _messenger.Subscribe<MessageFor<AssignmentsListViewModel>>(async msg => await RefreshCommand.ExecuteAsync());
            _tokenRefresh ??= _messenger.Subscribe<MessageFrom<OperationRootTabbedViewModel>>(async msg => await RefreshCommand.ExecuteAsync());
            _tokenRefreshLink ??= _messenger.Subscribe<LinkDataChangedMessage>(msg => RefreshLinkCommand.Execute(msg.LinkData));
            _tokenRefreshMarker ??= _messenger.Subscribe<MarkerDataChangedMessage>(msg => RefreshMarkerCommand.Execute(msg.MarkerData));

            await RefreshCommand.ExecuteAsync();
        }

        public override void Destroy()
        {
            _token?.Dispose();
            _token = null;
            _tokenFromMap?.Dispose();
            _tokenFromMap = null;
            _tokenRefresh?.Dispose();
            _tokenRefresh = null;
            _tokenRefreshLink?.Dispose();
            _tokenRefreshLink = null;
            _tokenRefreshMarker?.Dispose();
            _tokenRefreshMarker = null;
        }

        #region Properties

        public bool IsLoading { get; set; }

        public OperationModel? Operation { get; set; }
        
        public MvxObservableCollection<AssignmentData> Elements { get; set; } = new MvxObservableCollection<AssignmentData>();

        #endregion

        #region Commands

        public IMvxAsyncCommand RefreshCommand => new MvxAsyncCommand(RefreshExecuted);
        private async Task RefreshExecuted()
        {
            if (IsLoading)
            {
                _pendingRefreshCount++;
                return;
            }

            IsLoading = true;
            _pendingRefreshCount = 0;

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

                var assignedLinks = new List<LinkAssignmentData>();
                var assignedMarkers = new List<MarkerAssignmentData>();
                if (!Operation.Links.IsNullOrEmpty())
                    assignedLinks = Operation.Links.Where(l => l.AssignedTo.Equals(userGid))
                        .Select(CreateLinkAssignmentData).OrderBy(x => x.Link!.ThrowOrderPos).ToList();
                
                if (!Operation.Markers.IsNullOrEmpty())
                    assignedMarkers = Operation.Markers.Where(l => l.AssignedTo.Equals(userGid))
                        .Select(CreateMarkerAssignmentData).OrderBy(x => x.Marker!.Order).ToList();
                
                var orderedAssignments = new List<AssignmentData>();
                if (!assignedLinks.IsNullOrEmpty())
                    orderedAssignments.AddRange(assignedLinks);
                if (!assignedMarkers.IsNullOrEmpty())
                    orderedAssignments.AddRange(assignedMarkers);
                
                Elements.Clear();

                if (!orderedAssignments.IsNullOrEmpty())
                {
                    orderedAssignments = orderedAssignments.OrderBy(x => x.Order).ToList();
                    Elements.AddRange(orderedAssignments);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing AssignmentsListViewModel.RefreshCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => Elements);

                IsLoading = false;

                if (_pendingRefreshCount > 0)
                    await RefreshCommand.ExecuteAsync().ConfigureAwait(false);
            }
        }
        
        public IMvxCommand<LinkModel> RefreshLinkCommand => new MvxCommand<LinkModel>(RefreshLinkExecuted);
        private void RefreshLinkExecuted(LinkModel linkModel)
        {
            if (IsLoading || Elements.IsNullOrEmpty() || Operation == null)
                return;

            LoggingService.Trace("Executing AssignmentsListViewModel.RefreshLinkExecuted");

            try
            {
                var toRemove = Elements.Where(x => x.Link != null).FirstOrDefault(assignment => assignment.Link!.Id.Equals(linkModel.Id));
                if (toRemove != null)
                {
                    var index = Elements.IndexOf(toRemove);
                    if (string.IsNullOrWhiteSpace(linkModel.AssignedTo))
                    {
                        Elements.RemoveAt(index);
                        return;
                    }

                    var newAssignment = CreateLinkAssignmentData(linkModel);

                    Elements.ReplaceRange(new [] { newAssignment }, index, 1);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing AssignmentsListViewModel.RefreshLinkExecuted");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public IMvxCommand<MarkerModel> RefreshMarkerCommand => new MvxCommand<MarkerModel>(RefreshMarkerExecuted);
        private void RefreshMarkerExecuted(MarkerModel markerModel)
        {
            if (IsLoading || Elements.IsNullOrEmpty() || Operation == null)
                return;

            LoggingService.Trace("Executing AssignmentsListViewModel.RefreshMarkerExecuted");

            try
            {
                var toRemove = Elements.Where(x => x.Marker != null).FirstOrDefault(assignment => assignment.Marker!.Id.Equals(markerModel.Id));
                if (toRemove != null)
                {
                    var index = Elements.IndexOf(toRemove);
                    if (string.IsNullOrWhiteSpace(markerModel.AssignedTo))
                    {
                        Elements.RemoveAt(index);
                        return;
                    }
                    
                    var newAssignment = CreateMarkerAssignmentData(markerModel);
                    Elements.ReplaceRange(new [] { newAssignment }, index, 1);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing AssignmentsListViewModel.RefreshMarkerExecuted");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public IMvxAsyncCommand<AssignmentData> SelectAssignmentCommand => new MvxAsyncCommand<AssignmentData>(SelectAssignmentExecuted);
        private async Task SelectAssignmentExecuted(AssignmentData data)
        {
            if (data is LinkAssignmentData linkAssignmentData)
                await _dialogNavigationService.Navigate<LinkAssignmentDialogViewModel, LinkAssignmentData>(linkAssignmentData);
            else if (data is MarkerAssignmentData markerAssignmentData)
                await _dialogNavigationService.Navigate<MarkerAssignmentDialogViewModel, MarkerAssignmentData>(markerAssignmentData);
        }

        #endregion

        #region Private methods
        
        private LinkAssignmentData CreateLinkAssignmentData(LinkModel link)
        {
            if (Operation == null)
                throw new Exception("Operation is null");

            return new LinkAssignmentData(Operation!.Id, link.ThrowOrderPos)
            {
                Link = link,
                AssignedAgent = string.IsNullOrEmpty(link.AssignedTo) ? null : _agentsDatabase.GetAgent(link.AssignedTo).Result,
                FromPortal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(link.FromPortalId)),
                ToPortal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(link.ToPortalId)),
                Color = WasabeeColorsHelper.GetColorFromWasabeeName(link.Color, Operation.Color)
            };
        }

        private MarkerAssignmentData CreateMarkerAssignmentData(MarkerModel marker)
        {
            if (Operation == null)
                throw new Exception("Operation is null");

            return new MarkerAssignmentData(Operation.Id, marker.Order)
            {
                Marker = marker,
                AssignedAgent = string.IsNullOrEmpty(marker.AssignedTo) ? null : _agentsDatabase.GetAgent(marker.AssignedTo).Result,
                Portal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(marker.PortalId))
            };
        }
        
        #endregion
    }

    public abstract class AssignmentData
    {
        public string OpId { get; }
        public int Order { get; }

        protected AssignmentData(string opId, int order)
        {
            OpId = opId;
            Order = order;
        }

        public LinkModel? Link { get; set; }
        public MarkerModel? Marker { get; set; }

        public AgentModel? AssignedAgent { get; set; }
        public bool ShowAssignee { get; set; } = false;
    }

    public class LinkAssignmentData : AssignmentData
    {
        public LinkAssignmentData(string opId, int order) : base(opId, order)
        {

        }

        public PortalModel? FromPortal { get; set; }
        public PortalModel? ToPortal { get; set; }

        public string FromPortalName => FromPortal?.Name ?? FromPortal?.Id ?? string.Empty;
        public string ToPortalName => ToPortal?.Name ?? ToPortal?.Id ?? string.Empty;

        public Color Color { get; set; }
    }

    public class MarkerAssignmentData : AssignmentData
    {
        public MarkerAssignmentData(string opId, int order) : base(opId, order)
        {

        }

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