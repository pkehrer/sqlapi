using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public interface IUserConnection
    {
        Task<QueryResponse> RunQuery(string query);
        Task CloseAsync();
    }
}