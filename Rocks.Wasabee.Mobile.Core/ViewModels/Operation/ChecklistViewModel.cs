using Microsoft.AppCenter.Analytics;
using MvvmCross.Commands;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
{
    public class ChecklistViewModel : BaseViewModel
    {
        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamAgentsDatabase _teamAgentsDatabase;
        private readonly IMvxMessenger _messenger;
        private readonly IDialogNavigationService _dialogNavigationService;
        private readonly IPreferences _preferences;
        
        private MvxSubscriptionToken? _token;
        private MvxSubscriptionToken? _tokenRefresh;
        private MvxSubscriptionToken? _tokenRefreshLink;
        private MvxSubscriptionToken? _tokenRefreshMarker;

        private int _pendingRefreshCount = 0;

        public ChecklistViewModel(OperationsDatabase operationsDatabase, TeamAgentsDatabase teamAgentsDatabase,
            IMvxMessenger messenger, IDialogNavigationService dialogNavigationService, IPreferences preferences)
        {
            _operationsDatabase = operationsDatabase;
            _teamAgentsDatabase = teamAgentsDatabase;
            _messenger = messenger;
            _dialogNavigationService = dialogNavigationService;
            _preferences = preferences;
        }

        public override Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to ChecklistViewModel");

            return base.Initialize();
        }
        
        public override async void ViewAppearing()
        {
            base.ViewAppearing();
            
            _token ??= _messenger.Subscribe<SelectedOpChangedMessage>(async msg => await RefreshCommand.ExecuteAsync());
            _tokenRefresh ??= _messenger.Subscribe<MessageFrom<OperationRootTabbedViewModel>>(async msg => await RefreshCommand.ExecuteAsync());
            _tokenRefreshLink ??= _messenger.Subscribe<LinkDataChangedMessage>(msg => RefreshLinkCommand.Execute(msg.LinkData));
            _tokenRefreshMarker ??= _messenger.Subscribe<MarkerDataChangedMessage>(msg => RefreshMarkerCommand.Execute(msg.MarkerData));
            
            await RefreshCommand.ExecuteAsync();
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            _token?.Dispose();
            _token = null;
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

        #region Command

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

            LoggingService.Trace("Executing ChecklistViewModel.RefreshCommand");

            try
            {
                var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
                if (string.IsNullOrWhiteSpace(selectedOpId))
                    return;

                Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
                if (Operation == null)
                    return;
                
                var assignedLinks = new List<LinkAssignmentData>();
                var assignedMarkers = new List<MarkerAssignmentData>();
                if (!Operation.Links.IsNullOrEmpty())
                    assignedLinks = Operation.Links.Select(CreateLinkAssignmentData).ToList();

                if (!Operation.Markers.IsNullOrEmpty())
                    assignedMarkers = Operation.Markers.Select(CreateMarkerAssignmentData).ToList();

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
                LoggingService.Error(e, "Error Executing ChecklistViewModel.RefreshCommand");
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

            LoggingService.Trace("Executing ChecklistViewModel.RefreshLinkExecuted");

            try
            {
                var toRemove = Elements.Where(x => x.Link != null).FirstOrDefault(assignment => assignment.Link!.Id.Equals(linkModel.Id));
                if (toRemove != null)
                {
                    var index = Elements.IndexOf(toRemove);
                    var newAssignment = CreateLinkAssignmentData(linkModel);

                    Elements.ReplaceRange(new [] { newAssignment }, index, 1);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing ChecklistViewModel.RefreshLinkExecuted");
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

            LoggingService.Trace("Executing ChecklistViewModel.RefreshMarkerExecuted");

            try
            {
                var toRemove = Elements.Where(x => x.Marker != null).FirstOrDefault(assignment => assignment.Marker!.Id.Equals(markerModel.Id));
                if (toRemove != null)
                {
                    var index = Elements.IndexOf(toRemove);
                    var newAssignment = CreateMarkerAssignmentData(markerModel);

                    Elements.ReplaceRange(new [] { newAssignment }, index, 1);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing ChecklistViewModel.RefreshMarkerExecuted");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public IMvxAsyncCommand<AssignmentData> SelectElementCommand => new MvxAsyncCommand<AssignmentData>(SelectAssignmentExecuted);
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
                AssignedAgent = string.IsNullOrEmpty(link.AssignedTo) ? null : _teamAgentsDatabase.GetTeamAgent(link.AssignedTo).Result,
                ShowAssignee = !string.IsNullOrEmpty(link.AssignedTo),
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
                AssignedAgent = string.IsNullOrEmpty(marker.AssignedTo) ? null : _teamAgentsDatabase.GetTeamAgent(marker.AssignedTo).Result,
                ShowAssignee = !string.IsNullOrEmpty(marker.AssignedTo),
                Portal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(marker.PortalId))
            };
        }

        #endregion
    }
}