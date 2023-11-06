using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public interface IFirebaseService
    {
        Task<string> GetFcmToken();
    }
}