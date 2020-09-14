namespace Rocks.Wasabee.Mobile.Core.Settings.User
{
    public interface IUserSettingsService
    {
        void SaveLoggedUserGoogleId(string googleId);
        string GetLoggedUserGoogleId();

        void SaveIngressName(string ingressName);
        string GetIngressName();
    }
}