using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification
{
    public class AgentVerificationNavigationParameter
    {
        public bool ComingFromLogin { get; }

        public AgentVerificationNavigationParameter(bool comingFromLogin = true)
        {
            ComingFromLogin = comingFromLogin;
        }
    }

    public class AgentVerificationViewModel : BaseViewModel, IMvxViewModel<AgentVerificationNavigationParameter>
    {
        private readonly IMvxNavigationService _navigationService;

        private AgentVerificationNavigationParameter _parameter = new ();

        public AgentVerificationViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;

            Steps = new MvxObservableCollection<BaseViewModel>()
            {
                new AgentVerificationStep1SubViewModel(this),
                new AgentVerificationStep2SubViewModel(this),
                new AgentVerificationStep3SubViewModel(this)
            };
        }

        public void Prepare(AgentVerificationNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        #region Properties

        public MvxObservableCollection<BaseViewModel> Steps { get; set; }

        public BaseViewModel? CurrentStep { get; set; }

        public string AgentName { get; set; } = string.Empty;
        
        #endregion

        #region Commands

        public IMvxCommand NextStepCommand => new MvxCommand(NextStepExecuted);
        private void NextStepExecuted()
        {
            var index = CurrentStep != null ? Steps.IndexOf(CurrentStep) + 1 : 0;
            if (index < Steps.Count)
                CurrentStep = Steps[index];
        }

        public IMvxCommand BackStepCommand => new MvxCommand(BackStepExecuted);
        private void BackStepExecuted()
        {
            var index = CurrentStep != null ? Steps.IndexOf(CurrentStep) - 1 : 0;
            if (index >= 0)
                CurrentStep = Steps[index];
        }
        
        public IMvxCommand ExitCommand => new MvxCommand(ExitExecuted);
        private void ExitExecuted()
        {
            _navigationService.Navigate<RootViewModel>();
            if (_parameter.ComingFromLogin is false)
                _navigationService.Navigate<ProfileViewModel>();

            _navigationService.Close(this);
        }

        #endregion
    }
}