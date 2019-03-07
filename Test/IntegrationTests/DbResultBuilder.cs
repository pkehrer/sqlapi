using Core.Models;
using System.Linq;

namespace Service.IntegrationTests
{
    public static class DbResultBuilder
    {

        public static DbResult Create() => new DbResult();

        public static DbResult SetColumnNames(this DbResult result, params string[] columnNames)
        {
            result.ColumnNames = columnNames;
            return result;
        }

        public static DbResult AddRow(this DbResult result, params object[] row)
        {
            result.Rows = result.Rows
                .Concat(new[] { row })
                .ToArray();
            return result;
        }

        public static DbResult SetRowCount(this DbResult result, long rowCount)
        {
            result.RowCount = rowCount;
            return result;
        }
    }
}
