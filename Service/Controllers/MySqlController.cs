using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using Service.Models;

namespace Service.Controllers
{
    [Route("")]
    [Produces("application/json")]
    [ValidateModel]
    [ApiController]
    public class MySqlController : ControllerBase
    {
        private readonly UserConnectionManager _connectionManager;
        public MySqlController(UserConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
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
            return await _connectionManager.RunQuery(request);
        }


    }
}
