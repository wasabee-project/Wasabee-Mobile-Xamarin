using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

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
        private readonly ILoggingService _loggingService;
        private readonly IPreferences _preferences;
        private readonly IUserSettingsService _userSettingsService;

        private AgentVerificationNavigationParameter _parameter = new ();

        public AgentVerificationViewModel(IMvxNavigationService navigationService, ILoggingService loggingService,
            IPreferences preferences, IUserSettingsService userSettingsService)
        {
            _navigationService = navigationService;
            _loggingService = loggingService;
            _preferences = preferences;
            _userSettingsService = userSettingsService;
        }

        public void Prepare(AgentVerificationNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        public override Task Initialize()
        {
            Steps = new MvxObservableCollection<BaseViewModel>()
            {
                new AgentVerificationStep1SubViewModel(this, _parameter.ComingFromLogin),
                new AgentVerificationStep2SubViewModel(this),
                new AgentVerificationStep3SubViewModel(this)
            };

            return base.Initialize();
        }

        #region Properties

        public MvxObservableCollection<BaseViewModel>? Steps { get; set; }

        public BaseViewModel? CurrentStep { get; set; }

        public string AgentName { get; set; } = string.Empty;
        
        #endregion

        #region Commands

        public IMvxCommand NextStepCommand => new MvxCommand(NextStepExecuted);
        private void NextStepExecuted()
        {
            if (Steps is null || Steps.Count == 0)
                return;

            var index = CurrentStep != null ? Steps.IndexOf(CurrentStep) + 1 : 0;
            if (index < Steps.Count)
                CurrentStep = Steps[index];
        }

        public IMvxCommand BackStepCommand => new MvxCommand(BackStepExecuted);
        private void BackStepExecuted()
        {
            if (Steps is null || Steps.Count == 0)
                return;

            var index = CurrentStep != null ? Steps.IndexOf(CurrentStep) - 1 : 0;
            if (index >= 0)
                CurrentStep = Steps[index];
        }
        
        public IMvxCommand ExitCommand => new MvxCommand(ExitExecuted);
        private void ExitExecuted()
        {
            SaveDontAskAgainSetting();

            _navigationService.Navigate<RootViewModel>();
            if (_parameter.ComingFromLogin is false)
                _navigationService.Navigate<ProfileViewModel>();

            _navigationService.Close(this);
        }

        #endregion

        #region Private methods

        private void SaveDontAskAgainSetting()
        {
            if (CurrentStep is not AgentVerificationStep1SubViewModel step1 || step1.IsDontAskAgainVisible is false) 
                return;
            
            var loggedUserId = _userSettingsService.GetLoggedUserGoogleId();
            var rawConfig = _preferences.Get(UserSettingsKeys.NeverShowAgentCommunityVerificationAgain, string.Empty);
            var config = string.IsNullOrEmpty(rawConfig) ? null : JsonConvert.DeserializeObject<DontAskAgainConfig>(rawConfig);
            if (config is null)
            {
                config = new DontAskAgainConfig();
                config.Values.Add(loggedUserId, step1.IsDontAskAgainChecked);
            }
            else
            {
                if (config.Values.ContainsKey(loggedUserId) is false)
                    config.Values.Add(loggedUserId, step1.IsDontAskAgainChecked);
                else
                    config.Values[loggedUserId] = step1.IsDontAskAgainChecked;
            }
                        
            rawConfig = JsonConvert.SerializeObject(config);
            _preferences.Set(UserSettingsKeys.NeverShowAgentCommunityVerificationAgain, rawConfig);
        }

        #endregion
    }

    internal class DontAskAgainConfig
    {
        public DontAskAgainConfig()
        {
            Values = new Dictionary<string, bool>();
        }

        public Dictionary<string, bool> Values { get; }
    }
}