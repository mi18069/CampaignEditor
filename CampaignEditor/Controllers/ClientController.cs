using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ClientController
    {
        private readonly IClientRepository _repository;
        public ClientController(IClientRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<ClientDTO> CreateClient(CreateClientDTO clientDTO)
        {
            await _repository.CreateClient(clientDTO);
            var client = await _repository.GetClientByName(clientDTO.clname);
            return client;
        }

        public async Task<ClientDTO> GetClientById(int id)
        {
            var client = await _repository.GetClientById(id);
            return client;
        }
        public async Task<ClientDTO> GetClientByName(string clname)
        {
            var client = await _repository.GetClientByName(clname);
            return client;
        }

        public async Task<bool> UpdateClient(UpdateClientDTO clientDTO)
        {
            return await _repository.UpdateClient(clientDTO);
        }

        public async Task<bool> DeleteClientById(int id)
        {
            return await _repository.DeleteClientById(id);
        }
        public async Task<bool> DeleteClientByName(string clname)
        {
            return await _repository.DeleteClientByName(clname);
        }
        public async Task<IEnumerable<ClientDTO>> GetAllClients()
        {
            return await _repository.GetAllClients();
        }
    }
}
