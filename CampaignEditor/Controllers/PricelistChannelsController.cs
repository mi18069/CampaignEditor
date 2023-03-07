using Database.DTOs.PricelistChannels;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class PricelistChannelsController : ControllerBase
    {
        private readonly IPricelistChannelsRepository _repository;
        public PricelistChannelsController(IPricelistChannelsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreatePricelistChannels(CreatePricelistChannelsDTO pricelistChannelsDTO)
        {
            return await _repository.CreatePricelistChannels(pricelistChannelsDTO);
        }

        public async Task<PricelistChannelsDTO> GetPricelistChannelsByIds(int plid, int chid)
        {
            var pricelistChannels = await _repository.GetPricelistChannelsByIds(plid, chid);
            return pricelistChannels;
        }

        public async Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannelsByPlid(int plid)
        {
            return await _repository.GetAllPricelistChannelsByPlid(plid);
        }

        public async Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannelsByChid(int chid)
        {
            return await _repository.GetAllPricelistChannelsByChid(chid);
        }

        public async Task<IEnumerable<int>> GetIntersectedPlIds(IEnumerable<int> plids, IEnumerable<int> chids)
        {
            return await _repository.GetIntersectedPlIds(plids, chids);
        }

        public async Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannels()
        {
            return await _repository.GetAllPricelistChannels();
        }

        public async Task<bool> UpdatePricelistChannels(UpdatePricelistChannelsDTO pricelistChannelsDTO)
        {
            return await _repository.UpdatePricelistChannels(pricelistChannelsDTO);
        }

        public async Task<bool> DeletePricelistChannelsByIds(int plid, int chid)
        {
            return await _repository.DeletePricelistChannelsByIds(plid, chid);
        }
        public async Task<bool> DeleteAllPricelistChannelsByPlid(int plid)
        {
            return await _repository.DeleteAllPricelistChannelsByPlid(plid);
        }
    }
}
