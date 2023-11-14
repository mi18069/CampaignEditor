using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.EmsTypesDTO;
using Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IEmsTypesRepository
    {
        Task<EmsTypesDTO> GetEmsTypesByCode(string code);
        Task<IEnumerable<EmsTypesDTO>> GetAllEmsTypes();
        Task<IEnumerable<EmsTypes>> GetAllEmsTypesEntities();
    }
}
