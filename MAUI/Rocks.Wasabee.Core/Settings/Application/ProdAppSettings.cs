namespace Rocks.Wasabee.Core.Settings.Application
{
    public sealed class ProdAppSettings : WasabeeAppSettings
    {
        protected override string Environnement { get; set; } = "release";
    }
}
