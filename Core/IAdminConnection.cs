using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public interface IAdminConnection
    {
        string ConnectionString { get; }
        Task<DbResult> RunAdminQuery(string query);
        Task<List<string>> TableNames(string owner = null);
        Task InitializeUser(UserConfiguration config);
        Task CleanupUser(UserConfiguration config);
    }
}