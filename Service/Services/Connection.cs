using MySql.Data.MySqlClient;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Service.Services
{
    public class Connection : IDisposable
    {
        readonly MySqlConnection _dbConnection;
        private Task _dbConnectionTask;

        public Connection(MySqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private Task DbConnection
        {
            get
            {
                if (_dbConnectionTask == null)
                {
                    _dbConnectionTask = _dbConnection.OpenAsync();
                }
                return _dbConnectionTask;
            }
        }
        
        public async Task<DbResult> RunQuery(string query)
        {
            await DbConnection;
            var result = new DbResult();
            using (var cmd = new MySqlCommand(query, _dbConnection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while(await reader.ReadAsync())
                {
                    if (result.ColumnNames == null)
                    {
                        result.ColumnNames = ColumnNames(reader);
                    }

                    var row = new List<object>();
                    for(var i = 0; i < reader.FieldCount; i++)
                    {    
                        var value = reader.GetValue(i);
                        row.Add(value);
                    }
                    result.Rows.Add(row);
                }
            }
            return result;
        }

        private List<string> ColumnNames(DbDataReader reader)
        {
            var names = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                names.Add(reader.GetName(i));
            }
            return names;
        }

        public void Dispose()
        {
            _dbConnection.Dispose();
        }
    }
}
