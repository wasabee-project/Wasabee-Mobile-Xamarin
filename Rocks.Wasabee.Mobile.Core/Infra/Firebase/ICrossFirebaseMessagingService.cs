using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase
{
    public interface ICrossFirebaseMessagingService
    {
        void Initialize();
        Task SendRegistrationToServer(string registrationToken);
        Task ProcessMessageData(IDictionary<string, string> data);
    }
}