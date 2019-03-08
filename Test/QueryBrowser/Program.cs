using System;
using System.Threading.Tasks;
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global

namespace QueryBrowser
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("db.json").Build();
            var dbConfig = config.GetSection("DbConnectionConfig").Get<DbConnectionConfig>();

            var conn = new PostgresAdminConnection(Options.Create(dbConfig));
            Console.CancelKeyPress += (o, eArgs) => { Environment.Exit(0); };
            Task.Run(async () =>
            {
                while (true)
                {
                    var query = string.Empty;
                    try
                    {
                        Console.Write("[Query: > ");
                        query = Console.ReadLine();
                        var result = await conn.RunAdminQuery(query);
                        Console.WriteLine(result);
                        Console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error running: " + query);
                        Console.WriteLine(e);
                    }
                }
            }).Wait();
        }
    }
}