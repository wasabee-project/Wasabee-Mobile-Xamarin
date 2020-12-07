using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class ChangeOrientationMessage : MvxMessage
    {
        public Orientation Orientation { get; }

        public ChangeOrientationMessage(object sender, Orientation orientation) : base(sender)
        {
            Orientation = orientation;
        }
    }

    public enum Orientation
    {
        Portait,
        Landscape,
        Any
    }
}