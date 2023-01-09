using CampaignEditor.DTOs.UserDTO;
using Database.DTOs.UserClients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IUserClientsRepository
    {
        Task<bool> CreateUserClients(UserClientsDTO userClientsDTO);
        Task<UserClientsDTO> GetUserClientsByUserId(int id);
        Task<UserClientsDTO> GetUserClientsByClientId(int id);
        Task<IEnumerable<UserClientsDTO>> GetAllUserClients();
        Task<IEnumerable<UserClientsDTO>> GetAllUserClientsByUserId(int id);
        Task<IEnumerable<UserClientsDTO>> GetAllUserClientsByClientId(int id);
        Task<bool> DeleteUserClientsByUserId(int id);
        Task<bool> DeleteUserClientsByClientId(int id);
    }
}
