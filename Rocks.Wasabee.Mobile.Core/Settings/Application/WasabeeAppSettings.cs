﻿using System;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public abstract class WasabeeAppSettings : IAppSettings
    {
        protected abstract string Environnement { get; set; }

        public string GoogleAuthUrl => "https://accounts.google.com/o/oauth2/v2/auth?scope=email%20profile&response_type=code&" +
                                       $"redirect_uri={RedirectUrl}&client_id={ClientId}";
        public string GoogleTokenUrl { get; } = "https://oauth2.googleapis.com/token";
        public string ClientId { get; set; } = string.Empty;
        public string BaseRedirectUrl { get; set; } = string.Empty;
        public string RedirectUrl => $"{BaseRedirectUrl}:wasabee";

        private WasabeeServer _server = WasabeeServer.Undefined;
        public WasabeeServer Server
        {
            get => _server;
            set
            {
                if (_server == value) return;

                _server = value;
                UpdateWasabeeUrls();
            }
        }

        public string WasabeeBaseUrl { get; private set; } = string.Empty;
        public string WasabeeTokenUrl { get; private set; } = string.Empty;

        public string AppCenterKey { get; set; } = string.Empty;

        private void UpdateWasabeeUrls()
        {
            var server = Server switch
            {
                WasabeeServer.US => "am",
                WasabeeServer.EU => "eu",
                WasabeeServer.APAC => "ap",
                WasabeeServer.Undefined => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(Server))
            };

            WasabeeBaseUrl = $"https://{server}.wasabee.rocks";
            WasabeeTokenUrl = $"{WasabeeBaseUrl}/aptok";
        }
    }
}