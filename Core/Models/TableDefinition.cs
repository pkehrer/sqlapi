using System.Collections.Generic;
using System.Linq;

namespace Core.Models
{
    public class TableDefinition
    {
        public string Name { get; set; }
        public long RowCount { get; set; }
        public IList<ColumnDefinition> Columns { get; set; }

        public override string ToString()
        {
            return $@"Table: {Name} - Rows: {RowCount}
Columns:
  {string.Join("\n  ", Columns.Select(c => $"{c.Name}\t{c.Type}\t{c.NotNull}\t{c.PrimaryKey}"))}";
        }
    }

    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool NotNull { get; set; }
        public bool PrimaryKey { get; set; }
    }
}
