using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs
{
    public class LocationWarningDialogViewModel : BaseDialogViewModel
    {
        private readonly IMvxMessenger _messenger;
        private readonly IPreferences _preferences;

        public LocationWarningDialogViewModel(IDialogNavigationService dialogNavigationService, IMvxMessenger messenger,
            IPreferences preferences) : base(dialogNavigationService)
        {
            _messenger = messenger;
            _preferences = preferences;
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

        #endregion
    }
}