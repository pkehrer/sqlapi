using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class QueryResponse
    {
        public string Text { get; set; }
        public List<QueryResponseResultSet> ResultSets { get; set; }

        public override string ToString()
        {
            var moreThanOne = ResultSets.Count > 1;
            var s = moreThanOne
                ? "Full Text: " + Environment.NewLine + Text + Environment.NewLine
                : string.Empty;

            if (ResultSets.Count == 0)
            {
                return "Full Text: " + Text + Environment.NewLine + "No result sets returned..." + Environment.NewLine;
            }

            foreach (var resultSet in ResultSets)
            {
                s += (moreThanOne ? "Statement: " : "Full Text: ") 
                    + Environment.NewLine + resultSet.Query + Environment.NewLine;
                if (resultSet.Result != null)
                {
                    s += resultSet.Result.ToString();
                }
                if (!string.IsNullOrEmpty(resultSet.ErrorMessage))
                {
                    s += "ERROR: " + resultSet.ErrorMessage + Environment.NewLine;
                }
                if (resultSet.Status == ResultSetStatus.NotExecuted)
                {
                    s += "Statement not executed due to previous failure";
                }
            }
            return s;
        }
    }
}
