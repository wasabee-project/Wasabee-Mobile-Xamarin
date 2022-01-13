using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification.SubViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels.TelegramLinking.SubViewModels;
using System;
using System.Globalization;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core
{
    public class CoreApp : MvxApplication
    {
        public static Theme AppTheme { get; set; } = Theme.Light;

        public override async void Initialize()
        {
            CreatableTypes()
                .EndingWith("ViewModel")
                .Except(typeof(BaseViewModel))
                .Except(typeof(AgentVerificationStep1SubViewModel))
                .Except(typeof(AgentVerificationStep2SubViewModel))
                .Except(typeof(AgentVerificationStep3SubViewModel))
                .Except(typeof(TelegramLinkingStep1SubViewModel))
                .Except(typeof(TelegramLinkingStep2SubViewModel))
                .Except(typeof(TelegramLinkingStep3SubViewModel))
                .AsTypes()
                .RegisterAsDynamic();

            Bootstrapper.SetupCrossPlugins();
            Bootstrapper.SetupCrossConcerns();
            Bootstrapper.SetupEnvironment();
            Bootstrapper.SetupDatabases();
            Bootstrapper.SetupServices();

            var preferences = Mvx.IoCProvider.Resolve<IPreferences>();
            var versionTracking = Mvx.IoCProvider.Resolve<IVersionTracking>();

            var cultureSetting = preferences.Get(UserSettingsKeys.CurrentCulture, string.Empty);
            if (string.IsNullOrEmpty(cultureSetting) is false)
            {
                try 
                {
                    var culture = CultureInfo.GetCultureInfo(cultureSetting);
                    CultureInfo.CurrentUICulture = culture;
                }
                catch
                {
                    // Nothing to do
                }
            }
            
            var lastVersion = preferences.Get(UserSettingsKeys.LastLaunchedVersion, versionTracking.CurrentVersion);
            if (Version.TryParse(versionTracking.CurrentVersion, out var currentVersion) && 
                Version.TryParse(lastVersion, out var lastVersionParsed) && currentVersion > lastVersionParsed)
            {
                // App has updated
                preferences.Set(UserSettingsKeys.LastLaunchedVersion, versionTracking.CurrentVersion);
                preferences.Set(UserSettingsKeys.DevModeActivated, false);
            }

#if DEBUG
            preferences.Set(UserSettingsKeys.DevModeActivated, true);
#endif
            AppCenter.Start($"{Xamarin.Forms.Device.RuntimePlatform.ToLowerInvariant()}={Mvx.IoCProvider.Resolve<IAppSettings>().AppCenterKey}",
                typeof(Crashes), typeof(Analytics));

            var analyticsEnabled = preferences.Get(UserSettingsKeys.AnalyticsEnabled, false);
            if (!analyticsEnabled) {
                await Crashes.SetEnabledAsync(false);
                await Analytics.SetEnabledAsync(false);
            }

            Analytics.TrackEvent(AnalyticsConstants.AppStart);
            NLog.LogManager.EnableLogging();

            RegisterAppStart<SplashScreenViewModel>();
        }
    }
}