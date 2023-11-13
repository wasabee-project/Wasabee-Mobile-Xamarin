using System.Windows.Input;

namespace Rocks.Wasabee.Maui.Core.ViewModels;

public class SecondPageViewModel : ViewModelBase
{
    public SecondPageViewModel()
    {
        Hello = "Second Page!!!";
    }

    private string _hello = "";
    public string Hello
    {
        get => _hello;
        set => SetProperty(ref _hello, value);
    }

    public ICommand ChangeText => new Command(() => Hello = "Hell yeah!");
}