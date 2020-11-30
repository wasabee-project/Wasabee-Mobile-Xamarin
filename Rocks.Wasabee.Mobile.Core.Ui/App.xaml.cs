using MvvmCross.Forms.Core;
using Rocks.Wasabee.Mobile.Core.Ui.Themes;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui
{
    public partial class App : MvxFormsApplication
    {
        public static App Instance { get; private set; }

        public App()
        {
            Instance = this;
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
