using Acr.UserDialogs;
using Android.Runtime;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Plugin.CurrentActivity;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Droid.Infra.Firebase;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Droid
{
    public class Setup : MvxFormsAndroidSetup<CoreApp, App>
    {
        private Application _formsApplication;
        public override Application FormsApplication
        {
            get
            {
                if (!Forms.IsInitialized)
                {
                    var activity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>()?.Activity ?? ApplicationContext;
                    var asmb = activity.GetType().Assembly;
                    
                    Forms.SetFlags("SwipeView_Experimental");
                    Forms.Init(activity, null, ExecutableAssembly ?? asmb);
                }
                if (_formsApplication == null)
                {
                    _formsApplication = CreateFormsApplication();
                }
                if (Application.Current != _formsApplication)
                {
                    Application.Current = _formsApplication;
                }
                return _formsApplication;
            }
        }

        protected override IMvxApplication CreateApp()
        {
            SetupAppSettings();
            
            UserDialogs.Init(() => CrossCurrentActivity.Current.Activity);

            Mvx.IoCProvider.RegisterSingleton(UserDialogs.Instance);
            Mvx.IoCProvider.RegisterType<IFirebaseService, FirebaseService>();

            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());

            AndroidEnvironment.UnhandledExceptionRaiser += UnhandledExceptionHandler;

            return new CoreApp();
        }

        private static void SetupAppSettings()
        {
#if DEBUG
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppSettings, DevAppSettings>();
#else
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppSettings, ProdAppSettings>();
#endif
            Mvx.IoCProvider.Resolve<IAppSettings>().ClientId = OAuthClient.Id;
            Mvx.IoCProvider.Resolve<IAppSettings>().BaseRedirectUrl = OAuthClient.Redirect;
            Mvx.IoCProvider.Resolve<IAppSettings>().AppCenterKey = AppCenterKeys.Value;
        }

        private void UnhandledExceptionHandler(object sender, RaiseThrowableEventArgs e)
        {
            Mvx.IoCProvider.Resolve<ILoggingService>().Fatal(e.Exception, "Fatal error occured");
            e.Handled = false;

            throw e.Exception;
        }
    }
}