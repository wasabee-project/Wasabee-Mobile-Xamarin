namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public sealed class DevAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "dev";
    }
}