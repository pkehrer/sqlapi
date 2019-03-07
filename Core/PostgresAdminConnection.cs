using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Core
{
    public class PostgresAdminConnection : IAdminConnection
    {
        private readonly DbConnectionConfig _config;

        public PostgresAdminConnection(IOptions<DbConnectionConfig> config)
        {
            _config = config.Value;
        }
        
        public string ConnectionString =>
            $"Server={_config.Server};User ID={_config.Username};Password={_config.Password};Database={_config.Database}";
        
        public async Task<DbResult> RunAdminQuery(string query)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var runner = new PostgresQueryRunner(connection);
                return await runner.RunQuery(query);
            }
        }

        public async Task<List<string>> TableNames(string owner = null)
        {
            var result = await RunAdminQuery(
                $"SELECT tablename FROM pg_catalog.pg_tables where tableowner = '{owner ?? _config.Username}';");
            return result.Rows.Select(r => r[0].ToString()).ToList();
        }

        public async Task InitializeUser(UserConfiguration config)
        {
            var tableNames = await TableNames();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var runner = new PostgresQueryRunner(connection);
                await runner.RunQuery($"CREATE SCHEMA S{config.UserDatabase};");
                await runner.RunQuery($"CREATE USER \"{config.Username}\" WITH PASSWORD '{config.Password}';");
                await runner.RunQuery(
                    $"GRANT ALL PRIVILEGES ON SCHEMA S{config.UserDatabase} TO \"{config.Username}\"");
                await runner.RunQuery(
                    $"GRANT CONNECT ON DATABASE {_config.Database} TO \"{config.Username}\"");
                
                foreach (var table in tableNames)
                {
                    await runner.RunQuery(
                        $@"CREATE OR REPLACE VIEW S{config.UserDatabase}.{table} AS SELECT * FROM public.{table};");
                    await runner.RunQuery(
                        $"GRANT SELECT ON S{config.UserDatabase}.{table} to \"{config.Username}\"");
                }
            }
        }

        public async Task CleanupUser(UserConfiguration config)
        {
            var tableNames = await TableNames();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var runner = new PostgresQueryRunner(connection);
                await runner.RunQuery($"REVOKE ALL PRIVILEGES ON SCHEMA S{config.UserDatabase} FROM \"{config.Username}\"");
                await runner.RunQuery($"REVOKE ALL PRIVILEGES ON DATABASE {_config.Database} FROM \"{config.Username}\"");
                foreach (var table in tableNames)
                {
                    await runner.RunQuery($"REVOKE ALL PRIVILEGES ON S{config.UserDatabase}.{table} FROM \"{config.Username}\"");
                }

                await runner.RunQuery($"DROP SCHEMA S{config.UserDatabase} CASCADE;");
                await runner.RunQuery($"DROP USER IF EXISTS \"{config.Username}\";");
            }
        }
    }
}