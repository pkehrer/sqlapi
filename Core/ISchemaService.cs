using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core
{
    public interface ISchemaService
    {
        Task<IList<TableDefinition>> GetSchema();
    }
}