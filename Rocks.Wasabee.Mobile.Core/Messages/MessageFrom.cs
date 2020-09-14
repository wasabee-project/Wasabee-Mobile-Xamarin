using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.ViewModels;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class MessageFrom<T> : MvxMessage where T : BaseViewModel
    {
        public MessageFrom(object sender) : base(sender)
        {
        }
    }
}