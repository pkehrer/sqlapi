using MySql.Data.MySqlClient;
using Service;
using Service.Services;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ConnectionTests
    {
        private MySqlConnection SourceConnection
        {
            get
            {
                var config = new DbConnectionConfig
                {
                    Database = "sqlapi",
                    Username = "pkehrer",
                    Password = "pass12345",
                    Server = "sqlapiserver.clnvzv9jjsea.us-east-1.rds.amazonaws.com"
                };

                var connectionString = $"Server={config.Server};User ID={config.Username};Password={config.Password};Database={config.Database}";
                return new MySqlConnection(connectionString);
            }
        }

        [Fact]
        public async Task Test1()
        {
            using (var connection = new Connection(SourceConnection))
            {
                var response = (await connection.RunQuery("SELECT CURDATE();")).ToString();
                
            }

            Assert.True(true);
        }
    }
}