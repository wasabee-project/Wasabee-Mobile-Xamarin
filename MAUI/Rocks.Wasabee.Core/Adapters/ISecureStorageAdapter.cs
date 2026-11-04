namespace Rocks.Wasabee.Core.Adapters
{
    public interface ISecureStorageAdapter
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);
        bool Remove(string key);
        void RemoveAll();
    }
}
