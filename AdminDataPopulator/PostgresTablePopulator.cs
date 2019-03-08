using System.Threading.Tasks;
using Core;

namespace AdminDataPopulator
{
    public class PostgresTablePopulator : TablePopulator
    {
        private readonly PostgresAdminConnection _conn;

        public PostgresTablePopulator(PostgresAdminConnection conn)
        {
            _conn = conn;
        }
        
        public override Task Create(CsvFile file)
        {
            throw new System.NotImplementedException();
        }

        public override Task Drop(CsvFile file)
        {
            throw new System.NotImplementedException();
        }
    }
}