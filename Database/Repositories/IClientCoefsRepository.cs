using Database.DTOs.ClientCoefsDTO;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IClientCoefsRepository
    {
        Task<bool> CreateClientCoefs(ClientCoefsDTO clientCoefsDTO);
        Task<ClientCoefsDTO> GetClientCoefs(int clid, int schid);
        Task<bool> UpdateClientCoefs(ClientCoefsDTO clientCoefsDTO);
        Task<bool> DeleteClientCoefs(int clid, int schid);
        Task<bool> DeleteClientCoefsByClientId(int clid);
    }
}
