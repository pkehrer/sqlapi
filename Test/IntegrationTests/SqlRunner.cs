using Service.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Service.IntegrationTests
{
    public class SqlRunner : IDisposable
    {
        readonly HttpClient _client;
        readonly ITestOutputHelper _output;
        readonly string _connectionId;

        private SqlRunner(HttpClient client, ITestOutputHelper output, string connectionId)
        {
            _client = client;
            _output = output;
            _connectionId = connectionId;
        }

        public void Dispose()
        {
            CloseConnection(_connectionId).Wait();
        }

        public async Task<QueryResponse> Run(string query)
        {
            var request = new QueryRequest { ConnectionId = _connectionId, Query = query };
            var result = await PostToService<QueryResponse>(_client, "/query", request);
            _output.WriteLine($"Response from query \"{query}\":");
            _output.WriteLine(result.ToString());
            return result;
        }

        public static async Task<SqlRunner> Create(HttpClient client, ITestOutputHelper output)
        {
            var connectionId = await ConnectionId(client);
            var runner = new SqlRunner(client, output, connectionId);
            return runner;
        }
        
        private static async Task<string> ConnectionId(HttpClient client)
            => (await PostToService<ConnectionResponse>(client, "/connection")).ConnectionId;

        private async Task CloseConnection(string connectionId)
        {
            var deleteResponse = await _client.DeleteAsync($"/connection/{connectionId}");
            deleteResponse.EnsureSuccessStatusCode();
        }

        private static async Task<T> PostToService<T>(HttpClient client, string path, object body = null)
        {
            var response = await client.PostAsJsonAsync(path, body ?? new object());
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<T>();
        }
    }
}
