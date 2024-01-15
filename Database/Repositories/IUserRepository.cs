using CampaignEditor.DTOs.UserDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Repositories
{
    public interface IUserRepository
    {
        Task<int?> CreateUser(CreateUserDTO userDTO);
        Task<UserDTO> GetUserById(int id);
        Task<UserDTO> GetUserByUsername(string username);
        Task<IEnumerable<UserDTO>> GetAllUsers();
        Task<IEnumerable<string>> GetAllUsernames();
        Task<bool> UpdateUser(UpdateUserDTO userDTO);
        Task<bool> DeleteUserById(int id);
        Task<bool> DeleteUserByUsername(string username);
        Task<bool> CheckCredentials(string username, string password);
 
    }
}
