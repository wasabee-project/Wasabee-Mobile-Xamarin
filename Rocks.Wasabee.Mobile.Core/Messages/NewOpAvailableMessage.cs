using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class NewOpAvailableMessage : MvxMessage
    {
        public NewOpAvailableMessage(object sender) : base(sender)
        {
        }
    }
}