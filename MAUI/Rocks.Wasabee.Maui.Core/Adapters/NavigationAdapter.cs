using Rocks.Wasabee.Core.Services;
using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Core.Adapters
{
    public class NavigationAdapter : INavigationService
    {
        public async Task NavigateToAsync<TViewModel>(Dictionary<string, object>? parameters = null) where TViewModel : BaseViewModel
        {
            await ExecuteNavigationAsync(GetViewName<TViewModel>(), parameters);
        }

        public async Task NavigateToRootAsync<TViewModel>(Dictionary<string, object>? parameters = null) where TViewModel : BaseViewModel
        {
            var absoluteRoute = $"//Root/{GetViewName<TViewModel>()}";

            await ExecuteNavigationAsync(absoluteRoute, parameters);
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private static string GetViewName<TViewModel>() => typeof(TViewModel).Name.Replace("ViewModel", "Page");

        private static async Task ExecuteNavigationAsync(string route, Dictionary<string, object>? parameters)
        {
            if (parameters != null)
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }
    }
}
