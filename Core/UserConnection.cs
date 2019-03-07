using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public abstract class UserConnection : IUserConnection
    {
        private readonly QueryParser _queryParser;
        
        protected UserConnection(QueryParser parser)
        {
            _queryParser = parser;
        }
        
        public abstract Task CloseAsync();
        
        protected abstract Task OpenConnection();
        
        protected abstract IQueryRunner QueryRunner { get; }
        
        public async Task<QueryResponse> RunQuery(string query)
        {
            await OpenConnection();
            var runner = QueryRunner;
            
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
                        failure = true;
                    }
                }

                response.ResultSets.Add(resultSet);
            }

            return response;
        }
    }
}