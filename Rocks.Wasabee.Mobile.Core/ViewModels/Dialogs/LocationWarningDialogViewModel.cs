using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class LocationWarningDialogViewModel : BaseDialogViewModel
    {
        private readonly IMvxMessenger _messenger;
        private readonly IPreferences _preferences;
        private readonly IAppSettings _appSettings;

        public LocationWarningDialogViewModel(IDialogNavigationService dialogNavigationService, IMvxMessenger messenger,
            IPreferences preferences, IAppSettings appSettings) : base(dialogNavigationService)
        {
            _messenger = messenger;
            _preferences = preferences;
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

        public IMvxCommand AcceptCommand => new MvxCommand(AcceptExecuted);
        private void AcceptExecuted()
        {
            _messenger.Publish(new MessageFrom<LocationWarningDialogViewModel>(this, NeverShowAgain));

            base.CloseCommand.Execute();
        }
        public IMvxAsyncCommand OpenPrivacyPolicyCommand => new MvxAsyncCommand(OpenPrivacyPolicyExecuted);
        private async Task OpenPrivacyPolicyExecuted()
        {
            var uri = _appSettings.WasabeeBaseUrl + "/privacy";
            await Launcher.OpenAsync(new Uri(uri));
        }

        #endregion
    }
}