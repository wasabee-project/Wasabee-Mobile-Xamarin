namespace Rocks.Wasabee.Core.Settings.Application
{
    public sealed class DevAppSettings : WasabeeAppSettings
    {
        public DevAppSettings()
        {
            ClientId = "269534461245-ltpks4ofjh9epvida0ct965829i4cfsi.apps.googleusercontent.com";
            BaseRedirectUrl = "com.googleusercontent.apps.269534461245-ltpks4ofjh9epvida0ct965829i4cfsi";
        }

        protected override string Environnement { get; set; } = "dev";
    }
}
