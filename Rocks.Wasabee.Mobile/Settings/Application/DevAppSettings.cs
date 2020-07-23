namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public sealed class DevAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "dev";

        public DevAppSettings() : base()
        {
            ClientId = "269534461245-ltpks4ofjh9epvida0ct965829i4cfsi.apps.googleusercontent.com";
            BaseRedirectUrl = "com.googleusercontent.apps.269534461245-ltpks4ofjh9epvida0ct965829i4cfsi";
        }
    }
}