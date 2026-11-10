using Rocks.Wasabee.Core.Adapters;

namespace Rocks.Wasabee.Maui.Core.Adapters
{
    public class ConnectivityAdapter : IConnectivityAdapter
    {
        public bool IsConnected { get; private set; }

        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        public ConnectivityAdapter()
        {
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            IsConnected = e.NetworkAccess == NetworkAccess.Internet;

            MainThread.BeginInvokeOnMainThread(() => ConnectivityChanged?.Invoke(sender, e));
        }
    }
}