using Rocks.Wasabee.App.Views;
using Rocks.Wasabee.Maui.Core;
using Rocks.Wasabee.Maui.Core.VIewModels.Login;

namespace Rocks.Wasabee.App;

public partial class WasabeeApp : Application
{
    private readonly INavigator _navigator;

    public WasabeeApp(INavigator navigator)
	{
        _navigator = navigator;

        InitializeComponent();
        MainPage = new NavigationPage(new ContentPage());
    }

    protected override void OnStart()
    {
        base.OnStart();

        _navigator.NavigateToRoot<LoginPageViewModel>();
    }
}

