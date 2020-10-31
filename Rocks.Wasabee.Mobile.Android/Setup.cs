using Acr.UserDialogs;
using Android.Runtime;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.ViewModels;
using Plugin.CurrentActivity;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Droid.Infra.Firebase;

namespace Rocks.Wasabee.Mobile.Droid
{
    public class Setup : MvxFormsAndroidSetup<CoreApp, App>
    {
        protected override IMvxApplication CreateApp()
        {
            UserDialogs.Init(() => CrossCurrentActivity.Current.Activity);

            Mvx.IoCProvider.RegisterSingleton(UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(typeof(IFirebaseAnalyticsService), () => new FirebaseAnalyticsService(Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>()));

            AndroidEnvironment.UnhandledExceptionRaiser += UnhandledExceptionHandler;

            return new CoreApp();
        }

        private void UnhandledExceptionHandler(object sender, RaiseThrowableEventArgs e)
        {
            Mvx.IoCProvider.Resolve<ILoggingService>().Fatal(e.Exception, "Fatal error occured");
            e.Handled = false;

            throw e.Exception;
        }

        protected override void InitializeFirstChance()
        {
            base.InitializeFirstChance();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            return base.CreateViewPresenter();
        }
    }
}