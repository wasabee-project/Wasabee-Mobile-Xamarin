using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class LiveGeolocationTrackingMessage : MvxMessage
    {
        public Action Action { get; }

        public LiveGeolocationTrackingMessage(object sender, Action action) : base(sender)
        {
            Action = action;
        }
    }

    public enum Action
    {
        Start,
        Stop
    }
}