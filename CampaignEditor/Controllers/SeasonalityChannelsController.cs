using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.SeasonalityChannelsDTO;

namespace CampaignEditor.Controllers
{
    public class SeasonalityChannelsController : ControllerBase
    {
        private readonly ISeasonalityChannelsRepository _repository;
        public SeasonalityChannelsController(ISeasonalityChannelsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateSeasonalityChannels(CreateSeasonalityChannelsDTO seasonalityChannelsDTO)
        {
            return await _repository.CreateSeasonalityChannels(seasonalityChannelsDTO);
        }

        public async Task<SeasonalityChannelsDTO> GetSeasonalityChannelsByIds(int seasid, int chid)
        {
            var sectableChannels = await _repository.GetSeasonalityChannelsByIds(seasid, chid);
            return sectableChannels;
        }

        public async Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannelsByPlid(int seasid)
        {
            return await _repository.GetAllSeasonalityChannelsBySeasid(seasid);
        }

        public async Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannelsByChid(int chid)
        {
            return await _repository.GetAllSeasonalityChannelsByChid(chid);
        }

        public async Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannels()
        {
            return await _repository.GetAllSeasonalityChannels();
        }

        public async Task<bool> UpdateSeasonalityChannels(UpdateSeasonalityChannelsDTO seasonalityChannelsDTO)
        {
            return await _repository.UpdateSeasonalityChannels(seasonalityChannelsDTO);
        }

        public async Task<bool> DeleteSeasonalityChannelsByIds(int seasid, int chid)
        {
            return await _repository.DeleteSeasonalityChannelsByIds(seasid, chid);
        }
    }
}
