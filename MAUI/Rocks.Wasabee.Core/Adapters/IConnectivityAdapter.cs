namespace Rocks.Wasabee.Core.Adapters
{
    public interface IConnectivityAdapter
    {
        bool IsConnected { get; }

        event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;
    }
}