using Database.DTOs.ClientProgCoefDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ClientProgCoefController
    {
        private readonly IClientProgCoefRepository _repository;
        public ClientProgCoefController(IClientProgCoefRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateClientProgCoef(ClientProgCoefDTO clientProgCoefDTO)
        {
            return await _repository.CreateClientProgCoef(clientProgCoefDTO);
        }

        public async Task<ClientProgCoefDTO> GetClientProgCoef(int clid, int schid)
        {
            return await _repository.GetClientProgCoef(clid, schid);
        }

        public async Task<bool> UpdateClientProgCoef(ClientProgCoefDTO clientProgCoefDTO)
        {
            return await _repository.UpdateClientProgCoef(clientProgCoefDTO);
        }
        public async Task<bool> DeleteClientProgCoefByClientId(int clid)
        {
            return await _repository.DeleteClientProgCoefByClientId(clid);
        }

        public async Task<bool> DeleteClientProgCoef(int clid, int schid)
        {
            return await _repository.DeleteClientProgCoef(clid, schid);
        }
    }
}
