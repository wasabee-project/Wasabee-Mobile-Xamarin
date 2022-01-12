using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System.Threading;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels
{
    public class TelegramLinkingStep3SubViewModel : BaseViewModel
    {
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly UsersDatabase _usersDatabase;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IBrowser _browser;
        private readonly IClipboard _clipboard;
        private readonly IMvxMessenger _messenger;

        private readonly MvxSubscriptionToken? _mvxToken;

        public TelegramLinkingStep3SubViewModel(TelegramLinkingViewModel parent)
        {
            Parent = parent;

            _wasabeeApiV1Service = Mvx.IoCProvider.Resolve<WasabeeApiV1Service>();
            _usersDatabase = Mvx.IoCProvider.Resolve<UsersDatabase>();
            _userSettingsService = Mvx.IoCProvider.Resolve<IUserSettingsService>();
            _browser = Mvx.IoCProvider.Resolve<IBrowser>();
            _clipboard = Mvx.IoCProvider.Resolve<IClipboard>();
            _messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();

            _mvxToken ??= _messenger.Subscribe<MessageFor<TelegramLinkingStep3SubViewModel>>(_ => RefreshToken());
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            RefreshToken();
        }

        #region Properties

        public TelegramLinkingViewModel Parent { get; }
        
        public string AuthToken { get; set; } = string.Empty;

        public bool IsExitButtonVisible { get; set; }
        public bool IsTokenReady { get; set; }
        public bool IsSubStep1Visible { get; set; } = true;
        public bool IsSubStep2Visible { get; set; }
        public bool IsSubStep3Visible { get; set; }

        #endregion

        #region Commands

        public IMvxCommand CopyTokenCommand => new MvxCommand(CopyTokenExecuted);
        private async void CopyTokenExecuted()
        {
            if (IsTokenReady is false)
                return;

            await _clipboard.SetTextAsync(AuthToken);

            if (_clipboard.HasText)
            {
                var text = await _clipboard.GetTextAsync();
                if (string.Equals(text, AuthToken))
                {
                    IsSubStep1Visible = false;
                    IsSubStep2Visible = true;
                }
            }
        }

        private int _refreshCount;
        public IMvxCommand ReopenBotCommand => new MvxCommand(ReopenBotExecuted);
        private async void ReopenBotExecuted()
        {
            if (IsSubStep2Visible is false)
                return;

            await _browser.OpenAsync($"https://t.me/{Parent.BotUsername}");

            IsBusy = true;

            var hasSent = false;
            do
            {
                var user = await _wasabeeApiV1Service.User_GetUserInformations();
                if (user is not null)
                {
                    hasSent = user.Telegram.Verified;
                    if (hasSent)
                        await _usersDatabase.SaveUserModel(user);
                }

                _refreshCount++;
                if (hasSent is false)
                    Thread.Sleep(5000);

            } while (hasSent is false && _refreshCount < 60);

            IsBusy = false;

            if (hasSent)
            {
                IsSubStep2Visible = false;
                IsSubStep3Visible = true;
                IsExitButtonVisible = true;

                _messenger.Publish(new MessageFor<TelegramLinkingStep3SubViewModel>(this));
            }
        }

        #endregion

        #region Private methods

        private async void RefreshToken()
        {
            var userId = _userSettingsService.GetLoggedUserGoogleId();
            var user = await _usersDatabase.GetUserModel(userId);

            if (user is null || string.IsNullOrWhiteSpace(user.Telegram.AuthToken))
                user = await _wasabeeApiV1Service.User_GetUserInformations();
            if (user is null)
                return;

            AuthToken = user.Telegram.AuthToken;

            if (string.IsNullOrWhiteSpace(AuthToken) is false)
            {
                IsTokenReady = true;

                _mvxToken?.Dispose();
            }
        }

        #endregion
    }
}