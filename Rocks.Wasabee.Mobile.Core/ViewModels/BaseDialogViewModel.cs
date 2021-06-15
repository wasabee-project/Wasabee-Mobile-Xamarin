using MvvmCross.Commands;
using Rocks.Wasabee.Mobile.Core.Services;
using System.Threading.Tasks;

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

        public IMvxAsyncCommand CloseCommand => new MvxAsyncCommand(CloseExecuted);
        private async Task CloseExecuted()
        {
            if (IsBusy)
                return;

            await DialogNavigationService.Close();
        }

        #endregion
    }
}