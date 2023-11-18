using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rocks.Wasabee.Maui.Core.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected internal bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    public virtual void Initialize() { }

    public virtual void OnAppearing() { }
}

public abstract class ParameterizableViewModel<TParameter> : ViewModelBase where TParameter : class
{
    public virtual void Initialize(TParameter parameter) { }
}