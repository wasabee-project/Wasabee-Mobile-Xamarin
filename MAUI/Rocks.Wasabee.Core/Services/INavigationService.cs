using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Core.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync<TViewModel>(Dictionary<string, object>? parameters = null) where TViewModel : BaseViewModel;
        Task NavigateToRootAsync<TViewModel>(Dictionary<string, object>? parameters = null) where TViewModel : BaseViewModel;

        Task GoBackAsync();
    }
}