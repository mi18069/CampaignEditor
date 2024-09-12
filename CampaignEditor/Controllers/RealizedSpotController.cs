using Database.DTOs.RealizedSpotDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class RealizedSpotController
    {
        private readonly IRealizedSpotRepository _repository;
        public RealizedSpotController(IRealizedSpotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RealizedSpotDTO> GetRealizedSpot(int spotnum)
        {
            return await _repository.GetRealizedSpot(spotnum);
        }
    }
}
