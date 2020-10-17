using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public interface IBackgroundDataUpdaterService
    {
        Task UpdateOperation(string operationId);
        Task UpdateLink(string operationId, string linkId);
        Task UpdateMarker(string operationId, string markerId);
    }
}