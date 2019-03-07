using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Core
{
    public class MysqlUserConnection : UserConnection
    {
        private readonly MySqlConnection _dbConnection;
        private Task _dbConnectionTask;

        public MysqlUserConnection(MySqlConnection dbConnection, QueryParser queryParser)
            : base(queryParser)
        {
            _dbConnection = dbConnection;
        }

        protected override Task OpenConnection()
        {
            return _dbConnectionTask ?? (_dbConnectionTask = _dbConnection.OpenAsync());
        }

        protected override IQueryRunner QueryRunner => new MysqlQueryRunner(_dbConnection);

        public override async Task CloseAsync()
        {
            await _dbConnection.CloseAsync();
        }
    }
}
