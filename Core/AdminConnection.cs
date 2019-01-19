using Core.Models;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace Core
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

        public async Task<DbResult> RunAdminQuery(string query)
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var runner = new QueryRunner(connection);
            var result = await runner.RunQuery(query);
            await connection.CloseAsync();
            return result;
        }

        public async Task InitializeUser(UserConfiguration config)
        {
            var connection = new MySqlConnection(ConnectionString);
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
            await connection.CloseAsync();
        }

        public async Task CleanupUser(UserConfiguration config)
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var runner = new QueryRunner(connection);
            await runner.RunQuery($"REVOKE ALL PRIVILEGES, GRANT OPTION FROM '{config.Username}'@'%';");
            await runner.RunQuery($"DROP USER '{config.Username}'@'%';");
            await runner.RunQuery($"DROP DATABASE {config.UserDatabase};");
            await connection.CloseAsync();
        }
    }
}
