using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public interface IBackgroundDataUpdaterService
    {
        Task UpdateOperationAndNotify(string operationId);
        Task UpdateLinkAndNotify(string operationId, string linkId);
        Task UpdateMarkerAndNotify(string operationId, string markerId);
    }
}