namespace Rocks.Wasabee.Mobile.Core.Settings.User
{
    public interface IUserSettingsService
    {
        void SaveIngressName(string ingressName);
        string GetIngressName();
    }
}