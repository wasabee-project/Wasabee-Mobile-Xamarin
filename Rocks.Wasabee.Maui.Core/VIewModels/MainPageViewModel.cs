using System.Windows.Input;

namespace Rocks.Wasabee.Maui.Core.ViewModels;

public class MainPageViewModel : ViewModelBase
{
    private readonly INavigator _navigator;

    public MainPageViewModel(INavigator navigator)
	{
        Hello = "Hellowwww";
        _navigator = navigator;
    }

    private string _hello = "";

    public string Hello
    {
        get => _hello;
        set => SetProperty(ref _hello, value);
    }

    public ICommand ChangeText => new Command(async () => await _navigator.Navigate<SecondPageViewModel>());
}