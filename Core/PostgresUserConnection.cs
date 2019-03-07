using System.Threading.Tasks;
using Npgsql;

namespace Core
{
    public class PostgresUserConnection : UserConnection
    {
        private readonly NpgsqlConnection _dbConnection;
        private Task _dbConnectionTask;

        public PostgresUserConnection(NpgsqlConnection dbConnection, QueryParser queryParser)
            : base(queryParser)
        {
            _dbConnection = dbConnection;
        }
        
        protected override Task OpenConnection()
        {
            return _dbConnectionTask ?? (_dbConnectionTask = Task.Run(() => _dbConnection.Open()));
        }

        
        public override Task CloseAsync()
        {
            _dbConnection.Dispose();
            return Task.CompletedTask;
        }

        protected override IQueryRunner QueryRunner => new PostgresQueryRunner(_dbConnection);
    }
}