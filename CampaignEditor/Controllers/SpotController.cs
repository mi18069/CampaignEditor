using Database.DTOs.ChannelDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.SpotDTO;
using Database.Entities;

namespace CampaignEditor.Controllers
{
    public class SpotController : ControllerBase
    {
        private readonly ISpotRepository _repository;
        public SpotController(ISpotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<SpotDTO> CreateSpot(CreateSpotDTO spotDTO)
        {
            await _repository.CreateSpot(spotDTO);
            var spot = await _repository.GetSpotByCmpidAndCode(spotDTO.cmpid, spotDTO.spotcode);
            return spot;
        }
        public async Task<SpotDTO> GetSpotsByCmpidAndCode(int id, string code)
        {
            return await _repository.GetSpotByCmpidAndCode(id, code);
        }

        public async Task<IEnumerable<SpotDTO>> GetSpotsByCmpid(int id)
        {
            var spots = await _repository.GetSpotsByCmpid(id);
            return spots;
        }

        public async Task<SpotDTO> GetSpotByName(string spotname)
        {
            return await _repository.GetSpotByName(spotname);
        }

        public async Task<IEnumerable<SpotDTO>> GetAllSpots()
        {
            var spots = await _repository.GetAllSpots();
            return spots;
        }

        public async Task<bool> UpdateSpot(UpdateSpotDTO spotDTO)
        {
            var spot = await _repository.UpdateSpot(spotDTO);
            return spot;
        }

        public async Task<bool> DeleteSpotsByCmpid(int id)
        {
            return await _repository.DeleteSpotsByCmpid(id);
        }

        public async Task<bool> DeleteSpotByCmpidAndCode(int id, string code)
        {
            return await _repository.DeleteSpotByCmpidAndCode(id, code);
        }
    }
}
