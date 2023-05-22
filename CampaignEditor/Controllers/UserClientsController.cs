using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.UserClients;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class UserClientsController
    {
        private readonly IUserClientsRepository _repository;
        public UserClientsController(IUserClientsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<UserClientsDTO> CreateUserClients(UserClientsDTO userClientsDTO)
        {
                await _repository.CreateUserClients(userClientsDTO);
                var userClient = await _repository.GetUserClientsByUserId(userClientsDTO.usrid);
                return userClient;
        }
        public async Task<UserClientsDTO> GetUserByUserId(int id)
        {
            var userClient = await _repository.GetUserClientsByUserId(id);
            return userClient;
        }
        public async Task<UserClientsDTO> GetUserByClientId(int id)
        {
            var userClient = await _repository.GetUserClientsByClientId(id);
            return userClient;
        }
        public async Task<IEnumerable<UserClientsDTO>> GetAllUserClients()
        {
            return await _repository.GetAllUserClients();
        }
        public async Task<IEnumerable<UserClientsDTO>> GetAllUserClientsByUserId(int id)
        {
            return await _repository.GetAllUserClientsByUserId(id);
        }
        public async Task<IEnumerable<UserClientsDTO>> GetAllUserClientsByClientId(int id)
        {
            return await _repository.GetAllUserClientsByClientId(id);
        }
        public async Task<bool> DeleteUserClientsByClientId(int id)
        {
            return await _repository.DeleteUserClientsByClientId(id);
        }
        public async Task<bool> DeleteUserClientsByUserId(int id)
        {
            return await _repository.DeleteUserClientsByUserId(id);
        }

        public async Task<bool> DeleteUserClients(int usrid, int clid)
        {
            return await _repository.DeleteUserClients(usrid, clid);
        }

    }
}
