using CommunityToolkit.Mvvm.Messaging;
using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Messages;

namespace Rocks.Wasabee.Maui.Core.Adapters
{
    public class MessengerAdapter : IMessengerAdapter
    {
        public void Register<TRecipient, TMessage, TToken>(TRecipient recipient, MessageHandler<TRecipient, TMessage> handler)
            where TRecipient : class
            where TMessage : BaseMessage
        {
            WeakReferenceMessenger.Default.Register(recipient, handler);
        }

        public TMessage Send<TMessage>(TMessage message)
            where TMessage : BaseMessage
        {
            return WeakReferenceMessenger.Default.Send(message);
        }
    }
}
