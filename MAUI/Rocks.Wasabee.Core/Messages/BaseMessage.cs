namespace Rocks.Wasabee.Core.Messages
{
    public abstract class BaseMessage
    {
        public object Sender { get; }

        protected BaseMessage(object sender)
        {
            ArgumentNullException.ThrowIfNull(sender);

            Sender = sender;
        }
    }
}
