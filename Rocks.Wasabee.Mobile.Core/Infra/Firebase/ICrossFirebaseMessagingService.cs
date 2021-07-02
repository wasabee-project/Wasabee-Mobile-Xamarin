using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase
{
    public interface ICrossFirebaseMessagingService
    {
        void Initialize();
        Task<bool> SendRegistrationToServer(string registrationToken);
        Task ProcessMessageData(IDictionary<string, string> data);
    }
}