using Database.DTOs.ClientCoefsDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ClientCoefsController
    {
        private readonly IClientCoefsRepository _repository;
        public ClientCoefsController(IClientCoefsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateClientCoefs(ClientCoefsDTO clientProgCoefDTO)
        {
            return await _repository.CreateClientCoefs(clientProgCoefDTO);
        }

        public async Task<ClientCoefsDTO> GetClientCoefs(int clid, int schid)
        {
            return await _repository.GetClientCoefs(clid, schid);
        }

        public async Task<bool> UpdateClientCoefs(ClientCoefsDTO clientProgCoefDTO)
        {
            if ((clientProgCoefDTO.progcoef == null || clientProgCoefDTO.progcoef == 1) &&
                (clientProgCoefDTO.coefA == null || clientProgCoefDTO.coefA == 1) && 
                (clientProgCoefDTO.coefB == null || clientProgCoefDTO.coefB == 1))
            {
                return await _repository.DeleteClientCoefs(clientProgCoefDTO.clid, clientProgCoefDTO.schid);
            }
            else
            {
                return await _repository.UpdateClientCoefs(clientProgCoefDTO);
            }
        }
        public async Task<bool> DeleteClientCoefsByClientId(int clid)
        {
            return await _repository.DeleteClientCoefsByClientId(clid);
        }

        public async Task<bool> DeleteClientCoefs(int clid, int schid)
        {
            return await _repository.DeleteClientCoefs(clid, schid);
        }
    }
}
