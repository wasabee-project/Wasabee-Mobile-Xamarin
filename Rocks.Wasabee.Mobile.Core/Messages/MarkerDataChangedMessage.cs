using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class MarkerDataChangedMessage : MvxMessage
    {
        public MarkerModel MarkerData { get; }
        public string OperationId { get; }

        public MarkerDataChangedMessage(object sender, MarkerModel markerData, string operationId) : base(sender)
        {
            MarkerData = markerData;
            OperationId = operationId;
        }
    }
}