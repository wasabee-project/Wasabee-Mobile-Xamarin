using Rocks.Wasabee.Mobile.Core.Settings.Application;

namespace Rocks.Wasabee.Mobile.Core.Models
{
    public class ServerItem
    {
        public ServerItem(string name, WasabeeServer server, string image)
        {
            Name = name;
            Server = server;
            Image = image;
        }

        public string Name { get; }
        public WasabeeServer Server { get; }
        public string Image { get; }

        public static ServerItem Undefined => new ServerItem("_UNDEFINED_", WasabeeServer.Undefined, string.Empty);
    }
}