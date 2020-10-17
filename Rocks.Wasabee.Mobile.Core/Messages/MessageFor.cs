using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.ViewModels;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class MessageFor<T> : MvxMessage where T : BaseViewModel
    {

        public MessageFor(object sender) : base(sender)
        {
        }
    }
}