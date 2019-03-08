using System.Threading.Tasks;

namespace AdminDataPopulator
{
    public abstract class TablePopulator : ITablePopulator
    {
        public abstract Task Create(CsvFile file);

        public abstract Task Drop(CsvFile file);
    }
}