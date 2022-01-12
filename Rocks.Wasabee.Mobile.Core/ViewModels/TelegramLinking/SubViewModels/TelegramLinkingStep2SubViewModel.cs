﻿using MvvmCross;
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
    public class TelegramLinkingStep2SubViewModel : BaseViewModel
    {
        private readonly IClipboard _clipboard;
        private readonly IBrowser _browser;
        private readonly IMvxMessenger _messenger;
        private readonly IUserSettingsService _userSettingsService;
        private readonly UsersDatabase _usersDatabase;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        public TelegramLinkingStep2SubViewModel(TelegramLinkingViewModel parent)
        {
            Parent = parent;
            
            _clipboard = Mvx.IoCProvider.Resolve<IClipboard>();
            _browser = Mvx.IoCProvider.Resolve<IBrowser>();
            _messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
            _userSettingsService = Mvx.IoCProvider.Resolve<IUserSettingsService>();
            _usersDatabase = Mvx.IoCProvider.Resolve<UsersDatabase>();
            _wasabeeApiV1Service = Mvx.IoCProvider.Resolve<WasabeeApiV1Service>();
        }

        public override async void ViewAppearing()
        {
            base.ViewAppearing();

            var userId = _userSettingsService.GetLoggedUserGoogleId();
            var user = await _usersDatabase.GetUserModel(userId);
            if (user is not null)
            {
                Token = user.OneTimeToken;
                IsTokenReady = true;
            }
            else
            {
                // TODO
            }
        }

        #region Properties

        public TelegramLinkingViewModel Parent { get; }
        
        public string Token { get; set; } = string.Empty;

        public bool IsTokenReady { get; set; }
        public bool IsNextStepButtonVisible { get; set; }

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

            await _clipboard.SetTextAsync(Token);

            if (_clipboard.HasText)
            {
                var text = await _clipboard.GetTextAsync();
                if (string.Equals(text, Token))
                {
                    IsSubStep1Visible = false;
                    IsSubStep2Visible = true;
                }
            }
        }

        private int _refreshCount;
        public IMvxCommand StartBotCommand => new MvxCommand(StartBotExecuted);
        private async void StartBotExecuted()
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
                    hasSent = string.IsNullOrEmpty(user.Telegram.AuthToken) is false;
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
                IsNextStepButtonVisible = true;

                _messenger.Publish(new MessageFor<TelegramLinkingStep3SubViewModel>(this));
            }
        }

        #endregion

    }
}