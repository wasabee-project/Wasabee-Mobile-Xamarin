using Acr.UserDialogs;
using MvvmCross.Forms.Views;
using MvvmCross.WeakSubscription;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using System.ComponentModel;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views
{
    public partial class BaseContentPage<TViewModel> : MvxContentPage<TViewModel> where TViewModel : BaseViewModel
    {
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            ViewModel.WeakSubscribe(() => ViewModel.IsBusy, ViewModelIsBusyChanged);
        }

        private void ViewModelIsBusyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ViewModel.IsBusy)
                UserDialogs.Instance.ShowLoading();
            else
            {
                UserDialogs.Instance.HideLoading();
            }
        }
    }
}