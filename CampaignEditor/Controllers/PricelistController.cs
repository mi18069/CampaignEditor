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

        public async Task<PricelistDTO> CreatePricelist(CreatePricelistDTO pricelistDTO)
        {
            var newId =  await _repository.CreatePricelist(pricelistDTO);
            if (newId != -1)
            {
                return await _repository.GetPricelistById(newId);
            }

            return null;
        }

        public async Task<PricelistDTO> GetPricelistById(int id)
        {
            var pricelist = await _repository.GetPricelistById(id);
            return pricelist;
        }
        public async Task<PricelistDTO> GetPricelistByName(string pricelistname)
        {
            var pricelist = await _repository.GetPricelistByName(pricelistname);
            return pricelist;
        }
        public async Task<PricelistDTO> GetClientPricelistByName(int clid, string pricelistname)
        {
            var pricelist = await _repository.GetClientPricelistByName(clid, pricelistname);
            return pricelist;
        }

        public async Task<IEnumerable<PricelistDTO>> GetAllPricelists()
        {
            return await _repository.GetAllPricelists();
        }
        public async Task<IEnumerable<PricelistDTO>> GetAllClientPricelists(int clid)
        {
            return await _repository.GetAllClientPricelists(clid);
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
