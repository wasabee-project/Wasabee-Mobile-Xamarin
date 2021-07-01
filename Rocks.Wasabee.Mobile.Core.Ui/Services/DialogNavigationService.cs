using MvvmCross;
using MvvmCross.Core;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using Rg.Plugins.Popup.Contracts;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Ui.Views;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
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

        protected ConditionalWeakTable<IMvxViewModel, TaskCompletionSource<object>> _tcsResults = new ConditionalWeakTable<IMvxViewModel, TaskCompletionSource<object>>();

        public Task Navigate<TViewModel>() where TViewModel : IMvxViewModel
        {
            return Navigate(typeof(TViewModel));
        }

        public Task Navigate<TViewModel, TParameter>(TParameter param) where TViewModel : IMvxViewModel<TParameter>
            where TParameter : notnull
        {
            return Navigate(typeof(TViewModel), param);
        }

        public Task<TResult> Navigate<TViewModel, TResult>() where TViewModel : IMvxViewModelResult<TResult>
            where TResult : notnull
        {
            return Navigate<TResult>(typeof(TViewModel), null, new CancellationToken());
        }

        public async Task<bool> Close(bool animated = true)
        {
            var count = _popupNavigationService.PopupStack.Count;
            await _popupNavigationService.PopAsync(animated);

            return _popupNavigationService.PopupStack.Count < count;
        }

        public Task<bool> Close<TResult>(IMvxViewModelResult<TResult> viewModel, bool animated = true, TResult closeResult = default!) where TResult : notnull
        {
            return Close(viewModel, closeResult, new CancellationToken());
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

        protected virtual async Task Navigate<TParameter>(Type viewModelType, TParameter param) where TParameter : notnull
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

        public virtual async Task<TResult> Navigate<TResult>(Type viewModel, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default)
            where TResult : notnull
        {
            var request = new MvxViewModelInstanceRequest(viewModel) { PresentationValues = presentationBundle?.SafeGetData() };
            request.ViewModelInstance = _viewModelLoader.LoadViewModel(request, presentationBundle, null);
            return await Navigate<TResult>(request, (IMvxViewModelResult<TResult>)request.ViewModelInstance, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<TResult> Navigate<TResult>(MvxViewModelRequest request, IMvxViewModelResult<TResult> viewModel, CancellationToken cancellationToken = default)
            where TResult : notnull
        {
            var hasNavigated = false;
            var tcs = new TaskCompletionSource<object>();

            if (cancellationToken != default)
            {
                cancellationToken.Register(async () =>
                {
                    if (hasNavigated && !tcs.Task.IsCompleted)
                        await Close(viewModel, default, cancellationToken);
                });
            }

            viewModel.CloseCompletionSource = tcs;
            _tcsResults.Add(viewModel, tcs);

            if (cancellationToken.IsCancellationRequested)
                return default(TResult);

            var viewType = ViewsContainer.GetViewType(request.ViewModelType);
            var view = (BaseDialogPage)Activator.CreateInstance(viewType);
            view.ViewModel = viewModel;

            await _popupNavigationService.PushAsync(view);
            hasNavigated = true;

            if (viewModel.InitializeTask?.Task != null)
                await viewModel.InitializeTask.Task.ConfigureAwait(false);

            try
            {
                return (TResult)await tcs.Task;
            }
            catch (Exception)
            {
                return default;
            }
        }

        protected virtual async Task<bool> Close<TResult>(IMvxViewModelResult<TResult> viewModel, TResult result, CancellationToken cancellationToken = default)
            where TResult : notnull
        {
            _tcsResults.TryGetValue(viewModel, out var _tcs);

            //Disable cancelation of the Task when closing ViewModel through the service
            viewModel.CloseCompletionSource = null;

            try
            {
                var closeResult = await Close();
                if (closeResult)
                {
                    _tcs?.TrySetResult(result);
                    _tcsResults.Remove(viewModel);
                }
                else
                    viewModel.CloseCompletionSource = _tcs;

                return closeResult;
            }
            catch (Exception ex)
            {
                _tcs?.TrySetException(ex);
                return false;
            }
        }

        #endregion
    }
}