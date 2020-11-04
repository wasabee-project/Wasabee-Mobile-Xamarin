namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public sealed class ProdAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "release";

        public ProdAppSettings()
        {
            AndroidAppCenterKey = "bde7e1b2-e40a-4bcf-9398-8b4cdba9634f";
            IosAppCenterKey = string.Empty;
        }
    }
}