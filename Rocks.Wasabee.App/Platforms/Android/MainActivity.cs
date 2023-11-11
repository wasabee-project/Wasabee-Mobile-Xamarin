using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Rocks.Wasabee.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ResizeableActivity = true,
    WindowSoftInputMode = SoftInput.AdjustPan,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
}