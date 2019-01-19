using Core;
using Core.Models;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserConnectionManager
    {
        private readonly DbConnectionConfig _config;
        private readonly AdminConnection _adminConnection;
        private readonly QueryParser _queryParser;
        private readonly IDictionary<string, Tuple<UserConnection, UserConfiguration>> _openConnections
            = new Dictionary<string, Tuple<UserConnection, UserConfiguration>>();

        public UserConnectionManager(
            IOptions<DbConnectionConfig> config,
            AdminConnection adminConnection,
            QueryParser queryParser)
        {
            _config = config.Value;
            _adminConnection = adminConnection;
            _queryParser = queryParser;
        }

        private string BuildConnectionString(UserConfiguration uconfig) =>
            $"Server={_config.Server};User ID={uconfig.Username};Password={uconfig.Password};Database={uconfig.UserDatabase}";

        public async Task<ConnectionResponse> OpenNewConnection()
        {
            var connectionId = Guid.NewGuid().ToString();
            var uniqueString = GenerateUniqueString();
            var userConfig = new UserConfiguration
            {
                ConnectionId = connectionId,
                Username = $"u{uniqueString}",
                Password = Guid.NewGuid().ToString(),
                UserDatabase = $"{uniqueString}DB",
                MasterDatabase = "sqlapi"
            };

            await _adminConnection.InitializeUser(userConfig);
            _openConnections[connectionId] = CreateConnection(userConfig);

            return new ConnectionResponse
            {
                ConnectionId = connectionId
            };
        }

        private string GenerateUniqueString()
        {
            var random = new Random();
            const string availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            var username = string.Empty;
            for (var i = 0; i < 15; i++)
            {
                var charIndex = random.Next(availableChars.Length);
                username += availableChars[charIndex];
            }
            return username;
        }

        public async Task CloseConnection(string connectionId)
        {
            var connectionInfo = _openConnections[connectionId];
            if (connectionInfo != null)
            {
                _openConnections.Remove(connectionId);
                await connectionInfo.Item1.CloseAsync();
                await _adminConnection.CleanupUser(connectionInfo.Item2);
                await Task.FromResult(1);
            }
        }

        public async Task<QueryResponse> RunQuery(QueryRequest request)
        {
            var connectionInfo = _openConnections[request.ConnectionId];
            if (connectionInfo == null)
            {
                throw new Exception($"ConnectionId {request.ConnectionId} is either closed or never existed. Create a new one");
            }

            return await connectionInfo.Item1.RunQuery(request.Query);
        }

        private Tuple<UserConnection, UserConfiguration> CreateConnection(UserConfiguration userConfig)
        {
            var sourceConnection = new MySqlConnection(BuildConnectionString(userConfig));
            return Tuple.Create(new UserConnection(sourceConnection, _queryParser), userConfig);
        }
    }
}
