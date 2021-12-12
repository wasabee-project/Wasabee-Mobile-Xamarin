using MvvmCross.Plugin.Messenger;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Cache;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads;
using Rocks.Wasabee.Mobile.Core.Infra.LocalNotification;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase
{
    public class CrossFirebaseMessagingService : ICrossFirebaseMessagingService
    {
        private readonly IMvxMessenger _mvxMessenger;
        private readonly WasabeeApiV1Service _wasabeeApiV1;
        private readonly IBackgroundDataUpdaterService _backgroundDataUpdaterService;
        private readonly ISecureStorage _secureStorage;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IFirebaseService _firebaseService;
        private readonly ILocalNotificationService _localNotificationService;
        private readonly OperationsDatabase _operationsDatabase;
        
        private MvxSubscriptionToken? _mvxToken;

        private string _fcmToken = string.Empty;
        private bool _isInitialized;

        public CrossFirebaseMessagingService(IMvxMessenger mvxMessenger, WasabeeApiV1Service wasabeeApiV1, 
            IBackgroundDataUpdaterService backgroundDataUpdaterService, ISecureStorage secureStorage, 
            IUserSettingsService userSettingsService, IFirebaseService firebaseService, 
            ILocalNotificationService localNotificationService, OperationsDatabase operationsDatabase)
        {
            _mvxMessenger = mvxMessenger;
            _wasabeeApiV1 = wasabeeApiV1;
            _backgroundDataUpdaterService = backgroundDataUpdaterService;
            _secureStorage = secureStorage;
            _userSettingsService = userSettingsService;
            _firebaseService = firebaseService;
            _localNotificationService = localNotificationService;
            _operationsDatabase = operationsDatabase;
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            _mvxToken ??= _mvxMessenger.Subscribe<UserLoggedInMessage>(async msg => await SendRegistrationToServer(_firebaseService.GetFcmToken()));
        }
        
        public async Task<bool> SendRegistrationToServer(string registrationToken)
        {
            if (!_isInitialized)
                Initialize();

            if (string.IsNullOrWhiteSpace(registrationToken))
                return false;

            _fcmToken = registrationToken;
            
            await _secureStorage.SetAsync(SecureStorageConstants.FcmToken, _fcmToken);
            return await _wasabeeApiV1.User_UpdateFirebaseToken(_fcmToken);
        }

        public async Task ProcessMessageData(IDictionary<string, string> data)
        {
            if (!_isInitialized)
                Initialize();

            var cmd = data.ContainsKey("cmd") ? data.First(x => x.Key.Equals("cmd")).Value : string.Empty;
            var msg = data.ContainsKey("msg") ? data.First(x => x.Key.Equals("msg")).Value : string.Empty;

            if (string.IsNullOrEmpty(cmd) || string.IsNullOrEmpty(msg))
                return;

            var messageBody = $"{cmd} : {msg}";
#if DEBUG
            Debug.WriteLine($"[FIREBASE] : {messageBody}");
#endif
            _mvxMessenger.Publish(new NotificationMessage(this, messageBody, data));

            var opId = data.FirstOrDefault(x => x.Key.Equals("opID")).Value;
            var updateId = data.FirstOrDefault(x => x.Key.Equals("updateID")).Value;

            if (cmd.Contains("Agent Location Change"))
            {
                var gid = data.FirstOrDefault(x => x.Key.Equals("gid")).Value;
                _mvxMessenger.Publish(new TeamAgentLocationUpdatedMessage(this, gid, msg));
            }
            else if (cmd.Contains("Marker"))
            {
                var markerId = data.FirstOrDefault(x => x.Key.Equals("markerID")).Value;

                if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(markerId))
                {
                    if (!string.IsNullOrWhiteSpace(updateId))
                    {
                        if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                            await _backgroundDataUpdaterService.UpdateMarkerAndNotify(opId, markerId)
                                .ConfigureAwait(false);
                    }

                    var op = await _operationsDatabase.GetOperationModel(opId);
                    var marker = op?.Markers.FirstOrDefault(x => x.Id.Equals(markerId));
                    if (marker != null)
                    {
                        var loggedUserGid = _userSettingsService.GetLoggedUserGoogleId();
                        if (marker.AssignedTo.Equals(loggedUserGid))
                            _localNotificationService.Send($"{op!.Name} : Marker {msg}");
                    }
                }
            }
            else if (cmd.Contains("Link"))
            {
                var linkId = data.FirstOrDefault(x => x.Key.Equals("linkID")).Value;

                if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(linkId))
                {
                    if (!string.IsNullOrWhiteSpace(updateId))
                    {
                        if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                            await _backgroundDataUpdaterService.UpdateLinkAndNotify(opId, linkId)
                                .ConfigureAwait(false);
                    }

                    var op = await _operationsDatabase.GetOperationModel(opId);
                    var link = op?.Links.FirstOrDefault(x => x.Id.Equals(linkId));
                    if (link != null)
                    {
                        var loggedUserGid = _userSettingsService.GetLoggedUserGoogleId();
                        if (link.AssignedTo.Equals(loggedUserGid))
                            _localNotificationService.Send($"{op!.Name} : Link {msg}");
                    }
                }
            }
            else if (cmd.Contains("Map Change"))
            {
                if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(updateId))
                {
                    if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                        await _backgroundDataUpdaterService.UpdateOperationAndNotify(opId).ConfigureAwait(false);
                }
            }
            else if (cmd.Equals("Target"))
            {
                var targetPayload = JsonConvert.DeserializeObject<TargetPayload>(msg);
                _localNotificationService.Send($"Target from {targetPayload.Sender}: {targetPayload.Name}");
                _mvxMessenger.Publish(new TargetReceivedMessage(this, targetPayload));
            }
        }

        #region Private methods

        private bool CheckIfUpdateIdShouldBeProcessedOrNot(string updateId)
        {
            var result = false;
            if (OperationsUpdatesCache.Data.ContainsKey(updateId))
            {
                if (OperationsUpdatesCache.Data[updateId] == false)
                {
                    result = true;
                    OperationsUpdatesCache.Data[updateId] = true;
#if DEBUG
                    Debug.WriteLine($"updateID '{updateId}' isn't processed, update will be done");
                }
                else
                {
                    Debug.WriteLine($"updateID '{updateId}' has already been processed, update aborted");
#endif
                }
            }
            else
            {
                result = true;
                OperationsUpdatesCache.Data.Add(updateId, true);
#if DEBUG
                Debug.WriteLine($"updateID '{updateId}' isn't processed, update will be done");
#endif
            }

            return result;
        }

        #endregion
    }
}