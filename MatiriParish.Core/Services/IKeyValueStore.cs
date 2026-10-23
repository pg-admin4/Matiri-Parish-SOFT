using System.Threading.Tasks;

namespace MatiriParish.Core.Services
{
    public interface IKeyValueStore
    {
        Task SetAsync(string key, string value);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
    }
}
