using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class AnnouncementReceivedMessage : MvxMessage
    {
        public GenericMessagePayload Payload { get; }

        public AnnouncementReceivedMessage(object sender, GenericMessagePayload payload) : base(sender)
        {
            Payload = payload;
        }
    }
}