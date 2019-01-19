using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDataPopulator
{
    class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("db.json").Build();
            var dbconfig = config.GetSection("DbConnectionConfig").Get<DbConnectionConfig>();

            var conn = new AdminConnection(Options.Create(dbconfig));
            var tablePopulator = new TablePopulator(conn);

            Run(tablePopulator, args).Wait();

            Console.ReadKey();
        }

        async static Task Run(TablePopulator tablePopulator, string[] args)
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
