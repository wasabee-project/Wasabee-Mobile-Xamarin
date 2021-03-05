using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Services;
using Rocks.Wasabee.Mobile.iOS.Infra.Firebase;
using Rocks.Wasabee.Mobile.iOS.Services.Geolocation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Rocks.Wasabee.Mobile.iOS
{
    public class Setup : MvxFormsIosSetup<CoreApp, App>
    {
        private MvxSubscriptionToken _token;
        private LocationManager _locationManager;

        private Xamarin.Forms.Application _formsApplication;
        public override Xamarin.Forms.Application FormsApplication
        {
            get
            {
                if (!Forms.IsInitialized)
                {
                    Forms.SetFlags("SwipeView_Experimental");
                    Forms.Init();

                    UINavigationBar.Appearance.TintColor = Color.FromHex("#3BA345").ToUIColor(); // Green
                }
                if (_formsApplication == null)
                {
                    _formsApplication = CreateFormsApplication();
                }
                if (Xamarin.Forms.Application.Current != _formsApplication)
                {
                    Xamarin.Forms.Application.Current = _formsApplication;
                }
                return _formsApplication;
            }
        }

        protected override IMvxApplication CreateApp()
        {
            SetupAppSettings();

            Mvx.IoCProvider.RegisterSingleton(UserDialogs.Instance);
            Mvx.IoCProvider.RegisterType<IFirebaseService, FirebaseService>();

            Mvx.IoCProvider.RegisterSingleton<IPopupNavigation>(PopupNavigation.Instance);
            Mvx.IoCProvider.RegisterType<IDialogNavigationService, DialogNavigationService>();

            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());

            SetupGeolocationTrackingMessage();

            return new CoreApp();
        }

        private void SetupGeolocationTrackingMessage()
        {
            _token = Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<LiveGeolocationTrackingMessage>(async msg =>
            {
                if (msg.Action == Action.Start)
                {
                    _locationManager ??= new LocationManager();
                    await _locationManager.StartLocationUpdates();
                }
                else
                {
                    if (_locationManager is null)
                        return;

                    await _locationManager.StopLocationUpdates();
                }
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

        protected override void InitializeFirstChance()
        {
            base.InitializeFirstChance();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
        }

        protected override IMvxIosViewPresenter CreateViewPresenter()
        {
            return base.CreateViewPresenter();
        }
    }
}