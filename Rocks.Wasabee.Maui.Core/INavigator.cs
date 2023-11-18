using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Core;

public interface INavigator
{
    void NavigateToRoot<TViewModel>() where TViewModel : ViewModelBase;

    Task Navigate<TViewModel>(bool isAnimated = true) where TViewModel : ViewModelBase;
    Task Navigate<TViewModel, TParameter>(TParameter parameter, bool isAnimated = true) where TViewModel : ParameterizableViewModel<TParameter> where TParameter : class;
}