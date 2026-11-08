using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Views
{
    public abstract class BaseContentPage : ContentPage
    {
        protected BaseContentPage() { }
    }

    public abstract class BaseContentPage<TViewModel> : BaseContentPage where TViewModel : BaseViewModel
    {
        protected TViewModel ViewModel
        {
            get;
            private init
            {
                field = value;
                OnViewModelSet();
            }
        }

        protected BaseContentPage(TViewModel viewModel)
        {
            ViewModel = viewModel;
            BindingContext = viewModel;

            Shell.SetNavBarIsVisible(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.OnAppearing();
        }

        protected virtual void OnViewModelSet() { }
    }
}