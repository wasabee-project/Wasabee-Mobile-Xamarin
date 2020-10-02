using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class ShowPortalOnMapMessage : MvxMessage
    {
        public PortalModel Portal { get; }

        public ShowPortalOnMapMessage(object sender, PortalModel portal) : base(sender)
        {
            Portal = portal;
        }
    }
}