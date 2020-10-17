using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.ViewModels;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class MessageFrom<T> : MvxMessage where T : BaseViewModel
    {
        public object? Data { get; }

        public MessageFrom(object sender, object? data = null) : base(sender)
        {
            Data = data;
        }
    }
}