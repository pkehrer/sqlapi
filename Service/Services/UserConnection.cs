using Core;
using MySql.Data.MySqlClient;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserConnection
    {
        private readonly MySqlConnection _dbConnection;
        private readonly QueryParser _queryParser;
        private Task _dbConnectionTask;

        public UserConnection(MySqlConnection dbConnection, QueryParser queryParser)
        {
            _dbConnection = dbConnection;
            _queryParser = queryParser;
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
        
        public async Task<QueryResponse> RunQuery(string query)
        {
            await DbConnection;
            var runner = new QueryRunner(_dbConnection);
            
            var response = new QueryResponse
            {
                Text = query,
                ResultSets = new List<QueryResponseResultSet>()
            };

            var statements = _queryParser.ParseQuery(query);
            var failure = false;
            foreach (var statement in statements)
            {
                var resultSet = new QueryResponseResultSet
                {
                    Query = statement,
                    Status = ResultSetStatus.NotExecuted
                };

                if (!failure)
                {
                    try
                    {
                        var dbResult = await runner.RunQuery(statement);
                        resultSet.Result = dbResult;
                        resultSet.Status = ResultSetStatus.Success;
                    }
                    catch (Exception e)
                    {
                        resultSet.ErrorMessage = e.ToString();
                        resultSet.Status = ResultSetStatus.Failure;
                    }
                }

                response.ResultSets.Add(resultSet);
            }

            return response;
        }

        public async Task CloseAsync()
        {
            await _dbConnection.CloseAsync();
        }
    }
}
