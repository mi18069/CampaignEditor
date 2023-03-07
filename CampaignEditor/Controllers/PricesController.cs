using Database.DTOs.PricesDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class PricesController : ControllerBase
    {
        private readonly IPricesRepository _repository;
        public PricesController(IPricesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreatePrices(CreatePricesDTO pricesDTO)
        {
            return await _repository.CreatePrices(pricesDTO);
        }
        public async Task<PricesDTO> GetPricesById(int id)
        {
            return await _repository.GetPricesById(id);
        }
        public async Task<IEnumerable<PricesDTO>> GetAllPricesByPlId(int id)
        {
            return await _repository.GetAllPricesByPlId(id);
        }
        public async Task<IEnumerable<PricesDTO>> GetAllPrices()
        {
            return await _repository.GetAllPrices();

        }
        public async Task<bool> UpdatePrices(UpdatePricesDTO pricesDTO)
        {
            return await _repository.UpdatePrices(pricesDTO);
        }
        public async Task<bool> DeletePricesById(int id)
        {
            return await _repository.DeletePricesById(id);

        }
    }
}
