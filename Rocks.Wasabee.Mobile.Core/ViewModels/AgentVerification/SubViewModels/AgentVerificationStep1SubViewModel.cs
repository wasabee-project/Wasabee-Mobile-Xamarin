namespace Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels
{
    public class AgentVerificationStep1SubViewModel : BaseViewModel
    {
        public AgentVerificationStep1SubViewModel(AgentVerificationViewModel parent, bool isDontAskAgainVisible = true)
        {
            Parent = parent;
            IsDontAskAgainVisible = isDontAskAgainVisible;
        }

        #region Properties

        public AgentVerificationViewModel Parent { get; }
        public bool IsDontAskAgainVisible { get; }
        
        public bool IsDontAskAgainChecked { get; set; }

        #endregion

        #region Commands

        

        #endregion
    }
}