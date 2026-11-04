namespace Rocks.Wasabee.Core.Adapters
{
    public interface IPreferencesAdapter
    {
        bool ContainsKey(string key);

        void Set<T>(string key, T value);

        string Get(string key, string defaultValue);
        T Get<T>(string key, T defaultValue);

        void Remove(string key);
    }
}
