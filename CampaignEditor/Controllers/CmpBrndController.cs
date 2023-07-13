using Database.DTOs.CmpBrndDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    class CmpBrndController
    {
        private readonly ICmpBrndRepository _repository;
        public CmpBrndController(ICmpBrndRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateCmpBrnd(CmpBrndDTO cmpbrndDTO)
        {
            return await _repository.CreateCmpBrnd(cmpbrndDTO);
        }

        public async Task<CmpBrndDTO> GetCmpBrndByCmpId(int id)
        {
            return await _repository.GetCmpBrndByCmpId(id);
        }

        public async Task<bool> UpdateBrand(CmpBrndDTO cmpbrndDTO)
        {
            return await _repository.UpdateBrand(cmpbrndDTO);
        }

        public async Task<bool> DeleteBrandByCmpId(int id)
        {
            return await _repository.DeleteBrandByCmpId(id);
        }
    }
}
