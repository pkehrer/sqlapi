using Core.Models;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Core
{
    public class MysqlConnectionFactory : IConnectionFactory
    {
        private readonly DbConnectionConfig _config;
        private readonly QueryParser _queryParser;

        public MysqlConnectionFactory(IOptions<DbConnectionConfig> config, QueryParser queryParser)
        {
            _config = config.Value;
            _queryParser = queryParser;
        }
        
        public IUserConnection MakeConnection(UserConfiguration user)
        {
            var sourceConnection = new MySqlConnection(BuildConnectionString(user));
            return new MysqlUserConnection(sourceConnection, _queryParser);
        }
        
        private string BuildConnectionString(UserConfiguration user) =>
            $"Host={_config.Server};Username={user.Username};Password={user.Password};Database={user.UserDatabase}";
    }
}