using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class SelectedOpChangedMessage : MvxMessage
    {
        public string OpId { get; }

        public SelectedOpChangedMessage(object sender, string opId) : base(sender)
        {
            OpId = opId;
        }
    }

    public class UserLoggedInMessage : MvxMessage
    {
        public UserLoggedInMessage(object sender) : base(sender)
        {
        }
    }

    public class NotificationMessage : MvxMessage
    {
        public string Message { get; }

        public NotificationMessage(object sender, string message) : base(sender)
        {
            Message = message;
        }
    }
}