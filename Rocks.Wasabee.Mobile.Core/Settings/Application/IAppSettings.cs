namespace Rocks.Wasabee.Mobile.Core.Settings.Application
{
    public interface IAppSettings
    {
        string GoogleAuthUrl { get; }
        string GoogleTokenUrl { get; }

        string ClientId { get; set; }
        string BaseRedirectUrl { get; set; }
        string RedirectUrl { get; }
        
        WasabeeServer Server { get; set; }
        string WasabeeBaseUrl { get; }
        string WasabeeTokenUrl { get; }
        string WasabeeOneTimeTokenUrl { get; }

        string AppCenterKey { get; set; }
    }

    public enum WasabeeServer
    {
        Undefined,
        US,
        EU,
        APAC
    }
}