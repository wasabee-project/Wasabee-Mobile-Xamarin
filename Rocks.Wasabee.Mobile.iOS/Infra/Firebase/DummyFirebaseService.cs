using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Services;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
	public class DummyFirebaseService : IFirebaseService
	{
        public Task<string> GetFcmToken()
        {
			return Task.FromResult(string.Empty);
        }
    }
}

