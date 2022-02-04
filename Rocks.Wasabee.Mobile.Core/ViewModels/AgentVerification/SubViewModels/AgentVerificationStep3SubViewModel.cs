using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels
{
    public class AgentVerificationStep3SubViewModel : BaseViewModel
    {
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IUserDialogs _userDialogs;
        private readonly IBrowser _browser;

        public AgentVerificationStep3SubViewModel(AgentVerificationViewModel parent)
        {
            Parent = parent;

            _wasabeeApiV1Service = Mvx.IoCProvider.Resolve<WasabeeApiV1Service>();
            _userDialogs = Mvx.IoCProvider.Resolve<IUserDialogs>();
            _browser = Mvx.IoCProvider.Resolve<IBrowser>();
        }

        #region Properties

        public AgentVerificationViewModel Parent { get; }

        public bool HasOpenedCommunity { get; set; }
        public bool IsExitButtonVisible { get; set; }
        public bool IsVerified { get; set; }

        #endregion

        #region Commands

        public IMvxCommand OpenCommunityCommand => new MvxCommand(OpenCommunityExecuted);
        private async void OpenCommunityExecuted()
        {
            await _browser.OpenAsync("https://community.ingress.com/en/activity", BrowserLaunchMode.SystemPreferred);

            HasOpenedCommunity = true;
        }

        public IMvxCommand RefreshStatusCommand => new MvxCommand(RefreshStatusExecuted);
        private async void RefreshStatusExecuted()
        {
            if (IsBusy || HasOpenedCommunity is false)
                return;

            if (string.IsNullOrWhiteSpace(Parent.AgentName))
                Parent.BackStepCommand.Execute();

            IsBusy = true;

            var result = await _wasabeeApiV1Service.User_GetVerificationStatus(Parent.AgentName);
            if (result)
            {
                IsVerified = true;
                IsExitButtonVisible = true;
            }
            else
                _userDialogs.Alert(Strings.Dialogs_Warning_ACVFailed);

            IsBusy = false;
        }

        #endregion

    }
}