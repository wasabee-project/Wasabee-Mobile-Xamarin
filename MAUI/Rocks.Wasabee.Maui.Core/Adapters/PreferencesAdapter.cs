using Rocks.Wasabee.Core.Adapters;

namespace Rocks.Wasabee.Maui.Core.Adapters
{
    public class PreferencesAdapter : IPreferencesAdapter
    {
        public bool ContainsKey(string key) => Preferences.Default.ContainsKey(key);

        public void Set<T>(string key, T value) => Preferences.Default.Set(key, value);

        public string Get(string key, string defaultValue) => Preferences.Default.Get(key, defaultValue);
        public T Get<T>(string key, T defaultValue) => Preferences.Default.Get(key, defaultValue);

        public void Remove(string key) => Preferences.Default.Remove(key);
    }
}
