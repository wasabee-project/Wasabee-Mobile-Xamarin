using System;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels
{
    public class TelegramLinkingStep1SubViewModel : BaseViewModel
    {
        public TelegramLinkingStep1SubViewModel(TelegramLinkingViewModel parent, bool isDontAskAgainVisible = true)
        {
            Parent = parent;
            IsDontAskAgainVisible = isDontAskAgainVisible;
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            Device.StartTimer(TimeSpan.FromMilliseconds(100), () => AddProgress(1));
        }

        #region Properties

        public TelegramLinkingViewModel Parent { get; }
        public bool IsDontAskAgainVisible { get; }
        
        public bool IsDontAskAgainChecked { get; set; }
        public bool IsNextStepEnabled { get; set; }
        public int Progress { get; set; }

        #endregion

        #region Private methods

        private bool AddProgress(int add)
        {
            var value = Progress + add;

            if (value > 100) 
                IsNextStepEnabled = true;

            Progress = value;

            return !IsNextStepEnabled;
        }

        #endregion
    }
}