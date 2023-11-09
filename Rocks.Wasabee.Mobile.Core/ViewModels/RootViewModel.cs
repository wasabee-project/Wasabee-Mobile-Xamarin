using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public class RootViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IMvxMessenger _messenger;
        private readonly IDialogNavigationService _dialogNavigationService;

        private bool _alreadyLoaded;
        private MvxSubscriptionToken? _targetReceivedToken;

        public RootViewModel(IMvxNavigationService navigationService,
            IMvxMessenger messenger, IDialogNavigationService dialogNavigationService)
        {
            _navigationService = navigationService;
            _messenger = messenger;
            _dialogNavigationService = dialogNavigationService;
        }

        public override void ViewAppeared()
        {
            if (!_alreadyLoaded)
            {
                if (!_messenger.HasSubscriptionsFor<TargetReceivedMessage>())
                    _targetReceivedToken ??= _messenger.Subscribe<TargetReceivedMessage>(msg => TargetReceived(msg.Payload));

                MvxNotifyTask.Create(async () =>
                {
                    await ShowMenuViewModel();
                    await ShowOperationRootTabbedViewModel();
                });
            }
        }

        public override void ViewDisappeared()
        {
            _alreadyLoaded = true;
            base.ViewDisappeared();
        }

        private async Task ShowMenuViewModel()
        {
            await _navigationService.Navigate<MenuViewModel>();
        }

        private async Task ShowOperationRootTabbedViewModel()
        {
            await _navigationService.Navigate<OperationRootTabbedViewModel>();
        }

        private async void TargetReceived(TargetPayload payload)
        {
            await _dialogNavigationService.Navigate<TargetWarningDialogViewModel, TargetWarningDialogNavigationParameter>(
                new TargetWarningDialogNavigationParameter(payload));
        }
    }
}