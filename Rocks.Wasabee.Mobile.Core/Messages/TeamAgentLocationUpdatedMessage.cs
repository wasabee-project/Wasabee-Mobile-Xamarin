using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class TeamAgentLocationUpdatedMessage : MvxMessage
    {
        public string UserId { get; }

        public TeamAgentLocationUpdatedMessage(object sender, string userId) : base(sender)
        {
            UserId = userId;
        }
    }
}