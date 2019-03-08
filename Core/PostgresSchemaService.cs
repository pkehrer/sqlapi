using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public class PostgresSchemaService : ISchemaService
    {
        private readonly IAdminConnection _conn;

        public PostgresSchemaService(IAdminConnection conn)
        {
            _conn = conn;
        }
        
        public async Task<IList<TableDefinition>> GetSchema()
        {
            var tables =
                await _conn.RunAdminQuery("select * from information_schema.tables where table_schema = 'public'");
            var columns =
                await _conn.RunAdminQuery("select * from information_schema.columns where table_schema = 'public'");

            var tableNames = tables.Rows.Select(r => r[2].ToString());

            var tableDefinitions = new List<TableDefinition>();
            foreach (var tableName in tableNames)
            {
                var countResponse = await _conn.RunAdminQuery($"select count(1) from public.{tableName}");
                var count = countResponse.Rows[0][0];

                tableDefinitions.Add(new TableDefinition
                {
                    Name = tableName,
                    RowCount = Convert.ToInt64(count),
                    Columns = columns.Rows.Where(r => r[2].ToString() == tableName)
                        .Select(r => new ColumnDefinition
                        {
                            Name = r[3].ToString(),
                            NotNull = r[6].ToString() == "NO",
                            PrimaryKey = false,
                            Type = r[7].ToString()
                        })
                        .ToList()
                });
            }

            return tableDefinitions;
        }
    }
}