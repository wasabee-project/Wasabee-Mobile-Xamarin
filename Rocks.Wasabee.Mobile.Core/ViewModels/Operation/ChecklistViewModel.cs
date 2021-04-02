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
        
        private MvxSubscriptionToken? _tokenRefresh;

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
            
            _tokenRefresh ??= _messenger.Subscribe<MessageFrom<OperationRootTabbedViewModel>>(async msg => await RefreshCommand.ExecuteAsync());
            
            await RefreshCommand.ExecuteAsync();
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            _tokenRefresh?.Dispose();
            _tokenRefresh = null;
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
                {
                    assignedLinks = Operation.Links
                        .Select(l => new LinkAssignmentData(Operation.Id, l.ThrowOrderPos)
                        {
                            Link = l,
                            AssignedAgent = l.AssignedTo.IsNullOrEmpty() ? null : _teamAgentsDatabase.GetTeamAgent(l.AssignedTo).Result,
                            ShowAssignee = !l.AssignedTo.IsNullOrEmpty(),
                            FromPortal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(l.FromPortalId)),
                            ToPortal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(l.ToPortalId)),
                            Color = WasabeeColorsHelper.GetColorFromWasabeeName(l.Color, Operation.Color)
                        }).ToList();
                }

                if (!Operation.Markers.IsNullOrEmpty())
                {
                    assignedMarkers = Operation.Markers
                        .Select(m => new MarkerAssignmentData(Operation.Id, m.Order)
                        {
                            Marker = m,
                            AssignedAgent = m.AssignedTo.IsNullOrEmpty() ? null : _teamAgentsDatabase.GetTeamAgent(m.AssignedTo).Result,
                            ShowAssignee = !m.AssignedTo.IsNullOrEmpty(),
                            Portal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(m.PortalId))
                        }).ToList();
                }
                
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

        public IMvxAsyncCommand<AssignmentData> SelectElementCommand => new MvxAsyncCommand<AssignmentData>(SelectAssignmentExecuted);
        private async Task SelectAssignmentExecuted(AssignmentData data)
        {
            if (data is LinkAssignmentData linkAssignmentData)
                await _dialogNavigationService.Navigate<LinkAssignmentDialogViewModel, LinkAssignmentData>(linkAssignmentData);
            else if (data is MarkerAssignmentData markerAssignmentData)
                await _dialogNavigationService.Navigate<MarkerAssignmentDialogViewModel, MarkerAssignmentData>(markerAssignmentData);
        }

        #endregion
    }
}