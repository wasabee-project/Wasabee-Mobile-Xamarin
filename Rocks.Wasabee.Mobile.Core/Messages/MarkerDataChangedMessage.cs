using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class MarkerDataChangedMessage : MvxMessage
    {
        public MarkerModel MarkerModel { get; }
        public string OperationId { get; }

        public MarkerDataChangedMessage(object sender, MarkerModel markerModel, string operationId) : base(sender)
        {
            MarkerModel = markerModel;
            OperationId = operationId;
        }
    }
}