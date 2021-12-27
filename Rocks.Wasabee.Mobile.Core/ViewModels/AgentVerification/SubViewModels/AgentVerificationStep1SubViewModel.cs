using System;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels
{
    public class AgentVerificationStep1SubViewModel : BaseViewModel
    {
        public AgentVerificationStep1SubViewModel(AgentVerificationViewModel parent, bool isDontAskAgainVisible = true)
        {
            Parent = parent;
            IsDontAskAgainVisible = isDontAskAgainVisible;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            Device.StartTimer(TimeSpan.FromMilliseconds(100), () => AddProgress(1));
        }

        #region Properties

        public AgentVerificationViewModel Parent { get; }
        public bool IsDontAskAgainVisible { get; }
        
        public bool IsDontAskAgainChecked { get; set; }
        public bool IsNextStepEnabled { get; set; }
        public int Progress { get; set; }

        #endregion

        #region Commands

        

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