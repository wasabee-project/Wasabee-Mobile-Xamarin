#if __ANDROID__
#if PROD
[assembly: Android.App.MetaData("com.google.android.geo.API_KEY", Value = "<Android GoogleMaps API Key for PROD>")]
#else
[assembly: Android.App.MetaData("com.google.android.geo.API_KEY", Value = "<Android GoogleMaps API Key for DEV>")]
#endif
#endif


#if __ANDROID__
namespace Rocks.Wasabee.Mobile.Droid
#elif __IOS__
namespace Rocks.Wasabee.Mobile.iOS
#endif
{
    public static class OAuthClient
    {
#if __ANDROID__
        public const string Id = "269534461245-ltpks4ofjh9epvida0ct965829i4cfsi.apps.googleusercontent.com";
        public const string Redirect = "com.googleusercontent.apps.269534461245-ltpks4ofjh9epvida0ct965829i4cfsi";
#elif __IOS__
        public const string Id = "269534461245-kp961iiqgl661nsd20p5i74n6grhstq8.apps.googleusercontent.com";
        public const string Redirect = "com.googleusercontent.apps.269534461245-kp961iiqgl661nsd20p5i74n6grhstq8";
#endif
    }

    public static class AppCenterKeys
    {
        public static string Value
        {
            get
            {
                return 
#if __ANDROID__
    #if PROD
                    "<AppCenter Android Prod Key>";
    #else
                    "<AppCenter Android Dev Key>";
    #endif
#elif __IOS__
    #if PROD
                    "<AppCenter iOS Prod Key>";
    #else
                    "<AppCenter iOS Dev Key>";
    #endif
#endif
            }
        }
    }

#if __IOS__
    public static class MapsKey
    {
        public const string Value = "<iOS GoogleMaps API Key>";
    }
#endif
}