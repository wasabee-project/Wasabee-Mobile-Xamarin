﻿using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IVersionTracking _versionTracking;
        private readonly IPermissions _permissions;
        private readonly IUserDialogs _userDialogs;
        private readonly IPreferences _preferences;
        private readonly IMvxMessenger _messenger;
        private readonly ISecureStorage _secureStorage;
        private readonly IFirebaseService _firebaseService;
        private readonly ICrossFirebaseMessagingService _crossFirebaseMessagingService;

        private int _tapCount = 0;
        private bool _devModeActivated = false;

        public SettingsViewModel(IVersionTracking versionTracking, IPermissions permissions, IUserDialogs userDialogs,
            IPreferences preferences, IMvxMessenger messenger, ISecureStorage secureStorage, IFirebaseService firebaseService,
            ICrossFirebaseMessagingService crossFirebaseMessagingService)
        {
            _versionTracking = versionTracking;
            _permissions = permissions;
            _userDialogs = userDialogs;
            _preferences = preferences;
            _messenger = messenger;
            _secureStorage = secureStorage;
            _firebaseService = firebaseService;
            _crossFirebaseMessagingService = crossFirebaseMessagingService;
        }

        public override Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            _devModeActivated = _preferences.Get(UserSettingsKeys.DevModeActivated, false);

            Version = _versionTracking.CurrentVersion;

            var analyticsSetting = _preferences.Get(UserSettingsKeys.AnalyticsEnabled, false);
            SetProperty(ref _isAnonymousAnalyticsEnabled, analyticsSetting);

            var showAgentsFromAnyTeamSetting = _preferences.Get(UserSettingsKeys.ShowAgentsFromAnyTeam, false);
            SetProperty(ref _showAgentsFromAnyTeam, showAgentsFromAnyTeamSetting);

            var showDebugToastsSetting = _preferences.Get(UserSettingsKeys.ShowDebugToasts, false);
            SetProperty(ref _showDebugToasts, showDebugToastsSetting);

            return base.Initialize();
        }

        #region Properties

        public string Version { get; set; } = string.Empty;

        private bool _isAnonymousAnalyticsEnabled;
        public bool IsAnonymousAnalyticsEnabled
        {
            get => _isAnonymousAnalyticsEnabled;
            set => ToggleAnalyticsCommand.Execute(value);
        }

        private bool _showAgentsFromAnyTeam;
        public bool ShowAgentsFromAnyTeam
        {
            get => _showAgentsFromAnyTeam;
            set => ToggleAgentsFromAnyTeamCommand.Execute(value);
        }

        private bool _showDebugToasts;
        public bool ShowDebugToasts
        {
            get => _showDebugToasts;
            set => ToggleShowDebugToastsCommand.Execute(value);
        }

        #endregion

        #region Commands

        public IMvxCommand OpenApplicationSettingsCommand => new MvxCommand(OpenApplicationSettingsExecuted);
        private void OpenApplicationSettingsExecuted()
        {
            LoggingService.Trace("Executing SettingsViewModel.OpenApplicationSettingsCommand");

            if (IsBusy) return;
            IsBusy = true;

            AppInfo.ShowSettingsUI();

            IsBusy = false;
        }

        public IMvxCommand OpenWasabeeWebpageCommand => new MvxCommand(OpenWasabeeWebpageExecuted);
        private async void OpenWasabeeWebpageExecuted()
        {
            LoggingService.Trace("Executing SettingsViewModel.OpenWasabeeWebpageCommand");

            if (IsBusy) return;
            IsBusy = true;

            await Launcher.OpenAsync("https://cdn2.wasabee.rocks");

            IsBusy = false;
        }

        public IMvxCommand SendLogsCommand => new MvxCommand(async () => await SendLogsExecuted());
        private async Task SendLogsExecuted()
        {
            LoggingService.Trace("Executing SettingsViewModel.SendLogsCommand");

            if (IsBusy) return;
            IsBusy = true;

            // Check permissions
            var statusStorage = await _permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (statusStorage != PermissionStatus.Granted)
            {
                statusStorage = await _permissions.RequestAsync<Permissions.StorageWrite>();
                if (statusStorage != PermissionStatus.Granted)
                {
                    _userDialogs.Alert(Strings.Settings_Warning_StoragePermissionsNeeded);
                    IsBusy = false;
                    return;
                }

                LoggingService.Info("User has granted write storage permissions");
            }

            statusStorage = await _permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (statusStorage != PermissionStatus.Granted)
            {
                statusStorage = await _permissions.RequestAsync<Permissions.StorageRead>();
                if (statusStorage != PermissionStatus.Granted)
                {
                    _userDialogs.Alert(Strings.Settings_Warning_StoragePermissionsNeeded);
                    IsBusy = false;
                    return;
                }

                LoggingService.Info("User has granted read storage permissions");
            }

            try
            {
                var zip = await CreateZipFile();

                IsBusy = false;

                if (string.IsNullOrWhiteSpace(zip))
                {
                    _userDialogs.Alert(Strings.Settings_Warning_NoLogs);
                    return;
                }

                var hasSent = false;
                if (IsAnonymousAnalyticsEnabled)
                {
                    var report = await Crashes.GetLastSessionCrashReportAsync();
                    if (report != null)
                    {
                        Crashes.TrackError(new Exception(report.StackTrace), null, ErrorAttachmentLog.AttachmentWithBinary(File.ReadAllBytes(zip), "WasabeeLogs.zip", "application/zip"));
                        _userDialogs.Toast(Strings.Settings_Toast_LogsAutomaticallySent);

                        hasSent = true;
                    }
                }

                if (!hasSent)
                {
                    await _userDialogs.AlertAsync(Strings.Settings_Warning_SendFileOverTelegram);
                    await Share.RequestAsync(new ShareFileRequest(zip, new ShareFile(zip)));
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing SettingsViewModel.SendLogsCommand");
            }
        }

        public IMvxCommand<bool> ToggleAnalyticsCommand => new MvxCommand<bool>(async value => await ToggleAnalyticsExecuted(value));
        private async Task ToggleAnalyticsExecuted(bool value)
        {
            LoggingService.Trace($"Executing SettingsViewModel.ToggleAnalyticsCommand({value})");

            if (value)
            {
                SetProperty(ref _isAnonymousAnalyticsEnabled, true);
                _preferences.Set(UserSettingsKeys.AnalyticsEnabled, true);

                await Crashes.SetEnabledAsync(true);
                await Analytics.SetEnabledAsync(true);

                LoggingService.Info("User activated analytics");
            }
            else
            {
                LoggingService.Info("User disabled analytics");

                SetProperty(ref _isAnonymousAnalyticsEnabled, false);
                _preferences.Remove(UserSettingsKeys.AnalyticsEnabled);

                await Crashes.SetEnabledAsync(false);
                await Analytics.SetEnabledAsync(false);
            }
        }

        public IMvxCommand VersionTappedCommand => new MvxCommand(VersionTappedExecuted);
        private void VersionTappedExecuted()
        {
            if (_devModeActivated)
                return;

            _tapCount++;
            if (_tapCount < 5)
                return;

            _messenger.Publish(new MessageFrom<SettingsViewModel>(this));
            _userDialogs.Toast("Dev mode activated");
        }

        public IMvxCommand RefreshFcmTokenCommand => new MvxCommand(async () => await RefreshFcmTokenExecuted());
        private async Task RefreshFcmTokenExecuted()
        {
            if (IsBusy)
                return;

            LoggingService.Trace("Executing SettingsViewModel.RefreshFcmTokenCommand");

            IsBusy = true;

            var token = await _secureStorage.GetAsync(SecureStorageConstants.FcmToken);
            if (string.IsNullOrWhiteSpace(token))
                token = _firebaseService.GetFcmToken();

            var result = await _crossFirebaseMessagingService.SendRegistrationToServer(token);
            _userDialogs.Toast(result ? Strings.Settings_Toast_FcmtokenUpdated : Strings.Global_ErrorOccuredPleaseRetry);

            LoggingService.Trace($"Result for SettingsViewModel.RefreshFcmTokenCommand : {result}");

            IsBusy = false;
        }

        public IMvxCommand SwitchThemeCommand => new MvxCommand(SwitchThemeExecuted);
        private void SwitchThemeExecuted()
        {
            var themeRequested = CoreApp.AppTheme == Theme.Light ? Theme.Dark : Theme.Light;
            _messenger.Publish(new ChangeThemeMessage(this, themeRequested));
        }

        public IMvxCommand<bool> ToggleAgentsFromAnyTeamCommand => new MvxCommand<bool>(ToggleAgentsFromAnyTeamExecuted);
        private void ToggleAgentsFromAnyTeamExecuted(bool value)
        {
            LoggingService.Trace($"Executing SettingsViewModel.ToggleAgentsFromAnyTeamCommand({value})");

            if (value)
            {
                SetProperty(ref _showAgentsFromAnyTeam, true);
                _preferences.Set(UserSettingsKeys.ShowAgentsFromAnyTeam, true);
            }
            else
            {
                SetProperty(ref _showAgentsFromAnyTeam, false);
                _preferences.Remove(UserSettingsKeys.ShowAgentsFromAnyTeam);
            }
        }

        public IMvxCommand<bool> ToggleShowDebugToastsCommand => new MvxCommand<bool>(ToggleShowDebugToastsExecuted);
        private void ToggleShowDebugToastsExecuted(bool value)
        {
            LoggingService.Trace($"Executing SettingsViewModel.ToggleShowDebugToastsCommand({value})");

            if (value)
            {
                SetProperty(ref _showDebugToasts, true);
                _preferences.Set(UserSettingsKeys.ShowDebugToasts, true);
            }
            else
            {
                SetProperty(ref _showDebugToasts, false);
                _preferences.Remove(UserSettingsKeys.ShowDebugToasts);
            }
        }

        #endregion

        #region Private methods

        private bool QuickZip(string directoryToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (File.Exists(destinationZipFullPath))
                    File.Delete(destinationZipFullPath);
                if (!Directory.Exists(directoryToZip))
                    return false;
                else
                {
                    System.IO.Compression.ZipFile.CreateFromDirectory(directoryToZip, destinationZipFullPath, System.IO.Compression.CompressionLevel.Optimal, true);
                    return File.Exists(destinationZipFullPath);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error when zipping logs");
                return false;
            }
        }

        private async Task<string> CreateZipFile()
        {
            var zipFilename = string.Empty;
            if (NLog.LogManager.IsLoggingEnabled())
            {
                var folder = Device.RuntimePlatform switch
                {
                    Device.iOS => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library"),
                    Device.Android => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                    _ => throw new Exception("Could not show log: Platform undefined.")
                };

                var result = await _userDialogs.ConfirmAsync(
                    Strings.Settings_Warning_SendLogsWithDatabase,
                    Strings.Settings_Warning_SendLogsWithDatabase_Title,
                    Strings.Settings_Warning_SendLogs_WithDb,
                    Strings.Settings_Warning_SendLogs_NoDb);

                //Delete old zipfiles (housekeeping)
                var logFolder = Path.Combine(folder, "logs");
                try
                {
                    foreach (var fileName in Directory.GetFiles(logFolder, "*.zip"))
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error deleting old zip files");
                }

                if (Directory.Exists(logFolder))
                {
                    var filesCount = Directory.GetFiles(logFolder, "*.csv").Length;
                    if (filesCount > 0)
                    {
                        var destinationPath = Path.Combine(logFolder, "database");

                        if (result)
                        {
                            // Include local DB copy
                            try
                            {
                                var fileSystem = Mvx.IoCProvider.Resolve<IFileSystem>();
                                var databaseFullPathName = Path.Combine(fileSystem.AppDataDirectory, BaseDatabase.Name);

                                if (Directory.Exists(destinationPath) is false)
                                    Directory.CreateDirectory(destinationPath);

                                var destinationFullPathName = Path.Combine(destinationPath, BaseDatabase.Name);

                                File.Copy(databaseFullPathName, destinationFullPathName, true);
                            }
                            catch (Exception e)
                            {
                                LoggingService.Error(e, "Error importing local DB copy");
                            }
                        }
                        else
                        {
                            try
                            {
                                if (Directory.Exists(destinationPath))
                                    Directory.Delete(destinationPath, true);
                            }
                            catch (Exception e)
                            {
                                LoggingService.Error(e, "Error deleting old local DB copy");
                            }
                        }

                        zipFilename = $"{folder}/{DateTime.Now:yyyyMMdd-HHmmss}.zip";
                        if (!QuickZip(logFolder, zipFilename))
                            zipFilename = string.Empty;
                    }
                }
            }

            return zipFilename;
        }

        #endregion
    }
}