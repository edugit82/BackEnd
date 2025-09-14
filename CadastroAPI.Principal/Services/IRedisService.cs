using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IRedisService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task<bool> DeleteAsync(string key);
    }
}