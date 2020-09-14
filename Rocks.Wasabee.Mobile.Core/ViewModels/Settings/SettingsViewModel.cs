using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross.Commands;
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

        public SettingsViewModel(IVersionTracking versionTracking, IPermissions permissions, IUserDialogs userDialogs,
            IPreferences preferences)
        {
            _versionTracking = versionTracking;
            _permissions = permissions;
            _userDialogs = userDialogs;
            _preferences = preferences;
        }

        public override Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);

            Version = _versionTracking.CurrentVersion;

            var analyticsSetting = _preferences.Get(UserSettingsKeys.AnalyticsEnabled, false);
            SetProperty(ref _isAnonymousAnalyticsEnabled, analyticsSetting);

            return base.Initialize();
        }

        #region Properties

        public string Version { get; set; }

        private bool _isAnonymousAnalyticsEnabled;
        public bool IsAnonymousAnalyticsEnabled
        {
            get => _isAnonymousAnalyticsEnabled;
            set => ToggleAnalyticsCommand.Execute(value);
        }

        #endregion

        #region Commands

        public IMvxCommand OpenApplicationSettingsCommand => new MvxCommand(OpenApplicationSettingsExecuted);
        private void OpenApplicationSettingsExecuted()
        {
            if (IsBusy) return;
            IsBusy = true;

            AppInfo.ShowSettingsUI();

            IsBusy = false;
        }

        public IMvxCommand OpenWasabeeTelegramChatCommand => new MvxCommand(OpenWasabeeTelegramChatExecuted);
        private async void OpenWasabeeTelegramChatExecuted()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Launcher.OpenAsync("tg://join?invite=FSaGrFWGdrf87xdKQXWsPw");

            IsBusy = false;
        }

        public IMvxCommand OpenWasabeeWebpageCommand => new MvxCommand(OpenWasabeeWebpageExecuted);
        private async void OpenWasabeeWebpageExecuted()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Launcher.OpenAsync("https://cdn2.wasabee.rocks");

            IsBusy = false;
        }

        public IMvxCommand SendLogsCommand => new MvxCommand(async () => await SendLogsExecuted());
        private async Task SendLogsExecuted()
        {
            if (IsBusy) return;
            IsBusy = true;

            // Check permissions
            var statusStorage = await _permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (statusStorage != PermissionStatus.Granted)
            {
                statusStorage = await _permissions.RequestAsync<Permissions.StorageWrite>();
                if (statusStorage != PermissionStatus.Granted)
                {
                    _userDialogs.Alert("Storage permissions are required !");
                    IsBusy = false;
                    return;
                }

                LoggingService.Info("User has granted storage permissions");
            }

            var zip = CreateZipFile();

            IsBusy = false;

            var hasSent = false;
            if (IsAnonymousAnalyticsEnabled)
            {
                var report = await Crashes.GetLastSessionCrashReportAsync();
                if (report != null)
                {
                    Crashes.TrackError(new Exception(report.StackTrace), null, ErrorAttachmentLog.AttachmentWithBinary(File.ReadAllBytes(zip), "WasabeeLogs.zip", "application/zip"));
                    _userDialogs.Toast("Data is beeing sent automatically");

                    hasSent = true;
                }
            }

            if (!hasSent)
            {
                await _userDialogs.AlertAsync("Please send the file to @fisher01 on Telegram");
                await Share.RequestAsync(new ShareFileRequest(new ShareFile(zip)));
            }
        }

        public IMvxCommand<bool> ToggleAnalyticsCommand => new MvxCommand<bool>(async value => await ToggleAnalyticsExecuted(value));
        private async Task ToggleAnalyticsExecuted(bool value)
        {
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

        #endregion

        #region Private methods

        bool QuickZip(string directoryToZip, string destinationZipFullPath)
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
                LoggingService.Error("Error when zipping logs", e);
                return false;
            }
        }

        private string CreateZipFile()
        {
            string zipFilename;
            if (NLog.LogManager.IsLoggingEnabled())
            {
                string folder = Device.RuntimePlatform switch
                {
                    Device.iOS => Path.Combine(FileSystem.AppDataDirectory, "..", "Library"),
                    Device.Android => Path.Combine(FileSystem.AppDataDirectory),
                    _ => throw new Exception("Could not show log: Platform undefined.")
                };

                //Delete old zipfiles (housekeeping)
                try
                {
                    foreach (string fileName in Directory.GetFiles(folder, "*.zip"))
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error("Error deleting old zip files", ex);
                }

                string logFolder = Path.Combine(folder, "logs");
                if (Directory.Exists(logFolder))
                {
                    zipFilename = $"{folder}/{DateTime.Now:yyyyMMdd-HHmmss}.zip";
                    int filesCount = Directory.GetFiles(logFolder, "*.csv").Length;
                    if (filesCount > 0)
                    {
                        if (!QuickZip(logFolder, zipFilename))
                            zipFilename = string.Empty;
                    }
                    else
                        zipFilename = string.Empty;
                }
                else
                    zipFilename = string.Empty;
            }
            else
                zipFilename = string.Empty;

            return zipFilename;
        }

        #endregion
    }
}