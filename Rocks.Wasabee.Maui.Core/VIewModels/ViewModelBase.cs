using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Rocks.Wasabee.Maui.Core.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }
}

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