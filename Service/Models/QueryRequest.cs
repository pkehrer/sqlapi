using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Service.Models
{
    public class QueryRequest
    {
        [BindRequired]
        public string ConnectionId { get; set; }

        [BindRequired]
        public string Query { get; set; }
    }
}
