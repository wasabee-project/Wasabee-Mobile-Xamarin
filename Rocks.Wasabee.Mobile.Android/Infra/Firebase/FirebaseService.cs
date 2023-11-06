using System;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.Messaging;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Services;

namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        private readonly ILoggingService _loggingService;

        public FirebaseService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task<string> GetFcmToken()
        {
            try
            {
                var token = await FirebaseMessaging.Instance.GetToken();
                if (token != null)
                    return token.ToString();

                return string.Empty;
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error retrieving FirebaseMessaging Token");
                return string.Empty;
            }
        }
    }
}