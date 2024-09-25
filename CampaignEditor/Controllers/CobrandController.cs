using Database.DTOs.CobrandDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class CobrandController
    {
        private readonly ICobrandRepository _repository;
        public CobrandController(ICobrandRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateCobrand(CreateCobrandDTO cobrandDTO)
        => await _repository.CreateCobrand(cobrandDTO);
        public async Task<IEnumerable<CobrandDTO>> GetAllCampaignCobrands(int cmpid)
        => await _repository.GetAllCampaignCobrands(cmpid);

        public async Task<IEnumerable<CobrandDTO>> GetAllChannelCobrands(int cmpid, int chid)
        => await _repository.GetAllChannelCobrands(cmpid, chid);

        public async Task<bool> UpdateCobrand(UpdateCobrandDTO cobrandDTO)
        => await _repository.UpdateCobrand(cobrandDTO);

        public async Task<bool> DeleteCobrand(int cmpid, int chid, char spotcode)
        => await _repository.DeleteCobrand(cmpid, chid, spotcode);

        public async Task<bool> DeleteCobrandsForChannel(int cmpid, int chid)
        => await _repository.DeleteCobrandsForChannel(cmpid, chid);
        public async Task<bool> DeleteCampaignCobrands(int cmpid)
        => await _repository.DeleteCampaignCobrands(cmpid);
    }
}
