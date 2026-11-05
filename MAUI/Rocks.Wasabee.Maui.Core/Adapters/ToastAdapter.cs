using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Rocks.Wasabee.Core.Adapters;

namespace Rocks.Wasabee.Maui.Core.Adapters
{
    public class ToastAdapter : IToastAdapter
    {
        public void Show(string message)
        {
            MainThread.BeginInvokeOnMainThread(async void () =>
            {
                try
                {
                    var toast = Toast.Make(message, ToastDuration.Long);
                    await toast.Show();
                }
                catch (Exception e)
                {
                    // nothing to do here, just don't crash the app if the toast fails to show
                }
            });
        }
    }
}