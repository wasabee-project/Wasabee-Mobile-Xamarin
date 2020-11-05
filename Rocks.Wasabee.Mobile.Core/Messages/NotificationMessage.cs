using MvvmCross.Plugin.Messenger;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class NotificationMessage : MvxMessage
    {
        public string Message { get; }
        public IDictionary<string, string> Data { get; }

        public NotificationMessage(object sender, string message, IDictionary<string, string> data) : base(sender)
        {
            Message = message;
            Data = data;
        }
    }
}