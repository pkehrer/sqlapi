using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDataPopulator
{
    class SqlGenerator
    {
        const int MYSQL_MAX_PLACEHOLDERS = 65535;

        private IList<IList<object>> _currentRows;
        private string _currentInsert;
        private IList<string> _header;
        private int _insertBatchSize;
        private IList<string> Header
        {
            get { return _header; }
            set
            {
                _header = value;
                _insertBatchSize = (MYSQL_MAX_PLACEHOLDERS - 100) / value.Count;
            }
        }

        public string Drop(CsvFile file)
        {
            return $"DROP TABLE {file.Name};";
        }

        public async Task<bool> ReadInsertAsync(
            string name,
            CsvFileHeaderLoader headerReader,
            CsvFileRowLoader rowReader)
        {
            if (Header == null) Header = await headerReader.GetHeader();
            _currentRows = new List<IList<object>>();
            for (var i = 0; i < _insertBatchSize; i++)
            {
                if (await rowReader.ReadAsync())
                {
                    _currentRows.Add(rowReader.GetRow());
                }
                else if (_currentRows.Count > 0)
                {
                    LoadInsert(name);
                    return true;
                } else
                {
                    _currentRows = null;
                    return false;
                }
            }
            LoadInsert(name);
            return true;
        }

        private void LoadInsert(string name)
        {
            var rowData = _currentRows.Select(row => $"({string.Join(',', row.Select(FormatData))})");
            _currentInsert = $@"INSERT INTO {name} VALUES {string.Join(',', rowData)};";
        }

        public string GetInsert()
        {
            if (_currentInsert == null)
            {
                throw new Exception("Don't call GetInsert() when ReadInsertAsync() returns false!");
            }
            return _currentInsert;
        }

        private string FormatData(object cell)
        {
            if (cell == null) return "NULL";
            var type = cell.GetType();
            if (type == typeof(string)) return $"'{Sanitize(cell)}'";
            return cell.ToString();
        }

        private string Sanitize(object cell)
        {
            return cell.ToString()
                .Replace("'", "''")
                .Replace(@"\", @"\\");
        }

        private async Task<IList<object>> FindFullRow(CsvFile file, int cellCount)
        {
            using (var rows = file.Rows(cellCount))
            {
                while (await rows.ReadAsync())
                {
                    var row = rows.GetRow();
                    if (row.All(c => c != null))
                    {
                        return row;
                    }
                }
                return null;
            }
        }

        public async Task<string> Create(CsvFile file)
        {
            using (var headerReader = file.Header())
            {
                var header = await headerReader.GetHeader();
                var fullRow = await FindFullRow(file, header.Count);
                var types = fullRow.Select(c => c.GetType()).ToList();
                var columnDefinitions = new List<string>();

                for (var i = 0; i < header.Count; i++)
                {
                    columnDefinitions.Add(ColumnDefinition(types[i], header[i]));
                }

                return $@"DROP TABLE IF EXISTS {file.Name};
                    CREATE TABLE {file.Name} ({string.Join(',', columnDefinitions)});";
            }
        }

        private string ColumnDefinition(Type type, string name)
        {
            var types = new Dictionary<Type, string>
            {
                { typeof(string), "VARCHAR(50)" }
            };

            var sqlType = types[type];
            return $"{name.Replace(' ', '_')} {sqlType}";
        }
    }
}
