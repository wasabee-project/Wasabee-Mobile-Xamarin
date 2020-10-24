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

        protected IMvxCommand CloseCommand => new MvxCommand(CloseExecuted, () => !IsBusy);
        private async void CloseExecuted()
        {
            await DialogNavigationService.Close();
        }

        #endregion
    }
}