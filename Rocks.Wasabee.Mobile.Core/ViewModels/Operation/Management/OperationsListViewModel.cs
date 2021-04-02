using Microsoft.AppCenter.Analytics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.Management {
    public class OperationsListViewModel : BaseViewModel
    {
        private readonly IUserSettingsService _userSettingsService;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IMvxMessenger _messenger;
        private readonly IMvxNavigationService _navigationService;

        public OperationsListViewModel(IUserSettingsService userSettingsService, OperationsDatabase operationsDatabase,
            TeamsDatabase teamsDatabase, WasabeeApiV1Service wasabeeApiV1Service,
            IMvxMessenger messenger, IMvxNavigationService navigationService)
        {
            _userSettingsService = userSettingsService;
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _messenger = messenger;
            _navigationService = navigationService;
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            await base.Initialize();

            RefreshCommand.Execute(false);
        }

        #region Properties

        public MvxObservableCollection<Operation> OperationsCollection { get; set; } = new MvxObservableCollection<Operation>();
        public bool IsRefreshing { get; set; }

        private bool _showHiddenOps = false;
        public bool ShowHiddenOps
        {
            get => _showHiddenOps;
            set
            {
                if (SetProperty(ref _showHiddenOps, value))
                    RefreshCommand.Execute(_showHiddenOps);
            }
        }

        #endregion

        #region Commands

        public IMvxCommand RefreshCommand => new MvxCommand(async () => await RefreshExecuted());
        public async Task RefreshExecuted()
        {
            if (IsRefreshing)
                return;

            LoggingService.Trace("Executing OperationsListViewModel.RefreshCommand");

            IsRefreshing = true;
            
            var localOperations = await _operationsDatabase.GetOperationModels();
            var hasUpdatedLocalOps = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var userModel = await _wasabeeApiV1Service.User_GetUserInformations();
                if (userModel?.Ops != null && userModel.Ops.Any())
                {
                    if (localOperations.Count != userModel.Ops.Count)
                    {
                        var localToRemove = new List<OperationModel>(localOperations.Where(local => userModel.Ops.All(remote => local.Id != remote.Id)));
                        foreach (var toRemove in localToRemove)
                        {
                            var result = await _operationsDatabase.DeleteLocalOperation(toRemove.Id);
                            if (result > 0)
                                localOperations.Remove(toRemove);
                        }
                    }

                    foreach (var op in userModel.Ops)
                    {
                        if (localOperations.Any(x => x.Id == op.Id))
                            continue;
                        
                        var remoteOp = await _wasabeeApiV1Service.Operations_GetOperation(op.Id);
                        if (remoteOp != null)
                        {
                            await _operationsDatabase.SaveOperationModel(remoteOp);
                            hasUpdatedLocalOps = true;
                        }
                    }
                }
                else
                {
                    await _operationsDatabase.DeleteAllExceptOwnedBy(_userSettingsService.GetLoggedUserGoogleId());
                    localOperations.Clear();
                }
            }

            if (hasUpdatedLocalOps)
                localOperations = await _operationsDatabase.GetOperationModels();

            if (localOperations.Any())
            {
                if (!ShowHiddenOps)
                    localOperations = localOperations.Where(x => !x.IsHiddenLocally).ToList();

                OperationsCollection = new MvxObservableCollection<Operation>(
                    localOperations.Select(x => new Operation(x.Id, x.Name)
                    {
                        IsHiddenLocally = x.IsHiddenLocally
                    })
                    .OrderBy(x => x.Name));
            }
            else
            {
                OperationsCollection.Clear();
                await RaisePropertyChanged(() => OperationsCollection);

                var teamsCount = await _teamsDatabase.CountTeams();
                if (teamsCount == 0)
                {
                    // Leaves app
                    await _navigationService.Navigate(Mvx.IoCProvider.Resolve<SplashScreenViewModel>(), new SplashScreenNavigationParameter(doDataRefreshOnly: true));
                }
            }

            IsRefreshing = false;
        }

        public IMvxAsyncCommand<Operation> HideOperationCommand => new MvxAsyncCommand<Operation>(HideOperationExecuted);
        private async Task HideOperationExecuted(Operation op)
        {
            if (IsBusy)
                return;

            LoggingService.Trace("Executing OperationsListViewModel.HideOperationCommand");

            IsBusy = true;

            try
            {
                var hideOp = !op.IsHiddenLocally;
                var result = await _operationsDatabase.HideLocalOperation(op.Id, hideOp);
                if (result)
                {
                    var rowToUpdate = OperationsCollection.First(x => x.Id.Equals(op.Id));
                    var rowIndex = OperationsCollection.IndexOf(rowToUpdate);
                    if (ShowHiddenOps)
                    {
                        OperationsCollection[rowIndex].IsHiddenLocally = hideOp;
                    }
                    else
                    {
                        if (hideOp)
                            OperationsCollection.Remove(op);
                    }
                }

                _messenger.Publish(new MessageFrom<OperationsListViewModel>(this));
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing OperationsListViewModel.HideOperationCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxCommand<Operation> ShowOperationDetailCommand => new MvxCommand<Operation>(ShowOperationDetailExecuted);
        private async void ShowOperationDetailExecuted(Operation op)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            LoggingService.Trace("Executing OperationsListViewModel.ShowOperationDetailCommand");
            
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<OperationDetailViewModel>(), new OperationDetailNavigationParameter(op.Id));

            IsBusy = false;
        }

        #endregion

        #region Private Methods



        #endregion
    }

    public class Operation : MvxViewModel
    {
        public string Id { get; }
        public string Name { get; }

        public Operation(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool IsHiddenLocally { get; set; } = false;
    }
}