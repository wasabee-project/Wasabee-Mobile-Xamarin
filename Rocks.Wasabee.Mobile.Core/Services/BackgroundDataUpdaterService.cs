using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public class BackgroundDataUpdaterService : IBackgroundDataUpdaterService
    {
        private readonly IMvxMessenger _mvxMessenger;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly LinksDatabase _linksDatabase;
        private readonly MarkersDatabase _markersDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly UsersDatabase _usersDatabase;

        public BackgroundDataUpdaterService(IMvxMessenger mvxMessenger,
            WasabeeApiV1Service wasabeeApiV1Service, OperationsDatabase operationsDatabase,
            LinksDatabase linksDatabase, MarkersDatabase markersDatabase,
            TeamsDatabase teamsDatabase, UsersDatabase usersDatabase)
        {
            _mvxMessenger = mvxMessenger;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _operationsDatabase = operationsDatabase;
            _linksDatabase = linksDatabase;
            _markersDatabase = markersDatabase;
            _teamsDatabase = teamsDatabase;
            _usersDatabase = usersDatabase;
        }

        public async Task UpdateOperationAndNotify(string operationId)
        {
            var operationData = await _wasabeeApiV1Service.Operations_GetOperation(operationId);
            if (operationData != null)
            {
                await _operationsDatabase.SaveOperationModel(operationData);
                _mvxMessenger.Publish(new OperationDataChangedMessage(this, operationData));
            }
        }

        public async Task UpdateLinkAndNotify(string operationId, string linkId)
        {
            var linkData = await _wasabeeApiV1Service.Operations_GetLink(operationId, linkId);
            if (linkData != null)
            {
                await _linksDatabase.SaveLinkModel(linkData, operationId);
                _mvxMessenger.Publish(new LinkDataChangedMessage(this, linkData, operationId));
            }
        }

        public async Task UpdateMarkerAndNotify(string operationId, string markerId)
        {
            var markerData = await _wasabeeApiV1Service.Operations_GetMarker(operationId, markerId);
            if (markerData != null)
            {
                await _markersDatabase.SaveMarkerModel(markerData, operationId);
                _mvxMessenger.Publish(new MarkerDataChangedMessage(this, markerData, operationId));
            }
        }
    }
}