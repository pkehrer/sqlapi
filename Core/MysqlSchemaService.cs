using Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class MysqlSchemaService : ISchemaService
    {
        readonly IAdminConnection _conn;
        private IList<TableDefinition> _cachedSchema;

        public MysqlSchemaService(IAdminConnection conn)
        {
            _conn = conn;
        }

        public async Task<IList<TableDefinition>> GetSchema()
        {
            if (_cachedSchema == null)
            {
                var defs = new List<TableDefinition>();
                var tableRes = await _conn.RunAdminQuery("SHOW TABLES;");
                var tableNames = tableRes.Rows.Select(r => r[0].ToString());
                foreach (var tableName in tableNames)
                {
                    var describeRes = await _conn.RunAdminQuery($"DESCRIBE {tableName};");
                    var countRes = await _conn.RunAdminQuery($"SELECT COUNT(1) FROM {tableName};");
                    defs.Add(new TableDefinition
                    {
                        Name = tableName,
                        Columns = describeRes.Rows.Select(row => new ColumnDefinition
                        {
                            Name = row[0].ToString(),
                            NotNull = row[2].ToString() != "YES",
                            PrimaryKey = false,
                            Type = row[1].ToString()
                        }).ToList(),
                        RowCount = (long)countRes.Rows[0][0]
                    });
                }
               _cachedSchema = defs;
            }
            return _cachedSchema;
        }
    }
}
