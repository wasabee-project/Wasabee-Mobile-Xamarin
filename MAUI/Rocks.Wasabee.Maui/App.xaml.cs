using Rocks.Wasabee.Maui.Themes;

namespace Rocks.Wasabee.Maui
{
    public partial class App : Application
    {
        public static App Instance { get; private set; }

        public App()
        {
            Instance = this;

            InitializeComponent();

            Current?.Resources = new LightTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}