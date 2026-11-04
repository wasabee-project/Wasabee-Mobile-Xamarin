using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Views
{
    public abstract partial class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel
    {
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler != null && BindingContext is not TViewModel)
            {
                BindingContext = Handler!.MauiContext?.Services.GetRequiredService<TViewModel>();
            }
        }
    }
}