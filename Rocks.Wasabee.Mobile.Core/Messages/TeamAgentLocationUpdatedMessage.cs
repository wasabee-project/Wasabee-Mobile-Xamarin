using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class TeamAgentLocationUpdatedMessage : MvxMessage
    {
        public string UserId { get; }
        public string TeamId { get; }

        public TeamAgentLocationUpdatedMessage(object sender, string userId, string teamId) : base(sender) {
            UserId = userId;
            TeamId = teamId;
        }
    }
}