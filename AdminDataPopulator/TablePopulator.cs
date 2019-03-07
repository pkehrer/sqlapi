using Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminDataPopulator
{
    class TablePopulator
    {
        readonly MysqlAdminConnection _conn;
        private SqlGenerator _sql;

        public TablePopulator(MysqlAdminConnection conn)
        {
            _conn = conn;
        }

        public async Task Create(CsvFile file)
        {
            //await _conn.RunAdminQuery("SET GLOBAL max_allowed_packet=1073741824;");
            _sql = new SqlGenerator();

            Console.WriteLine($"Creating table {file.Name}");
            var createSql = await _sql.Create(file);
            await _conn.RunAdminQuery(createSql);

            const int insertBatchSize = 5;

            var rowsCreated = 0;
            using (var headerReader = file.Header())
            {
                var header = await headerReader.GetHeader();
                using (var rowReader = file.Rows(header.Count))
                {
                    var rowsReturned = await _sql.ReadInsertAsync(file.Name, headerReader, rowReader);
                    var inserts = new List<string>();
                    while (rowsReturned > 0)
                    {
                        var insert = _sql.GetInsert();
                        if (inserts.Count >= insertBatchSize)
                        {
                            await _conn.RunAdminQuery(string.Join("\n", inserts));
                            Console.WriteLine($"Created {rowsCreated} rows...");
                            inserts = new List<string>();
                        }
                        inserts.Add(insert);
                        rowsCreated += rowsReturned;
                        rowsReturned = await _sql.ReadInsertAsync(file.Name, headerReader, rowReader);
                    }

                    if (inserts.Count > 0)
                    {
                        await _conn.RunAdminQuery(string.Join("\n", inserts));
                        Console.WriteLine($"Created {rowsCreated} rows...");
                    }
                }
            }

            var result = await _conn.RunAdminQuery($"select * from {file.Name} limit 10;");
            Console.WriteLine(result);
        }

        public async Task Drop(CsvFile file)
        {
            Console.WriteLine($"Dropping table {file.Name}");
            await _conn.RunAdminQuery(_sql.Drop(file));
        }
    }
}
