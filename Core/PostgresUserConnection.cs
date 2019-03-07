using System.Threading.Tasks;
using Npgsql;

namespace Core
{
    public class PostgresUserConnection : UserConnection
    {
        private readonly NpgsqlConnection _dbConnection;

        public PostgresUserConnection(NpgsqlConnection dbConnection, QueryParser queryParser)
            : base(queryParser)
        {
            _dbConnection = dbConnection;
        }
        
        protected override Task OpenConnection()
        {
            _dbConnection.Open();
            return Task.CompletedTask;
        }
        
        public override Task CloseAsync()
        {
            _dbConnection.Dispose();
            return Task.CompletedTask;
        }

        protected override IQueryRunner QueryRunner => new PostgresQueryRunner(_dbConnection);
    }
}