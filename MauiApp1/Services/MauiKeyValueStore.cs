using System.Threading.Tasks;
using MatiriParish.Core.Services;
using Microsoft.Maui.Storage;

namespace MauiApp1.Services
{
    public class MauiKeyValueStore : IKeyValueStore
    {
        public Task SetAsync(string key, string value)
        {
            return SecureStorage.SetAsync(key, value);
        }

        public async Task<string?> GetAsync(string key)
        {
            try
            {
                return await SecureStorage.GetAsync(key);
            }
            catch
            {
                return null;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                SecureStorage.Remove(key);
            }
            catch { }
            return Task.CompletedTask;
        }
    }
}
