using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
