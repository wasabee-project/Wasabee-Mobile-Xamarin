using Acr.UserDialogs;
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

        private MvxSubscriptionToken _token;

        public OperationRootTabbedViewModel(IMvxNavigationService navigationService, IUserDialogs userDialogs,
            IPreferences preferences, IMvxMessenger messenger,
            OperationsDatabase operationsDatabase, WasabeeApiV1Service wasabeeApiV1Service)
        {
            _navigationService = navigationService;
            _userDialogs = userDialogs;
            _preferences = preferences;
            _messenger = messenger;
            _operationsDatabase = operationsDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;

            _token = messenger.Subscribe<OperationDataChangedMessage>(mgs => messenger.Publish(new MessageFrom<OperationRootTabbedViewModel>(this)));
        }

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

                if (hasUpdated)
                {
                    _messenger.Publish(new MessageFrom<OperationRootTabbedViewModel>(this));
                }

                _messenger.Publish(new RefreshAllAgentsLocationsMessage(this));
            }
        }


        public IMvxAsyncCommand ShowInitialViewModelsCommand => new MvxAsyncCommand(ShowInitialViewModels);
        private async Task ShowInitialViewModels()
        {
            var tasks = new List<Task>
            {
                _navigationService.Navigate<MapViewModel>(),
                _navigationService.Navigate<AssignmentsListViewModel>()
            };
            await Task.WhenAll(tasks);
        }
    }
}