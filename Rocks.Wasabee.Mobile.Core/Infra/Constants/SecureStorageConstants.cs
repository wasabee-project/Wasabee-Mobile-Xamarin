using System;

namespace Rocks.Wasabee.Mobile.Core.Infra.Constants
{
    public static class SecureStorageConstants
    {
        [Obsolete]
        public static string WasabeeCookie => "WASABEE_COOKIE";
        
        public static string WasabeeJwt => "WASABEE_JWT";
        public static string GoogleToken => "GOOGLE_TOKEN";
        public static string FcmToken => "FCM_TOKEN";
    }
}