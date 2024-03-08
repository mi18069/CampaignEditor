using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.ClientDTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;
        public UserController(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<UserDTO?> CreateUser(CreateUserDTO userDTO)
        {
            var id = await _repository.CreateUser(userDTO);
            if (!id.HasValue)
            {
                return null;
            }
            return await _repository.GetUserById(id.Value);
        }

        public async Task<UserDTO> GetUserById(int id)
        {
            var user = await _repository.GetUserById(id);
            return user;
        }
        public async Task<UserDTO> GetUserByUsername(string username)
        {
            var user = await _repository.GetUserByUsername(username);
            return user;
        }
        public async Task<IEnumerable<UserDTO>> GetAllUsers()
        {
            return await _repository.GetAllUsers();
        }

        public async Task<IEnumerable<string>> GetAllUsernames()
        {
            return await _repository.GetAllUsernames();
        }

        public async Task<bool> UpdateUser(UpdateUserDTO userDTO)
        {
            return await _repository.UpdateUser(userDTO);
        }

        public async Task<bool> DeleteUserById(int id)
        {
            return await _repository.DeleteUserById(id);
        }
        public async Task<bool> DeleteUserByUsername(string username)
        {
            return await _repository.DeleteUserByUsername(username);
        }

        public async Task<bool> CheckCredentials(string username, string password)
        {
            return await _repository.CheckCredentials(username, password);
        }
    }
}
