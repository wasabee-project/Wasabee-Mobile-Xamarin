using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System.Threading.Tasks;
using MvvmCross;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public class RootViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        private bool _alreadyLoaded;

        public RootViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public override void ViewAppeared()
        {
            if (!_alreadyLoaded)
            {
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
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<MenuViewModel>());
        }

        private async Task ShowOperationRootTabbedViewModel()
        {
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<OperationRootTabbedViewModel>());
        }
    }
}