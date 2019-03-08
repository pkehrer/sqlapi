using System.Threading.Tasks;

namespace AdminDataPopulator
{
    internal interface ITablePopulator
    {
        Task Create(CsvFile file);
        Task Drop(CsvFile file);
    }
}