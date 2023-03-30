using Database.DTOs.SchemaDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISchemaRepository
    {
        Task<bool> CreateSchema(CreateSchemaDTO schemaDTO);
        Task<SchemaDTO> GetSchemaById(int id);
        Task<SchemaDTO> GetSchemaByName(string name);
        Task<IEnumerable<SchemaDTO>> GetAllSchemas();
        Task<IEnumerable<SchemaDTO>> GetAllChannelSchemas(int chid);
        Task<bool> UpdateSchema(UpdateSchemaDTO schemaDTO);
        Task<bool> DeleteSchemaById(int id);
    }
}
