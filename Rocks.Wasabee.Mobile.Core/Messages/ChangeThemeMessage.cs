using MvvmCross.Plugin.Messenger;

namespace Rocks.Wasabee.Mobile.Core.Messages
{
    public class ChangeThemeMessage : MvxMessage
    {
        public Theme Theme { get; }

        public ChangeThemeMessage(object sender, Theme theme) : base(sender)
        {
            Theme = theme;
        }
    }

    public enum Theme
    {
        Light,
        Dark
    }
}