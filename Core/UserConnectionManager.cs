using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public class UserConnectionManager
    {
        private readonly IConnectionFactory _connectionFactory;
        
        private readonly IAdminConnection _adminConnection;
        
        private readonly IDictionary<string, Tuple<IUserConnection, UserConfiguration>> _openConnections
            = new Dictionary<string, Tuple<IUserConnection, UserConfiguration>>();

        public UserConnectionManager(
            IConnectionFactory connectionFactory,
            IAdminConnection adminConnection)
        {
            _connectionFactory = connectionFactory;
            _adminConnection = adminConnection;
        }

        public async Task<ConnectionResponse> OpenNewConnection()
        {
            var connectionId = Guid.NewGuid().ToString();
            var uniqueString = GenerateUniqueString();
            var userConfig = new UserConfiguration
            {
                ConnectionId = connectionId,
                Username = $"u{uniqueString}",
                Password = GenerateUniqueString(),
                UserDatabase = $"{uniqueString}DB",
                MasterDatabase = "sqlapi"
            };

            await _adminConnection.InitializeUser(userConfig);
            _openConnections[connectionId] = Tuple.Create(
                _connectionFactory.MakeConnection(userConfig),
                userConfig);

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

        public async Task<QueryResponse> RunQuery(string connectionId, string query)
                 {
            var connectionInfo = _openConnections[connectionId];
            if (connectionInfo == null)
            {
                throw new Exception($"ConnectionId {connectionId} is either closed or never existed. Create a new one");
            }

            return await connectionInfo.Item1.RunQuery(query);
        }
    }
}
