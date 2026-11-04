using Android.App;
using Android.Content.PM;
using Android.Views;

namespace Rocks.Wasabee.Maui
{
    [Activity(
        //Theme = "@style/Maui.SplashTheme", 
        Theme = "@style/Theme.Splash",
        ResizeableActivity = true,
        MainLauncher = true,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
