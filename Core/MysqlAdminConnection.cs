﻿using Core.Models;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class MysqlAdminConnection : IAdminConnection
    {
        private readonly DbConnectionConfig _config;

        public MysqlAdminConnection(IOptions<DbConnectionConfig> config)
        {
            _config = config.Value;
        }

        public string ConnectionString =>
            $"Host={_config.Server};Username={_config.Username};Password={_config.Password};Database={_config.Database}";

        public async Task<DbResult> RunAdminQuery(string query)
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var runner = new MysqlQueryRunner(connection);
            var result = await runner.RunQuery(query);
            await connection.CloseAsync();
            return result;
        }

        public async Task<List<string>> TableNames(string owner = null)
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var runner = new MysqlQueryRunner(connection);
            var result = await runner.RunQuery("SHOW TABLES;");
            await connection.CloseAsync();
            return result.Rows.Select(r => r[0].ToString()).ToList();
        }

        public async Task InitializeUser(UserConfiguration config)
        {
            var tableNames = await TableNames();
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var runner = new MysqlQueryRunner(connection);
            await runner.RunQuery($"CREATE DATABASE {config.UserDatabase};");
            await runner.RunQuery($"CREATE USER '{config.Username}'@'%' IDENTIFIED BY '{config.Password}'");
            await runner.RunQuery(
                $@"GRANT SELECT,INSERT,UPDATE,DELETE,CREATE,ALTER,DROP 
                    ON {config.UserDatabase}.*
                    TO '{config.Username}'@'%';");
            foreach (var table in tableNames)
            {
                await runner.RunQuery(
$@"CREATE OR REPLACE 
ALGORITHM = TEMPTABLE 
VIEW {config.UserDatabase}.{table} AS SELECT * FROM {table};");
            }
            await connection.CloseAsync();
        }

        public async Task CleanupUser(UserConfiguration config)
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var runner = new MysqlQueryRunner(connection);
            await runner.RunQuery($"REVOKE ALL PRIVILEGES, GRANT OPTION FROM '{config.Username}'@'%';");
            await runner.RunQuery($"DROP USER '{config.Username}'@'%';");
            await runner.RunQuery($"DROP DATABASE {config.UserDatabase};");
            await connection.CloseAsync();
        }
    }
}
