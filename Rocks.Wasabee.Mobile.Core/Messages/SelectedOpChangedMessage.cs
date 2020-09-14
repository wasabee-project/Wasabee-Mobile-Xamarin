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
}