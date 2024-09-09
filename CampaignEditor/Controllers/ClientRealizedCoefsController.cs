using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ClientRealizedCoefsController
    {
        private readonly IClientRealizedCoefsRepository _repository;
        public ClientRealizedCoefsController(IClientRealizedCoefsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateOrUpdateRealizedCoefs(ClientRealizedCoefs coefs)
        {
            if (await _repository.GetRealizedCoefs(coefs.clid, coefs.emsnum) != null)
                return await _repository.UpdateRealizedCoefs(coefs);
            else
                return await _repository.CreateRealizedCoefs(coefs);
        }
        public async Task<bool> CreateRealizedCoefs(ClientRealizedCoefs coefs)
        {
            return await _repository.CreateRealizedCoefs(coefs);
        }

        public async Task<ClientRealizedCoefs> GetRealizedCoefs(int clid, int emsnum)
        {
            return await _repository.GetRealizedCoefs(clid, emsnum);
        }
        public async Task<IEnumerable<ClientRealizedCoefs>> GetAllClientRealizedCoefs(int clid)
        {
            return await _repository.GetAllClientRealizedCoefs(clid);
        }

        public async Task<bool> UpdateRealizedCoefs(ClientRealizedCoefs coefs)
        {
            return await _repository.UpdateRealizedCoefs(coefs);
        }

        public async Task<bool> DeleteRealizedCoefs(int clid, int emsnum)
        {
            return await _repository.DeleteRealizedCoefs(clid, emsnum);
        }
    }
}
