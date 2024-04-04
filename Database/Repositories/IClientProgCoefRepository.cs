using Database.DTOs.ClientProgCoefDTO;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IClientProgCoefRepository
    {
        Task<bool> CreateClientProgCoef(ClientProgCoefDTO clientProgCoefDTO);
        Task<ClientProgCoefDTO> GetClientProgCoef(int clid, int schid);
        Task<bool> UpdateClientProgCoef(ClientProgCoefDTO clientProgCoefDTO);
        Task<bool> DeleteClientProgCoef(int clid, int schid);
        Task<bool> DeleteClientProgCoefByClientId(int clid);
    }
}
