using MvvmCross.Base;
using MvvmCross.Navigation;
using MvvmCross.Navigation.EventArguments;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels;

namespace Rocks.Wasabee.Mobile.Core.Infra.Navigation
{
    public class NavigationService : MvxNavigationService
    {
        public NavigationService(IMvxNavigationCache navigationCache, IMvxViewModelLoader viewModelLoader)
            : base(navigationCache, viewModelLoader)
        {
            base.BeforeClose += This_OnBeforeClose;
            base.BeforeNavigate += This_OnBeforeNavigate;
        }

        private void This_OnBeforeClose(object sender, IMvxNavigateEventArgs e)
        {
            base.OnBeforeClose(sender, e);

            if (e.ViewModel is BaseViewModel baseViewModel)
                baseViewModel.Dispose();
        }

        private void This_OnBeforeNavigate(object sender, IMvxNavigateEventArgs e)
        {
            base.OnBeforeNavigate(sender, e);

            if (e.ViewModel is BaseViewModel baseViewModel)
                baseViewModel.Dispose();
        }
    }
}