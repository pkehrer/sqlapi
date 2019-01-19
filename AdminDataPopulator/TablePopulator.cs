using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdminDataPopulator
{
    class TablePopulator
    {
        readonly AdminConnection _conn;
        private SqlGenerator _sql;

        public TablePopulator(AdminConnection conn)
        {
            _conn = conn;
        }

        public async Task Create(CsvFile file)
        {
            _sql = new SqlGenerator();

            Console.WriteLine($"Creating table {file.Name}");
            var createSql = await _sql.Create(file);
            await _conn.RunAdminQuery(createSql);

            using (var headerReader = file.Header())
            {
                var header = await headerReader.GetHeader();
                using (var rowReader = file.Rows(header.Count))
                {
                    while (await _sql.ReadInsertAsync(file.Name, headerReader, rowReader))
                    {
                        var insert = _sql.GetInsert();
                        await _conn.RunAdminQuery(insert);
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
