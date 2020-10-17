using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class LinkDataChangedMessage : MvxMessage
    {
        public LinkModel LinkData { get; }
        public string OperationId { get; }

        public LinkDataChangedMessage(object sender, LinkModel linkData, string operationId) : base(sender)
        {
            LinkData = linkData;
            OperationId = operationId;
        }
    }
}