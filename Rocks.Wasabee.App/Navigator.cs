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

    public void NavigateToRoot<TViewModel>() where TViewModel : ViewModelBase
    {
        if (Application.Current is null)
            throw new Exception($"Application.Current is null");

        var viewModelType = typeof(TViewModel);
        var pageType = GetPageTypeForViewModel(viewModelType);

        if (pageType == null)
            throw new Exception($"Cannot locate page type for {viewModelType}");

        try
        {
            var viewModel = _serviceProvider.GetService(viewModelType) as ViewModelBase;
            var page = _serviceProvider.GetService(pageType) as ContentPage;

            if (viewModel == null)
                throw new Exception($"{viewModelType} not registered in IServiceProvider");
            if (page == null)
                throw new Exception($"{pageType} not registered in IServiceProvider");

            page.BindingContext = viewModel;

            Application.Current.MainPage = new NavigationPage(page);

            page.Appearing -= new EventHandler(OnAppearing);
            page.Appearing += new EventHandler(OnAppearing);
        }
        catch (Exception ex)
        {

        }
    }

    public async Task Navigate<TViewModel>(bool isAnimated = true) where TViewModel : ViewModelBase
    {
        if (Application.Current is null)
            throw new Exception($"Application.Current is null");

        var viewModelType = typeof(TViewModel);
        var pageType = GetPageTypeForViewModel(viewModelType);

        if (pageType == null)
            throw new Exception($"Cannot locate page type for {viewModelType}");

        try
        {
            var viewModel = _serviceProvider.GetService(viewModelType) as ViewModelBase;
            var page = _serviceProvider.GetService(pageType) as ContentPage;

            if (viewModel == null)
                throw new Exception($"{viewModelType} not registered in IServiceProvider");
            if (page == null)
                throw new Exception($"{pageType} not registered in IServiceProvider");

            page.BindingContext = viewModel;

            if (Application.Current.MainPage is AppShell appShell)
                await appShell.Navigation.PushAsync(page, isAnimated);
            else if (Application.Current.MainPage is NavigationPage navigationPage)
                await navigationPage.PushAsync(page, isAnimated);

            page.Appearing -= new EventHandler(OnAppearing);
            page.Appearing += new EventHandler(OnAppearing);
        }
        catch (Exception ex)
        {

        }
    }

    public async Task Navigate<TViewModel, TParameter>(TParameter parameter, bool isAnimated = true)
        where TViewModel : ParameterizableViewModel<TParameter>
        where TParameter : class
    {
        if (Application.Current is null)
            throw new Exception($"Application.Current is null");

        var viewModelType = typeof(TViewModel);
        var pageType = GetPageTypeForViewModel(viewModelType);

        if (pageType == null)
            throw new Exception($"Cannot locate page type for {viewModelType}");

        try
        {
            var viewModel = _serviceProvider.GetService(viewModelType) as ParameterizableViewModel<TParameter>;
            var page = _serviceProvider.GetService(pageType) as ContentPage;

            if (viewModel == null)
                throw new Exception($"{viewModelType} not registered in IServiceProvider");
            if (page == null)
                throw new Exception($"{pageType} not registered in IServiceProvider");

            page.BindingContext = viewModel;

            if (Application.Current.MainPage is AppShell appShell)
                await appShell.Navigation.PushAsync(page, isAnimated);
            else if (Application.Current.MainPage is NavigationPage navigationPage)
                await navigationPage.PushAsync(page, isAnimated);
            
            viewModel.Initialize(parameter);

            page.Appearing -= new EventHandler(OnAppearing);
            page.Appearing += new EventHandler(OnAppearing);
        }
        catch (Exception ex)
        {

        }
    }

    private void OnAppearing(object? sender, EventArgs e)
    {
        if (sender is Page page && page.BindingContext is ViewModelBase viewModelBase)
        {
            viewModelBase.OnAppearing();
        }
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