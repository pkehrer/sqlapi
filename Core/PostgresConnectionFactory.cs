using Core.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Core
{
    public class PostgresConnectionFactory : IConnectionFactory
    {
        private readonly DbConnectionConfig _config;
        private readonly QueryParser _queryParser;
        
        public PostgresConnectionFactory(IOptions<DbConnectionConfig> config, QueryParser queryParser)
        {
            _config = config.Value;
            _queryParser = queryParser;
        }

        public IUserConnection MakeConnection(UserConfiguration user)
        {
            var conn = new NpgsqlConnection(BuildConnectionString(user));
            return new PostgresUserConnection(conn, _queryParser);
        }
        
        private string BuildConnectionString(UserConfiguration user) =>
            $"Server={_config.Server};User ID={user.Username};Password={user.Password};Database={_config.Database};" +
            $"Search Path={user.UserDatabase}";
    }
}