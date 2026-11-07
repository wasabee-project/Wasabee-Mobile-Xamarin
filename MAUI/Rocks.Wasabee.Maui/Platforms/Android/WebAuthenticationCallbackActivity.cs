using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Rocks.Wasabee.Maui.Platforms.Android
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.googleusercontent.apps.269534461245-ltpks4ofjh9epvida0ct965829i4cfsi")]
    public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
    {
        // WORKAROUND:
        // Use the non-MAUI Activity and start the internal WebAuthenticatorIntermediateActivity activity
        // instead of derivng from WebAuthenticatorCallbackActivity directly.

        /*protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // start the intermediate activity again with flags to close the custom tabs
            var klass = Java.Lang.Class.FromType(typeof(Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity));
            klass = Java.Lang.Class.ForName($"{klass.PackageName}.WebAuthenticatorIntermediateActivity");
            var intent = new Intent(this, klass);
            intent.SetData(Intent.Data);
            intent.PutExtra("launched", true); // THIS IS THE FIX
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);

            // finish this activity
            Finish();
        }*/
    }
}
