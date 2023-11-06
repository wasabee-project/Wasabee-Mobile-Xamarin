using System.Threading.Tasks;
using Firebase.CloudMessaging;
using Rocks.Wasabee.Mobile.Core.Services;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        public Task<string> GetFcmToken()
        {
            return Task.FromResult(Messaging.SharedInstance.FcmToken ?? string.Empty);
        }
    }
}