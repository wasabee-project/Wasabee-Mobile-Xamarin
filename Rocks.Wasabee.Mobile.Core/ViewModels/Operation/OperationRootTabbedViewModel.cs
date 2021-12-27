using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
{
    public class OperationRootTabbedViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IUserDialogs _userDialogs;
        private readonly IPreferences _preferences;
        private readonly IMvxMessenger _messenger;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        private MvxSubscriptionToken? _token;
        private List<BasePageInTabbedPageViewModel> Children;

        public OperationRootTabbedViewModel(IMvxNavigationService navigationService, IUserDialogs userDialogs,
            IPreferences preferences, IMvxMessenger messenger, OperationsDatabase operationsDatabase,
            WasabeeApiV1Service wasabeeApiV1Service)
        {
            _navigationService = navigationService;
            _userDialogs = userDialogs;
            _preferences = preferences;
            _messenger = messenger;
            _operationsDatabase = operationsDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;

            Children = new List<BasePageInTabbedPageViewModel>();
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            _token ??= _messenger.Subscribe<OperationDataChangedMessage>(mgs => _messenger.Publish(new MessageFrom<OperationRootTabbedViewModel>(this)));
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);

            foreach (var child in Children) 
                child.Destroy();

            _token?.Dispose();
            _token = null;
        }

        #region Commands

        public IMvxCommand RefreshOperationCommand => new MvxCommand(async () => await RefreshOperationExecuted());
        private async Task RefreshOperationExecuted()
        {
            if (IsBusy) return;
            LoggingService.Trace("Executing OperationRootTabbedViewModel.RefreshOperationCommand");

            IsBusy = true;
            _userDialogs.ShowLoading();
            
            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            var hasUpdated = false;
            try
            {
                var localData = await _operationsDatabase.GetOperationModel(selectedOpId);
                var updatedData = await _wasabeeApiV1Service.Operations_GetOperation(selectedOpId);

                if (localData != null && updatedData != null && !localData.Modified.Equals(updatedData.Modified))
                {
                    await _operationsDatabase.SaveOperationModel(updatedData);
                    hasUpdated = true;
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing OperationRootTabbedViewModel.RefreshOperationCommand");
            }
            finally
            {
                IsBusy = false;

                _userDialogs.HideLoading();
                _userDialogs.Toast(hasUpdated ? "Operation data updated" : "You already have latest OP version");
                
                _messenger.Publish(new MessageFrom<OperationRootTabbedViewModel>(this));
                _messenger.Publish(new RefreshAllAgentsLocationsMessage(this));
            }
        }

        public IMvxAsyncCommand ShowInitialViewModelsCommand => new MvxAsyncCommand(ShowInitialViewModels);
        private async Task ShowInitialViewModels()
        {
            Children.Clear();

            var mapViewModel = Mvx.IoCProvider.Resolve<MapViewModel>();
            var assignmentsListViewModel = Mvx.IoCProvider.Resolve<AssignmentsListViewModel>();
            var checklistViewModel = Mvx.IoCProvider.Resolve<ChecklistViewModel>();

            Children.Add(mapViewModel);
            Children.Add(assignmentsListViewModel);
            Children.Add(checklistViewModel);

            var tasks = new List<Task>
            {
                _navigationService.Navigate(mapViewModel),
                _navigationService.Navigate(assignmentsListViewModel),
                _navigationService.Navigate(checklistViewModel)
            };
            await Task.WhenAll(tasks);
        }

        #endregion
    }
}