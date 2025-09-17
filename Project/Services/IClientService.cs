using System.Threading.Tasks;

namespace Project.Services
{
    public interface IClientService
    {
        Task AddClientAsync(string message);
    }
}