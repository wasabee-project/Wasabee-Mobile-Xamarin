using MvvmCross.Commands;
using Rocks.Wasabee.Mobile.Core.Services;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public abstract class BaseDialogViewModel : BaseViewModel
    {
        protected readonly IDialogNavigationService DialogNavigationService;

        protected BaseDialogViewModel(IDialogNavigationService dialogNavigationService)
        {
            DialogNavigationService = dialogNavigationService;
        }

        #region Commands

        public IMvxCommand CloseCommand => new MvxCommand(CloseExecuted);
        private async void CloseExecuted()
        {
            if (IsBusy)
                return;

            await DialogNavigationService.Close();
        }

        #endregion
    }
}