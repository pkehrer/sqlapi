using MySql.Data.MySqlClient;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class QueryRunner
    {
        private readonly MySqlConnection _sourceConnection;

        public QueryRunner(MySqlConnection sourceConnection)
        {
            _sourceConnection = sourceConnection;
        }
        public async Task<DbResult> RunQuery(string query)
        {
            var result = new DbResult();
            using (var cmd = new MySqlCommand(query, _sourceConnection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                var rows = new List<List<object>>();
                while (await reader.ReadAsync())
                {
                    var row = new List<object>();

                    if (result.ColumnNames.Length == 0)
                    {
                        AddColumnNames(result, reader);
                    }

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        row.Add(value);
                    }
                    rows.Add(row);
                }
                result.Rows = rows.Select(r => r.ToArray()).ToArray();
            }
            return result;
        }

        private void AddColumnNames(DbResult result, DbDataReader reader)
        {
            var names = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                names.Add(reader.GetName(i));
            }
            result.ColumnNames = names.ToArray();
        }
    }
}
