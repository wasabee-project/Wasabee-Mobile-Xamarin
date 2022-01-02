using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels
{
    public class AgentVerificationStep2SubViewModel : BaseViewModel
    {
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IUserDialogs _userDialogs;
        private readonly IClipboard _clipboard;

        public AgentVerificationStep2SubViewModel(AgentVerificationViewModel parent)
        {
            Parent = parent;

            _wasabeeApiV1Service = Mvx.IoCProvider.Resolve<WasabeeApiV1Service>();
            _userDialogs = Mvx.IoCProvider.Resolve<IUserDialogs>();
            _clipboard = Mvx.IoCProvider.Resolve<IClipboard>();
        }

        #region Properties

        public AgentVerificationViewModel Parent { get; }
        
        public string Token { get; set; } = string.Empty;
        public bool IsTokenReady { get; set; }
        public bool IsNextStepButtonVisible { get; set; }

        #endregion

        #region Commands

        public IMvxCommand GetTokenCommand => new MvxCommand(GetTokenExecuted);
        private async void GetTokenExecuted()
        {
            if (IsTokenReady)
                return;

            if (string.IsNullOrWhiteSpace(Parent.AgentName))
            {
                _userDialogs.Alert(Strings.Dialogs_Warning_InGameNameRequired);
                return;
            }

            IsBusy = true;

            var token = await _wasabeeApiV1Service.User_GetVerificationToken(Parent.AgentName);
            if (string.IsNullOrWhiteSpace(token))
                _userDialogs.Toast(Strings.Dialogs_Warning_InGameNameAlreadyVerified);
            else
            {
                Token = token;
                IsTokenReady = true;
            }

            IsBusy = false;
        }

        public IMvxCommand CopyTokenCommand => new MvxCommand(CopyTokenExecuted);
        private async void CopyTokenExecuted()
        {
            if (IsTokenReady is false)
                return;

            await _clipboard.SetTextAsync(Token);

            if (_clipboard.HasText)
            {
                var text = await _clipboard.GetTextAsync();
                if (string.Equals(text, Token))
                    IsNextStepButtonVisible = true;
            }
        }

        public IMvxCommand ResetCommand => new MvxCommand(ResetExecuted);
        private void ResetExecuted()
        {
            if (IsTokenReady is false)
                return;

            IsTokenReady = false;
            IsNextStepButtonVisible = false;

            Token = string.Empty;
            Parent.AgentName = string.Empty;
        }

        #endregion

    }
}