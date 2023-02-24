using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.SeasonalitiesDTO;

namespace CampaignEditor.Controllers
{
    public class SeasonalitiesController : ControllerBase
    {
        private readonly ISeasonalitiesRepository _repository;
        public SeasonalitiesController(ISeasonalitiesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateSeasonalities(CreateSeasonalitiesDTO seasonalitiesDTO)
        {
            return await _repository.CreateSeasonalities(seasonalitiesDTO);
        }

        public async Task<IEnumerable<SeasonalitiesDTO>> GetSeasonalitiesById(int id)
        {
            var seasonalities = await _repository.GetSeasonalitiesById(id);
            return seasonalities;
        }

        public async Task<IEnumerable<SeasonalitiesDTO>> GetAllSeasonalities()
        {
            return await _repository.GetAllSeasonalities();
        }

        public async Task<bool> UpdateSeasonalities(UpdateSeasonalitiesDTO seasonalitiesDTO)
        {
            return await _repository.UpdateSeasonalities(seasonalitiesDTO);
        }

        public async Task<bool> DeleteSeasonalitiesById(int id)
        {
            return await _repository.DeleteSeasonalitiesById(id);
        }
    }
}
