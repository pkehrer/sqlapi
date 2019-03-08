using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable ClassNeverInstantiated.Global

namespace AdminDataPopulator
{
    class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("db.json").Build();
            var dbConfig = config.GetSection("DbConnectionConfig").Get<DbConnectionConfig>();

            var conn = new MysqlAdminConnection(Options.Create(dbConfig));
            var tablePopulator = new MysqlTablePopulator(conn);

            Run(tablePopulator).Wait();

            Console.ReadKey();
        }

        private static async Task Run(MysqlTablePopulator tablePopulator)
        {
            try
            {
                var csvFiles = Directory.EnumerateFiles("csvs")
                    .Select(path => new CsvFile(path))
                    .ToList();
                foreach (var csv in csvFiles)
                {
                    await tablePopulator.Create(csv);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
