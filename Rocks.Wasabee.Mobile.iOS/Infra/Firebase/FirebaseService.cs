using Firebase.CloudMessaging;
using Rocks.Wasabee.Mobile.Core.Services;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        public string GetFcmToken()
        {
            return Messaging.SharedInstance.FcmToken ?? string.Empty;

        }
    }
}