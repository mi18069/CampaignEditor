using Database.DTOs.ClientDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IClientRepository
    {
        Task<bool> CreateClient(CreateClientDTO userDTO);
        Task<ClientDTO> GetClientById(int id);
        Task<ClientDTO> GetClientByName(string clname);
        Task<IEnumerable<ClientDTO>> GetAllClients();
        Task<bool> UpdateClient(UpdateClientDTO userDTO);
        Task<bool> DeleteClientById(int id);
        Task<bool> DeleteClientByName(string clname);
    }
}
