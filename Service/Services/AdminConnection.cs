using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Service.Models;
using System.Threading.Tasks;

namespace Service.Services
{
    public class AdminConnection
    {
        private readonly DbConnectionConfig _config;

        public AdminConnection(IOptions<DbConnectionConfig> config)
        {
            _config = config.Value;
        }
        private string ConnectionString =>
            $"Server={_config.Server};User ID={_config.Username};Password={_config.Password};Database={_config.Database}";

        public async Task InitializeUser(UserConfiguration config)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var runner = new QueryRunner(connection);
                await runner.RunQuery($"CREATE DATABASE {config.UserDatabase};");
                await runner.RunQuery($"CREATE USER '{config.Username}'@'%' IDENTIFIED BY '{config.Password}'");
                await runner.RunQuery(
                    $@"GRANT SELECT
                        ON {config.MasterDatabase}.*
                        TO '{config.Username}'@'%';");
                await runner.RunQuery(
                    $@"GRANT SELECT,INSERT,UPDATE,DELETE,CREATE,ALTER,DROP 
                        ON {config.UserDatabase}.*
                        TO '{config.Username}'@'%';");
            }
        }

        public async Task CleanupUser(UserConfiguration config)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var runner = new QueryRunner(connection);
                await runner.RunQuery($"REVOKE ALL PRIVILEGES, GRANT OPTION FROM '{config.Username}'@'%';");
                await runner.RunQuery($"DROP USER '{config.Username}'@'%';");
                await runner.RunQuery($"DROP DATABASE {config.UserDatabase};");
            }
        }
    }
}
