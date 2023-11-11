using System.Windows.Input;

namespace Rocks.Wasabee.Maui.Core.ViewModels;

public class MainPageViewModel : ViewModelBase
{
	public MainPageViewModel()
	{
        Hello = "Hellowwww";
	}

    private string _hello = "";
    public string Hello
    {
        get => _hello;
        set => SetProperty(ref _hello, value);
    }

    public ICommand ChangeText => new Command(() => Hello = "Hello World!");
}