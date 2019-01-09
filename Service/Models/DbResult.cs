using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class DbResult
    {
        public DbResult()
        {
            Rows = new List<List<object>>();
        }

        public List<string> ColumnNames { get; set; }
        public List<List<object>> Rows { get; }

        public override string ToString()
        {
            var s = string.Join('\t', ColumnNames.ToArray());
            s += Environment.NewLine;
            foreach (var row in Rows)
            {
                s += string.Join('\t', row.ToArray());
            }
            return s;
        }
    }
}
