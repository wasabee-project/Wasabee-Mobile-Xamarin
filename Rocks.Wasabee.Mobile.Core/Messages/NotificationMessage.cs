using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class NotificationMessage : MvxMessage
    {
        public string Message { get; }

        public NotificationMessage(object sender, string message) : base(sender)
        {
            Message = message;
        }
    }
}