using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Core;

public interface INavigator
{
    Task Navigate<TViewModel>(object? parameter = null, bool isRootPage = false, bool isPopupPage = false, bool isSubscribeOnAppear = false, bool isFetchUI = false) where TViewModel : ViewModelBase;
}