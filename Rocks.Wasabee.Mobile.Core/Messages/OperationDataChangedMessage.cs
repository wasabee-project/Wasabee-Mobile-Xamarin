using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class OperationDataChangedMessage : MvxMessage
    {
        public OperationModel? OperationModel { get; }

        public OperationDataChangedMessage(object sender, OperationModel? operationModel = null) : base(sender)
        {
            OperationModel = operationModel;
        }
    }
}