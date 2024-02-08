using Database.DTOs.ReachDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ReachController
    {
        private readonly IReachRepository _repository;
        public ReachController(IReachRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<ReachDTO>> GetReachByCmpid(int cmpid)
        {
            return await _repository.GetReachByCmpid(cmpid);
        }

        public async Task<ReachDTO> GetFinalReachByCmpid(int cmpid)
        {
            return await _repository.GetFinalReachByCmpid(cmpid);
        }

        public async Task<bool> DeleteReachByCmpid(int cmpid)
        {
            return await _repository.DeleteReachByCmpid(cmpid);
        }
    }
}
