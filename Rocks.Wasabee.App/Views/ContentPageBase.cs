using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.App.Views;

public interface IPage { }

public abstract class ContentPageBase<TViewModel> : ContentPage, IPage where TViewModel : ViewModelBase
{
    protected ContentPageBase(TViewModel viewModel)
    {
        BindingContext = viewModel;
    }
}
