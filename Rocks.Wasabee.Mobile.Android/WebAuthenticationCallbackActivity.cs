using Android.App;
using Android.Content;
using Android.Content.PM;
using Xamarin.Essentials;

namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView},
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable},
        DataScheme = "com.googleusercontent.apps.269534461245-ltpks4ofjh9epvida0ct965829i4cfsi" )]
    public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
    {
    }
}