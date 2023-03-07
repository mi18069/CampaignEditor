using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.SeasonalityDTO;

namespace CampaignEditor.Controllers
{
    public class SeasonalityController : ControllerBase
    {
        private readonly ISeasonalityRepository _repository;
        public SeasonalityController(ISeasonalityRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<SeasonalityDTO> CreateSeasonality(CreateSeasonalityDTO seasonalityDTO)
        {
            await _repository.CreateSeasonality(seasonalityDTO);
            var seasonality = await _repository.GetSeasonalityByName(seasonalityDTO.seasname);
            return seasonality;
        }

        public async Task<SeasonalityDTO> GetSeasonalityById(int id)
        {
            var seasonality = await _repository.GetSeasonalityById(id);
            return seasonality;
        }
        public async Task<SeasonalityDTO> GetSeasonalityByName(string seasonalityname)
        {
            var seasonality = await _repository.GetSeasonalityByName(seasonalityname);
            return seasonality;
        }
        public async Task<IEnumerable<SeasonalityDTO>> GetAllSeasonalitiesByOwnerId(int id)
        {
            return await _repository.GetAllSeasonalitiesByOwnerId(id);
        }

        public async Task<IEnumerable<SeasonalityDTO>> GetAllSeasonalities()
        {
            return await _repository.GetAllSeasonalities();
        }

        public async Task<bool> UpdateSeasonality(UpdateSeasonalityDTO seasonalityDTO)
        {
            return await _repository.UpdateSeasonality(seasonalityDTO);
        }

        public async Task<bool> DeleteSeasonalityById(int id)
        {
            return await _repository.DeleteSeasonalityById(id);
        }
    }
}
