using System;

namespace Core.Models
{
    public class DbResult
    {
        public DbResult()
        {
            ColumnNames = new string[0];
            Rows = new object[0][];
        }

        public string[] ColumnNames { get; set; }
        public object[][] Rows { get; set; }

        public override string ToString()
        {
            const string emptyResponse = "Successful execution returned no response.";
            if (ColumnNames.Length == 0)
            {
                return emptyResponse + Environment.NewLine;
            }

            var s = string.Join('\t', ColumnNames);
            s += Environment.NewLine;
            foreach (var row in Rows)
            {
                s += string.Join('\t', row);
                s += Environment.NewLine;
            }
            return s;
        }
    }
}
