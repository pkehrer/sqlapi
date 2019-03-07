using System.Linq;

namespace Core
{
    public class QueryParser
    {
        public string[] ParseQuery(string query)
        {
            return query.Split(';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }
    }
}
