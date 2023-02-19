using Database.DTOs.PricelistDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class PricelistController : ControllerBase
    {
        private readonly IPricelistRepository _repository;
        public PricelistController(IPricelistRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreatePricelist(CreatePricelistDTO pricelistDTO)
        {
            return await _repository.CreatePricelist(pricelistDTO);
        }

        public async Task<PricelistDTO> GetPricelistById(int id)
        {
            var pricelist = await _repository.GetPricelistById(id);
            return pricelist;
        }

        public async Task<IEnumerable<PricelistDTO>> GetAllPricelists()
        {
            return await _repository.GetAllPricelists();
        }

        public async Task<bool> UpdatePricelist(UpdatePricelistDTO pricelistDTO)
        {
            return await _repository.UpdatePricelist(pricelistDTO);
        }

        public async Task<bool> DeletePricelistById(int id)
        {
            return await _repository.DeletePricelistById(id);
        }
    }
}
