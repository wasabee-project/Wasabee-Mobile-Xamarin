using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class TeamAgentLocationUpdatedMessage : MvxMessage
    {
        public string TeamId { get; }
        public string UserId { get; }

        public TeamAgentLocationUpdatedMessage(object sender, string teamId, string userId) : base(sender)
        {
            TeamId = teamId;
            UserId = userId;
        }
    }
}