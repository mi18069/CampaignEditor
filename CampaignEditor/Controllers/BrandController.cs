using Database.DTOs.BrandDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class BrandController
    {
        private readonly IBrandRepository _repository;
        public BrandController(IBrandRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateBrand(CreateBrandDTO brandDTO)
        {
            return await _repository.CreateBrand(brandDTO);
        }

        public async Task<BrandDTO> GetBrandById(int id)
        {
            return await _repository.GetBrandById(id);
        }

        public async Task<BrandDTO> GetBrandByName(string brandname)
        {
            return await _repository.GetBrandByName(brandname);
        }

        public async Task<IEnumerable<BrandDTO>> GetAllBrands()
        {
            return await _repository.GetAllBrands();
        }

        public async Task<bool> UpdateBrand(UpdateBrandDTO brandDTO)
        {
            return await _repository.UpdateBrand(brandDTO);
        }

        public async Task<bool> DeleteBrandById(int id)
        {
            return await _repository.DeleteBrandById(id);
        }
    }
}
