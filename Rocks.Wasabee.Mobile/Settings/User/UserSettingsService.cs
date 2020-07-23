using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Settings.User
{
    public sealed class UserSettingsService : IUserSettingsService
    {
        private const string IngressNameKey = "INGRESS_NAME";

        private readonly IPreferences _preferences;

        public UserSettingsService(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public void SaveIngressName(string ingressName)
        {
            if (_preferences.ContainsKey(IngressNameKey))
                _preferences.Remove(IngressNameKey);

            _preferences.Set(IngressNameKey, ingressName);
        }

        public string GetIngressName()
        {
            return _preferences.Get(IngressNameKey, string.Empty);
        }
    }
}