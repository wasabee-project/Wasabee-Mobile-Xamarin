namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public sealed class DevAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "dev";

        public DevAppSettings()
        {
            AndroidAppCenterKey = "8f922eeb-7f17-4894-937a-b8b8b5cab085";
            IosAppCenterKey = "f215a3f5-2bf8-438c-9c65-d7ee6fd66059";
        }
    }
}