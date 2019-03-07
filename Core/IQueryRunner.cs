using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public interface IQueryRunner
    {
        Task<DbResult> RunQuery(string query);
    }
}