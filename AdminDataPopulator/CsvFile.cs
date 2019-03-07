using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDataPopulator
{
    class CsvFile
    {
        readonly string _path;
        public CsvFile(string path)
        {
            _path = path;
            Name = Path.GetFileNameWithoutExtension(path);
        }
        public string Name { get; }
        public CsvFileHeaderLoader Header() => new CsvFileHeaderLoader(_path);
        public CsvFileRowLoader Rows(int cellCount) => new CsvFileRowLoader(_path, cellCount);
    }


    abstract class CsvFileDataLoader: IDisposable
    {
        
        public CsvFileDataLoader(string path)
        {
            Reader = new StreamReader(path);
            Csv = new CsvReader(Reader);
        }

        protected StreamReader Reader { get; }
        protected CsvReader Csv { get; }

        public void Dispose()
        {
            Reader.Dispose();
            Csv.Dispose();
        }
    }

    class CsvFileHeaderLoader : CsvFileDataLoader
    {
        public CsvFileHeaderLoader(string path) : base(path) { }

        public async Task<IList<string>> GetHeader()
        {
            await Csv.ReadAsync();
            var row = new List<string>();
            var index = 0;

            try
            {
                while (true)
                {
                    row.Add(Csv.GetField(typeof(string), index) as string);
                    index++;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return row;
        }

    }

    class CsvFileRowLoader : CsvFileDataLoader
    {
        private IList<object> _currentRow;
        private int _cellCount;

        public CsvFileRowLoader(string path, int cellCount) : base(path)
        {
            _cellCount = cellCount;
            Csv.ReadAsync().Wait();
        }

        public async Task<bool> ReadAsync()
        {
            if (await Csv.ReadAsync())
            {
                LoadRow();
                return true;
            }
            return false;
        }

        private void LoadRow()
        {
            _currentRow = Enumerable.Range(0, _cellCount)
                .Select(i =>
                {
                    var c = Csv.GetField(i);
                    return (object)(c == string.Empty ? null : c);
                }).ToList();
        }

        public IList<object> GetRow()
        {
            if (_currentRow == null)
            {
                throw new Exception("Don't call GetRow() when ReadAsync() returns false!");
            }
            return _currentRow;
        }
    }
}
