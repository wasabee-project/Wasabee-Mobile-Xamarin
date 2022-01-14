using Acr.UserDialogs;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.IoC;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rg.Plugins.Popup.Services;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Infra.LocalNotification;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Services;
using Rocks.Wasabee.Mobile.iOS.Infra.Firebase;
using Rocks.Wasabee.Mobile.iOS.Infra.LocalNotification;
using Rocks.Wasabee.Mobile.iOS.Services.Geolocation;
using UIKit;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.Platform.iOS;

#nullable enable
namespace Rocks.Wasabee.Mobile.iOS
{
    public class Setup : MvxFormsIosSetup<CoreApp, App>
    {
        private ILoggingService? _loggingService;
        private LocationManager? _locationManager;

        private MvxSubscriptionToken? _tokenLocation;
        private MvxSubscriptionToken? _tokenFcm;

        private Xamarin.Forms.Application? _formsApplication;
        public override Xamarin.Forms.Application FormsApplication
        {
            get
            {
                if (!Xamarin.Forms.Forms.IsInitialized)
                {
                    Xamarin.Forms.Forms.SetFlags("SwipeView_Experimental");
                    Xamarin.Forms.Forms.Init();

                    UINavigationBar.Appearance.TintColor = Xamarin.Forms.Color.FromHex("#3BA345").ToUIColor(); // Green
                }
                
                _formsApplication ??= CreateFormsApplication();

                if (Xamarin.Forms.Application.Current != _formsApplication)
                    Xamarin.Forms.Application.Current = _formsApplication;

                return _formsApplication;
            }
        }

        protected override IMvxApplication CreateApp(IMvxIoCProvider iocProvider)
        {
            Mvx.IoCProvider.RegisterType<IPreferences, PreferencesImplementation>();

            // this force Xamarin.Forms to initialize
            var app = this.FormsApplication;

            SetupAppSettings();
            
            Mvx.IoCProvider.RegisterType<Core.Infra.HttpClientFactory.IFactory, iOS.Infra.HttpClientFactory.Factory>();

            Mvx.IoCProvider.RegisterSingleton(UserDialogs.Instance);
            Mvx.IoCProvider.RegisterType<IFirebaseService, FirebaseService>();

            Mvx.IoCProvider.RegisterSingleton(PopupNavigation.Instance);
            Mvx.IoCProvider.RegisterType<IDialogNavigationService, DialogNavigationService>();
            Mvx.IoCProvider.RegisterType<ILocalNotificationService, LocalNotificationService>();

            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());

            SetupGeolocationTrackingMessage();
            SetupFcmServiceMessage();

            return new CoreApp();
        }

        private void SetupGeolocationTrackingMessage()
        {
            _tokenLocation ??= Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<LiveGeolocationTrackingMessage>(async msg =>
            {
                _loggingService ??= Mvx.IoCProvider.Resolve<ILoggingService>();

                _loggingService.Trace("MvxMessengerHub - LiveGeolocationTrackingMessage received" +
                                      (msg.Action == Action.Start ? 
                                          "Start LocationManager" : 
                                          "Stop LocationManager"));

                if (msg.Action == Action.Start)
                {
                    _locationManager ??= new LocationManager();
                    await _locationManager.StartLocationUpdates();
                }
                else
                {
                    if (_locationManager is null)
                        return;

                    _locationManager.StopLocationUpdates();
                }
            });
        }

        private void SetupFcmServiceMessage()
        {
            _tokenFcm ??= Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<UserLoggedInMessage>(msg =>
            {
                MessagingService.Instance.Initialize();
            });
        }

        private static void SetupAppSettings()
        {
#if DEBUG
            Mvx.IoCProvider.RegisterSingleton<IAppSettings>(new DevAppSettings());
#else
            Mvx.IoCProvider.RegisterSingleton<IAppSettings>(new ProdAppSettings());
#endif
            Mvx.IoCProvider.Resolve<IAppSettings>().ClientId = OAuthClient.Id;
            Mvx.IoCProvider.Resolve<IAppSettings>().BaseRedirectUrl = OAuthClient.Redirect;
            Mvx.IoCProvider.Resolve<IAppSettings>().AppCenterKey = AppCenterKeys.Value;
        }
        
        protected override ILoggerProvider? CreateLogProvider()
        {
            return null;
        }

        protected override ILoggerFactory? CreateLogFactory()
        {
            return null;
        }
    }
}
#nullable disable