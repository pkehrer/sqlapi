using Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Service.IntegrationTests
{
    public class MySqlControllerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;
        private Func<Task<SqlRunner>> _getRunner;

        public MySqlControllerTests(ITestOutputHelper output)
        {
            _output = output;

            //var builder = new WebHostBuilder()
            //    .UseStartup<Startup>()
            //    .UseContentRoot(Directory.GetCurrentDirectory())
            //    .UseConfiguration(new ConfigurationBuilder()
            //        .SetBasePath(Directory.GetCurrentDirectory())
            //        .AddJsonFile("db.json")
            //        .Build());
            //var server = new TestServer(builder);
            //_client = server.CreateClient();

            _client = new HttpClient
            {
                BaseAddress = new Uri("http://sqlapi.pkehrer.click")
            };

            _getRunner = () => SqlRunner.Create(_client, output);
        }

        [Fact]
        public async Task CreateTableInsertSelect()
        {
            using (var r = await _getRunner())
            {
                await r.Run("CREATE TABLE TMP (id INT, name VARCHAR(100), age INT)");
                await r.Run("INSERT INTO TMP VALUES (1, 'Paul', 32), (2, 'Kristen', 36)");

                var selectResult = await r.Run("SELECT * FROM TMP");

                selectResult.ResultSets.First()
                    .Result
                    .AssertEquivalentTo(new DbResult()
                    .SetColumnNames("id", "name", "age")
                    .AddRow((Int64)1, "Paul", (Int64)32)
                    .AddRow((Int64)2, "Kristen", (Int64)36));
            }
        }
        
        [Fact]
        public async Task ShowTables()
        {
            using (var r = await _getRunner())
            {
                await r.Run("create table table1(id int)");
                await r.Run("create table table2(id int)");
                var showRes = await r.Run("show tables;");
                var expectedResult = DbResultBuilder.Create()
                    .AddRow("table1")
                    .AddRow("table2");
                showRes.ResultSets.First()
                    .Result.Rows
                    .AssertEquivalentTo(expectedResult.Rows);
            }
            Assert.True(true);
        }

        [Fact]
        public async Task MultiStatement()
        {
            using (var r = await _getRunner())
            {
                await r.Run("CREATE TABLE TMP (id INT, name VARCHAR(100), age INT)");
                await r.Run("INSERT INTO TMP VALUES (1, 'Paul', 32), (2, 'Kristen', 36)");

                var showRes = await r.Run(@"SELECT id from TMP;
                                            SELECT name from TMP;");

                var res1 = new DbResult()
                    .SetColumnNames("id")
                    .AddRow((Int64)1)
                    .AddRow((Int64)2);
                var res2 = new DbResult()
                    .SetColumnNames("name")
                    .AddRow("Paul")
                    .AddRow("Kristen");

                showRes.ResultSets[0].Result.AssertEquivalentTo(res1);
                showRes.ResultSets[1].Result.AssertEquivalentTo(res2);
            }
        }

        [Fact]
        public async Task GetSchema()
        {
            var response = await _client.GetAsync("/schema");
            var schema = await response.Content.ReadAsAsync<IList<TableDefinition>>();
            foreach (var table in schema)
            {
                _output.WriteLine(table.ToString());
                _output.WriteLine(string.Empty);
            }
            
            Assert.True(true);
        }
    }
}
