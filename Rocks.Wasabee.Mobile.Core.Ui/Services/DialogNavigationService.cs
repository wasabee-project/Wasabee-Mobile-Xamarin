using MvvmCross;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using Rg.Plugins.Popup.Contracts;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Ui.Views;
using System;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Ui.Services
{
    public class DialogNavigationService : IDialogNavigationService
    {
        private readonly IPopupNavigation _popupNavigationService;
        private readonly IMvxViewModelLoader _viewModelLoader;


        public DialogNavigationService(IPopupNavigation popupNavigationService, IMvxViewModelLoader viewModelLoader)
        {
            _popupNavigationService = popupNavigationService;
            _viewModelLoader = viewModelLoader;
        }

        public Task Navigate<TViewModel>() where TViewModel : IMvxViewModel
        {
            return Navigate(typeof(TViewModel));
        }

        public Task Navigate<TViewModel, TParameter>(TParameter param) where TViewModel : IMvxViewModel<TParameter> where TParameter : class
        {
            return Navigate(typeof(TViewModel), param);
        }

        public Task Close(bool animated = true)
        {
            return _popupNavigationService.PopAsync(animated);
        }

        #region Properties

        private IMvxViewsContainer _viewsContainer;
        protected virtual IMvxViewsContainer ViewsContainer => _viewsContainer ??= Mvx.IoCProvider.Resolve<IMvxViewsContainer>();

        #endregion


        #region Protected methods

        protected virtual async Task Navigate(Type viewModelType)
        {
            var request = new MvxViewModelInstanceRequest(viewModelType);
            request.ViewModelInstance = _viewModelLoader.LoadViewModel(request, null);
            await Navigate(request, request.ViewModelInstance).ConfigureAwait(false);
        }

        protected virtual async Task Navigate<TParameter>(Type viewModelType, TParameter param) where TParameter : class
        {
            var request = new MvxViewModelInstanceRequest(viewModelType);
            request.ViewModelInstance = _viewModelLoader.LoadViewModel(request, param, null);
            await Navigate(request, request.ViewModelInstance).ConfigureAwait(false);
        }

        protected virtual async Task Navigate(MvxViewModelRequest request, IMvxViewModel viewModel)
        {
            if (viewModel.InitializeTask?.Task != null)
                await viewModel.InitializeTask.Task.ConfigureAwait(false);

            var viewType = ViewsContainer.GetViewType(request.ViewModelType);
            var view = (BaseDialogPage)Activator.CreateInstance(viewType);
            view.ViewModel = viewModel;

            await _popupNavigationService.PushAsync(view);
        }

        #endregion
    }
}