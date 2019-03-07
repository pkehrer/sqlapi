using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Npgsql;

namespace Core
{
    public class PostgresQueryRunner : IQueryRunner
    {
        private readonly NpgsqlConnection _connection;
        const int MaxRowsReturned = 20000;

        public PostgresQueryRunner(NpgsqlConnection connection)
        {
            _connection = connection;
        }
        
        private void AddColumnNames(DbResult result, NpgsqlDataReader reader)
        {
            var names = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                names.Add(reader.GetName(i));
            }
            result.ColumnNames = names.ToArray();
        }

        public Task<DbResult> RunQuery(string query)
        {
            var result = new DbResult();
            try
            {

                using (var cmd = new NpgsqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    var rows = new List<List<object>>();
                    long rowCount = 0;
                    while (reader.Read())
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
            }
            catch (Exception e)
            {
                throw new DbException(e, query);
            }

            return Task.FromResult(result);
        }
    }
}