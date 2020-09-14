using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class UserLoggedInMessage : MvxMessage
    {
        public UserLoggedInMessage(object sender) : base(sender)
        {
        }
    }
}