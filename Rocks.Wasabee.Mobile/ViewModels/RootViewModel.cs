using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

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
                    await ShowHomeViewModel();
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

        private async Task ShowHomeViewModel()
        {
            await _navigationService.Navigate<HomeViewModel>();
        }
    }
}