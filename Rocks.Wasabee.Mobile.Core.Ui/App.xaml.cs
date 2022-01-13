using MvvmCross;
using MvvmCross.Forms.Core;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.Ui.Themes;
using System.Globalization;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui
{
    public partial class App : MvxFormsApplication
    {
        public static App Instance { get; private set; }

        private readonly MvxSubscriptionToken _token;

        public App()
        {
            Instance = this;

            var preferences = Mvx.IoCProvider.Resolve<IPreferences>();
            var cultureSetting = preferences.Get(UserSettingsKeys.CurrentCulture, string.Empty);
            if (string.IsNullOrEmpty(cultureSetting) is false)
            {
                try 
                {
                    var culture = CultureInfo.GetCultureInfo(cultureSetting);
                    CultureInfo.CurrentUICulture = culture;
                }
                catch
                {
                    // Nothing to do
                }
            }

            InitializeComponent();

            Application.Current.Resources = new LightTheme();

            Plugin.Iconize.Iconize.With(new Plugin.Iconize.Fonts.MaterialDesignIconsModule());

            /*Application.Current.RequestedThemeChanged += (sender, args) =>
            {
                AppTheme = args.RequestedTheme switch
                {
                    OSAppTheme.Dark => Theme.Dark,
                    OSAppTheme.Light => Theme.Light,
                    _ => Theme.Light
                };
            };*/
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}
