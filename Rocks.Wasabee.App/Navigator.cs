using System.Globalization;
using System.Reflection;
using Rocks.Wasabee.App.Views;
using Rocks.Wasabee.Maui.Core;
using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.App;

public class Navigator : INavigator
{
    private readonly IServiceProvider _serviceProvider;

	public Navigator(IServiceProvider serviceProvider)
	{
        _serviceProvider = serviceProvider;
	}

    public async Task Navigate<TViewModel>(object? parameter = null, bool isRootPage = false, bool isPopupPage = false, bool isSubscribeOnAppear = false, bool isFetchUI = false) where TViewModel : ViewModelBase
    {
        var viewModelType = typeof(TViewModel);
        var pageType = GetPageTypeForViewModel(viewModelType);

        if (pageType == null)
            throw new Exception($"Cannot locate page type for {viewModelType}");

        try
        {
            var viewModel = _serviceProvider.GetService(viewModelType) as ViewModelBase;
            var page = _serviceProvider.GetService(pageType) as ContentPageBase<TViewModel>;

            if (viewModel == null)
                throw new Exception($"{viewModelType} not registered in IServiceProvider");
            if (page == null)
                throw new Exception($"{pageType} not registered in IServiceProvider");

            if (isRootPage)
                Application.Current.MainPage = new AppShell();

            var appShell = Application.Current.MainPage as AppShell;
            if (appShell != null)
                await appShell.Navigation.PushAsync(page, true);
            else
            {
                Application.Current.MainPage = new AppShell();
                await appShell.Navigation.PushAsync(page, true);
            }

            if (!isFetchUI)
                viewModel.Initialize(parameter);
            else
                viewModel.Initialize(parameter, page);

            if (isSubscribeOnAppear)
            {
                page.Appearing -= new EventHandler(OnAppearing);
                page.Appearing += new EventHandler(OnAppearing);
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void OnAppearing(object sender, EventArgs e)
    {
        var viewModel = ((Page)sender).BindingContext as ViewModelBase;
        viewModel.OnAppearing();
    }

    private static Type? GetPageTypeForViewModel(Type viewModelType)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var viewsAssemblyName = currentAssembly.FullName;

        var currentAssemblyName = currentAssembly.GetName().Name;
        var viewName = string.Format(CultureInfo.InvariantCulture, "{0}.Views.{1}", currentAssemblyName, viewModelType.Name.Replace("ViewModel", string.Empty));

        var viewAssemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewsAssemblyName);
        var viewType = Type.GetType(viewAssemblyName);

        return viewType;
    }
}