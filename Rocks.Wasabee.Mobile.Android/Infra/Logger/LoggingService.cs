using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;
using NLog;
using NLog.Config;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Droid.Infra.Logger
{
    public class LoggingService : ILoggingService
    {
        private readonly IPreferences _preferences;

        public LoggingService(IPreferences preferences)
        {
            _preferences = preferences;
        }

        private LogFactory? _logFactory;
        private LogFactory LogFactory
        {
            get
            {
                if (_logFactory != null)
                    return _logFactory;

                _logFactory = new LogFactory() /*{ Configuration = new XmlLoggingConfiguration("Assets/NLog.config") }*/;

                return _logFactory;
            }
        }

        private ILogger? _logger;
        private ILogger Logger => _logger ?? (_logger = LogFactory.GetLogger("WasabeeLogger"));

        private bool IsAnalyticsEnabled => _preferences.Get(UserSettingsKeys.AnalyticsEnabled, false);

        #region Error

        public void Error(string message) => Logger.Error(message);

        public void Error(Exception e, string message)
        {
            if (IsAnalyticsEnabled)
            {
                var data = new Dictionary<string, string>() { { "message", message } };
                Crashes.TrackError(e, data);
            }

            Logger.Error(e, message);
        }

        public void Error(string format, params object[] args) => Logger.Error(format, args);
        public void Error(Exception e, string format, params object[] args)
        {
            if (IsAnalyticsEnabled)
            {
                var data = new Dictionary<string, string>();
                for (var i = 0; i < args.Length; i++)
                {
                    var o = args[i];
                    data.Add($"args[{i}]", o.ToString());
                }

                Crashes.TrackError(e, data);
            }

            Logger.Error(e, format, args);
        }

        #endregion

        #region Fatal

        public void Fatal(string message) => Logger.Fatal(message);
        public void Fatal(string format, params object[] args) => Logger.Fatal(format, args);

        public void Fatal(Exception e, string message)
        {
            if (IsAnalyticsEnabled)
            {
                var data = new Dictionary<string, string>() { { "message", message } };
                Crashes.TrackError(e, data);
            }

            Logger.Fatal(e, message);
        }

        public void Fatal(Exception e, string format, params object[] args)
        {
            if (IsAnalyticsEnabled)
            {
                var data = new Dictionary<string, string>();
                for (var i = 0; i < args.Length; i++)
                {
                    var o = args[i];
                    data.Add($"args[{i}]", o.ToString());
                }

                Crashes.TrackError(e, data);
            }

            Logger.Fatal(e, format, args);
        }

        #endregion

        #region Debug

        public void Debug(string message) => Logger.Debug(message);
        public void Debug(string format, params object[] args) => Logger.Debug(format, args);

        #endregion

        #region Info

        public void Info(string message) => Logger.Info(message);
        public void Info(string message, params object[] args) => Logger.Info(message, args);

        #endregion

        #region Trace

        public void Trace(string message) => Logger.Trace(message);
        public void Trace(string format, params object[] args) => Logger.Trace(format, args);

        #endregion

        #region Warn

        public void Warn(string message) => Logger.Warn(message);
        public void Warn(string format, params object[] args) => Logger.Warn(format, args);

        #endregion
    }
}

