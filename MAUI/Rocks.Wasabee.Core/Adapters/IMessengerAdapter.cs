using CommunityToolkit.Mvvm.Messaging;
using Rocks.Wasabee.Core.Messages;

namespace Rocks.Wasabee.Core.Adapters
{
    public interface IMessengerAdapter
    {
        void Register<TRecipient, TMessage, TToken>(TRecipient recipient, MessageHandler<TRecipient, TMessage> handler)
            where TRecipient : class
            where TMessage : BaseMessage;

        TMessage Send<TMessage>(TMessage message)
            where TMessage : BaseMessage;
    }
}