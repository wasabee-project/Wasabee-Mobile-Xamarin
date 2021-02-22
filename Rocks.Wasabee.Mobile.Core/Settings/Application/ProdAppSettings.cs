namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public sealed class ProdAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "release";

        public ProdAppSettings()
        {
            AndroidAppCenterKey = "bde7e1b2-e40a-4bcf-9398-8b4cdba9634f";
            IosAppCenterKey = "bdf1ed19-1f4c-45f6-b0ce-6f5048065422";
        }
    }
}