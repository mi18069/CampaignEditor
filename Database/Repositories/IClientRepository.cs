using Database.DTOs.ClientDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IClientRepository
    {
        Task<bool> CreateClient(CreateClientDTO clientDTO);
        Task<ClientDTO> GetClientById(int id);
        Task<ClientDTO> GetClientByName(string clname);
        Task<IEnumerable<ClientDTO>> GetAllClients();
        Task<bool> UpdateClient(UpdateClientDTO clientDTO);
        Task<bool> DeleteClientById(int id);
        Task<bool> DeleteClientByName(string clname);
    }
}
