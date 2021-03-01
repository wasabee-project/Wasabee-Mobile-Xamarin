﻿using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.IoC;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.ViewModels;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Services;
using Rocks.Wasabee.Mobile.iOS.Infra.Firebase;

namespace Rocks.Wasabee.Mobile.iOS
{
    public class Setup : MvxFormsIosSetup<CoreApp, App>
    {
        private Xamarin.Forms.Application _formsApplication;
        public override Xamarin.Forms.Application FormsApplication
        {
            get
            {
                if (!Xamarin.Forms.Forms.IsInitialized)
                {
                    Xamarin.Forms.Forms.SetFlags("SwipeView_Experimental");
                    Xamarin.Forms.Forms.Init();
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

            return new CoreApp();
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