using Firebase.Iid;
using Rocks.Wasabee.Mobile.Core.Services;

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        public string GetFcmToken()
        {
            if (FirebaseInstanceId.Instance.Token != null)
                return FirebaseInstanceId.Instance.Token;

            return string.Empty;
        }
    }
}
#pragma warning restore CS0618