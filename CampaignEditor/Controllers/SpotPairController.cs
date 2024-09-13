using Database.DTOs.SpotPairDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class SpotPairController
    {
        private readonly ISpotPairRepository _repository;
        public SpotPairController(ISpotPairRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateSpotPair(CreateSpotPairDTO spotPairDTO)
            => await _repository.CreateSpotPair(spotPairDTO);
        public async Task<SpotPairDTO> GetSpotPairBySpotnum(int cmpid, int spotnum)
            => await _repository.GetSpotPairBySpotnum(cmpid, spotnum);

        public async Task<IEnumerable<SpotPairDTO>> GetAllCampaignSpotPairs(int cmpid)
            => await _repository.GetAllCampaignSpotPairs(cmpid);

        public async Task<bool> UpdateSpotPair(UpdateSpotPairDTO spotPairDTO)
            => await _repository.UpdateSpotPair(spotPairDTO);
        public async Task<bool> DeleteSpotcodeFromSpotPairs(int cmpid, string spotcode)
            => await _repository.DeleteSpotcodeFromSpotPairs(cmpid, spotcode);

        public async Task<bool> DeleteAllCampaignSpotPairs(int cmpid)
            => await _repository.DeleteAllCampaignSpotPairs(cmpid);
    }
}
