using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.Map;
using System.Threading.Tasks;

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
                    await ShowMapViewModel();
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

        private async Task ShowMapViewModel()
        {
            await _navigationService.Navigate<MapViewModel>();
        }
    }
}