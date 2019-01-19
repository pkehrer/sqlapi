using Core.Models;

namespace Service.Models
{
    public class QueryResponseResultSet
    {
        public string Query { get; set; }
        public ResultSetStatus Status { get; set; }
        public DbResult Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}