using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Settings.User
{
    public sealed class UserSettingsService : IUserSettingsService
    {
        private const string GoogleIdKey = "GOOGLE_ID";
        private const string IngressNameKey = "INGRESS_NAME";

        private readonly IPreferences _preferences;

        public UserSettingsService(IPreferences preferences)
        {
            _preferences = preferences;
        }

        public void SaveLoggedUserGoogleId(string googleId)
        {
            if (_preferences.ContainsKey(GoogleIdKey))
                _preferences.Remove(GoogleIdKey);

            _preferences.Set(GoogleIdKey, googleId);
        }

        public string GetLoggedUserGoogleId()
        {
            return _preferences.Get(GoogleIdKey, string.Empty);
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