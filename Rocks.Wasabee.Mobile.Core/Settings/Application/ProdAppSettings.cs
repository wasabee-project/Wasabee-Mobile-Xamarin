namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public sealed class ProdAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "release";

        public ProdAppSettings()
        {
            ClientId = "269534461245-ltpks4ofjh9epvida0ct965829i4cfsi.apps.googleusercontent.com";
            BaseRedirectUrl = "com.googleusercontent.apps.269534461245-ltpks4ofjh9epvida0ct965829i4cfsi";
            AndroidAppCenterKey = "bde7e1b2-e40a-4bcf-9398-8b4cdba9634f";
        }
    }
}