namespace Rocks.Wasabee.Core
{
    public sealed class CoreApp
    {
        private static CoreApp? _instance;
        public static CoreApp Instance
        {
            get => _instance!;
        }

        public IServiceProvider Services { get; }

        public CoreApp(IServiceProvider services)
        {
            _instance = this;

            Services = services;
        }
    }
}