using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class ShowMarkerOnMapMessage : MvxMessage
    {
        public MarkerModel Marker { get; }

        public ShowMarkerOnMapMessage(object sender, MarkerModel marker) : base(sender)
        {
            Marker = marker;
        }
    }
}