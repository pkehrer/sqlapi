using Core.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class MysqlQueryRunner : IQueryRunner
    {
        const int MaxRowsReturned = 20000;

        private readonly MySqlConnection _sourceConnection;

        public MysqlQueryRunner(MySqlConnection sourceConnection)
        {
            _sourceConnection = sourceConnection;
        }
        public async Task<DbResult> RunQuery(string query)
        {
            var result = new DbResult();
            try
            {
                using (var cmd = new MySqlCommand(query, _sourceConnection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var rows = new List<List<object>>();
                    long rowCount = 0;
                    while (await reader.ReadAsync())
                    {
                        var row = new List<object>();

                        if (result.ColumnNames.Length == 0)
                        {
                            AddColumnNames(result, reader);
                        }

                        if (rows.Count < MaxRowsReturned)
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.GetValue(i);
                                row.Add(value);
                            }
                            rows.Add(row);
                        }
                        rowCount++;
                    }
                    result.Rows = rows.Select(r => r.ToArray()).ToArray();
                    result.RowCount = rowCount;
                }
            } catch (Exception e)
            {
                throw new DbException(e, query);
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
