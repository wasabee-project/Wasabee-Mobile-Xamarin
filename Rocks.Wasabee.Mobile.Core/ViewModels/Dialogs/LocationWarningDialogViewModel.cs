using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class LocationWarningDialogResult
    {
        public bool Accepted { get; } = false;
        public bool NeverShowWarningAgain { get; } = false;

        public LocationWarningDialogResult(bool accepted, bool neverShowWarningAgain)
        {
            Accepted = accepted;
            NeverShowWarningAgain = neverShowWarningAgain;
        }
    }

    public class LocationWarningDialogViewModel : BaseDialogViewModel, IMvxViewModelResult<LocationWarningDialogResult>
    {
        private readonly IAppSettings _appSettings;

        public LocationWarningDialogViewModel(IDialogNavigationService dialogNavigationService, IAppSettings appSettings) : base(dialogNavigationService)
        {
            _appSettings = appSettings;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            // Start timer here to ensure user has read
        }

        #region Properties

        public bool CanClose { get; set; }
        public bool NeverShowAgain { get; set; }

        #endregion

        #region Commands

        public IMvxCommand DenyCommand => new MvxCommand(DenyExecuted);
        private async void DenyExecuted()
        {
            var result = new LocationWarningDialogResult(false, false);
            CloseCompletionSource.SetResult(result);

            await DialogNavigationService.Close(this, true, result);
        }

        public IMvxCommand AcceptCommand => new MvxCommand(AcceptExecuted);
        private async void AcceptExecuted()
        {
            var result = new LocationWarningDialogResult(true, NeverShowAgain);
            CloseCompletionSource.SetResult(result);

            await DialogNavigationService.Close(this, true, result);
        }

        public IMvxAsyncCommand OpenPrivacyPolicyCommand => new MvxAsyncCommand(OpenPrivacyPolicyExecuted);
        private async Task OpenPrivacyPolicyExecuted()
        {
            var uri = _appSettings.WasabeeBaseUrl + "/privacy";
            await Launcher.OpenAsync(new Uri(uri));
        }

        #endregion

        #region IMvxViewModelResult<TResult> implementation

        public TaskCompletionSource<object> CloseCompletionSource { get; set; } = new TaskCompletionSource<object>();

        public override void ViewDestroy(bool viewFinishing = true)
        {
            if (viewFinishing && !CloseCompletionSource.Task.IsCompleted && !CloseCompletionSource.Task.IsFaulted)
                CloseCompletionSource?.TrySetCanceled();

            base.ViewDestroy(viewFinishing);
        }

        #endregion
    }
}