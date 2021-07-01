using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class TargetReceivedMessage : MvxMessage
    {
        public TargetPayload Payload { get; }

        public TargetReceivedMessage(object sender, TargetPayload payload) : base(sender)
        {
            Payload = payload;
        }
    }
}