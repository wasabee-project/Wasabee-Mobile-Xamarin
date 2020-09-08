using Acr.UserDialogs;
using MvvmCross.Commands;
using System;
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

        public SettingsViewModel(IVersionTracking versionTracking, IPermissions permissions, IUserDialogs userDialogs)
        {
            _versionTracking = versionTracking;
            _permissions = permissions;
            _userDialogs = userDialogs;
        }

        public override Task Initialize()
        {
            Version = _versionTracking.CurrentVersion;

            return base.Initialize();
        }

        #region Properties

        public string Version { get; set; }

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

            await Launcher.OpenAsync("https://enl.rocks/-dEHQ");

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

        public IMvxAsyncCommand SendLogsCommand => new MvxAsyncCommand(SendLogsExecuted);
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
                else
                {
                    LoggingService.Info("User has granted storage permissions");
                }
            }

            var zip = CreateZipFile();

            IsBusy = false;

            await _userDialogs.AlertAsync("Please send the file to @fisher01 on Telegram");
            await Share.RequestAsync(new ShareFileRequest(new ShareFile(zip)));
        }

        #endregion

        #region Private methods

        bool QuickZip(string directoryToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (System.IO.File.Exists(destinationZipFullPath))
                    System.IO.File.Delete(destinationZipFullPath);
                if (!System.IO.Directory.Exists(directoryToZip))
                    return false;
                else
                {
                    System.IO.Compression.ZipFile.CreateFromDirectory(directoryToZip, destinationZipFullPath, System.IO.Compression.CompressionLevel.Optimal, true);
                    return System.IO.File.Exists(destinationZipFullPath);
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
                    Device.iOS => System.IO.Path.Combine(FileSystem.AppDataDirectory, "..", "Library"),
                    Device.Android => System.IO.Path.Combine(FileSystem.AppDataDirectory),
                    _ => throw new Exception("Could not show log: Platform undefined.")
                };

                //Delete old zipfiles (housekeeping)
                try
                {
                    foreach (string fileName in System.IO.Directory.GetFiles(folder, "*.zip"))
                    {
                        System.IO.File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error("Error deleting old zip files", ex);
                }

                string logFolder = System.IO.Path.Combine(folder, "logs");
                if (System.IO.Directory.Exists(logFolder))
                {
                    zipFilename = $"{folder}/{DateTime.Now:yyyyMMdd-HHmmss}.zip";
                    int filesCount = System.IO.Directory.GetFiles(logFolder, "*.csv").Length;
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