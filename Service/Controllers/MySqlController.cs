using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Core.Models;
using Core;

namespace Service.Controllers
{
    [Route("")]
    [Produces("application/json")]
    [ValidateModel]
    [ApiController]
    public class MySqlController : ControllerBase
    {
        private readonly UserConnectionManager _connectionManager;
        private readonly SchemaService _schemaSvc;

        public MySqlController(
            UserConnectionManager connectionManager,
            SchemaService schemaSvc)
        {
            _connectionManager = connectionManager;
            _schemaSvc = schemaSvc;
        }
        
        [HttpPost("connection")]
        public async Task<ActionResult<ConnectionResponse>> OpenConnection()
        {
            return await _connectionManager.OpenNewConnection();
        }

        [HttpDelete("connection/{connectionId}")]
        public async Task<ActionResult> CloseConnection([FromRoute] string connectionId)
        {
            await _connectionManager.CloseConnection(connectionId);
            return NoContent();
        }

        [HttpPost("query")]
        public async Task<ActionResult<QueryResponse>> RunQuery([FromBody] QueryRequest request)
        {
            return await _connectionManager.RunQuery(request.ConnectionId, request.Query);
        }

        [HttpGet("schema")]
        public async Task<ActionResult<List<TableDefinition>>> GetSchema()
        {
            return (await _schemaSvc.GetSchema()).ToList();
        }


    }
}
