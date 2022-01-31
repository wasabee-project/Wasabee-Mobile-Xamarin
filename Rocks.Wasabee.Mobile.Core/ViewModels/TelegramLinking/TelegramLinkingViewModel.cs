using Microsoft.AppCenter.Analytics;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking
{
    public class TelegramLinkingNavigationParameter
    {
        public UserModel User { get; }
        public bool ComingFromLogin { get; }

        public TelegramLinkingNavigationParameter(UserModel user, bool comingFromLogin = true)
        {
            User = user;
            ComingFromLogin = comingFromLogin;
        }
    }

    public class TelegramLinkingCloseResult
    {
        public bool IsSuccess { get; }

        public TelegramLinkingCloseResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }

    public class TelegramLinkingViewModel : BaseViewModel, IMvxViewModel<TelegramLinkingNavigationParameter, TelegramLinkingCloseResult>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IPreferences _preferences;
        private readonly IUserSettingsService _userSettingsService;
        private readonly UsersDatabase _usersDatabase;

        private TelegramLinkingNavigationParameter? _parameter;

        public TelegramLinkingViewModel(IMvxNavigationService navigationService, IPreferences preferences,
            IUserSettingsService userSettingsService, UsersDatabase usersDatabase)
        {
            _navigationService = navigationService;
            _preferences = preferences;
            _userSettingsService = userSettingsService;
            _usersDatabase = usersDatabase;
        }

        public void Prepare(TelegramLinkingNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        public override Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to TelegramLinkingViewModel");

            if (_parameter?.User.Telegram is not null && string.IsNullOrEmpty(_parameter.User.Telegram.AuthToken) is false)
            {
                // Skip step 2 because token was already sent
                Steps = new MvxObservableCollection<BaseViewModel>()
                {
                    new TelegramLinkingStep1SubViewModel(this, _parameter.ComingFromLogin),
                    new TelegramLinkingStep3SubViewModel(this)
                };
            }
            else
            {
                Steps = new MvxObservableCollection<BaseViewModel>()
                {
                    new TelegramLinkingStep1SubViewModel(this, _parameter?.ComingFromLogin ?? true),
                    new TelegramLinkingStep2SubViewModel(this),
                    new TelegramLinkingStep3SubViewModel(this)
                };
            }
            
            var server = _preferences.Get(UserSettingsKeys.CurrentServer, string.Empty);
            if (server == WasabeeServer.US.ToString())
            {
                BotUsername = "PhDevBot";
                BotName = "@WasabeeUS_bot";
            }
            else if (server == WasabeeServer.EU.ToString())
            {
                BotUsername = "WasabeeEU_bot";
                BotName = $"@{BotUsername}";
            }
            else if (server == WasabeeServer.APAC.ToString())
            {
                BotUsername = "WasabeeAP_bot";
                BotName = $"@{BotUsername}";
            }

            return base.Initialize();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (Steps is null || Steps.IsNullOrEmpty())
                return;
            
            foreach (var step in Steps)
                step.ViewAppeared();
        }

        #region Properties

        public MvxObservableCollection<BaseViewModel>? Steps { get; set; }

        public BaseViewModel? CurrentStep { get; set; }

        public string BotName { get; set; } = string.Empty;
        public string BotUsername { get; private set; } = string.Empty;

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

            if (CurrentStep is AgentVerificationStep3SubViewModel { IsVerified: true })
            {
                Analytics.TrackEvent(AnalyticsConstants.TelegramLinked);
                CloseCompletionSource?.SetResult(new TelegramLinkingCloseResult(isSuccess: true));
            }
            else
                CloseCompletionSource?.SetResult(new TelegramLinkingCloseResult(isSuccess: false));

            _navigationService.Close(this);
        }

        #endregion

        #region Private methods

        private void SaveDontAskAgainSetting()
        {
            if (CurrentStep is not AgentVerificationStep1SubViewModel step1 || step1.IsDontAskAgainVisible is false) 
                return;
            
            var loggedUserId = _userSettingsService.GetLoggedUserGoogleId();
            var rawConfig = _preferences.Get(UserSettingsKeys.NeverShowTelegramLinkingAgain, string.Empty);
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
            _preferences.Set(UserSettingsKeys.NeverShowTelegramLinkingAgain, rawConfig);
        }

        #endregion

        #region IMvxViewModelResult<TResult> implementation

        public TaskCompletionSource<object?>? CloseCompletionSource { get; set; } = new();

        #endregion
    }
}