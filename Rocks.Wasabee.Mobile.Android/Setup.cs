using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core;

namespace Rocks.Wasabee.Mobile.Droid
{
    public class Setup : MvxFormsAndroidSetup<CoreApp, App>
    {
        protected override IMvxApplication CreateApp()
        {
            UserDialogs.Init(() => Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>().Activity);

            Mvx.IoCProvider.RegisterSingleton(UserDialogs.Instance);

            return new CoreApp();
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