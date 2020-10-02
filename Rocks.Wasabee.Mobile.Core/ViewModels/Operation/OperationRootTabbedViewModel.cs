using MvvmCross.Commands;
using MvvmCross.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
{
    public class OperationRootTabbedViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        public OperationRootTabbedViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
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