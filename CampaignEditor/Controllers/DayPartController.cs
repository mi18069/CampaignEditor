using Database.DTOs.DayPartDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class DayPartController
    {
        private readonly IDayPartRepository _repository;
        public DayPartController(IDayPartRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<DayPartDTO> CreateDayPart(CreateDayPartDTO dayPartDTO)
        {
            var dpId = await _repository.CreateDayPart(dayPartDTO);
            return await _repository.GetDayPartById(dpId);
        }
        public async Task<DayPartDTO> GetDayPartById(int id)
        {
            return await _repository.GetDayPartById(id);
        }

        public async Task<IEnumerable<DayPartDTO>> GetAllClientDayParts(int clientId)
        {
            return await _repository.GetAllClientDayParts(clientId);
        }

        public async Task<bool> UpdateDayPart(UpdateDayPartDTO dayPartDTO)
        {
           return await _repository.UpdateDayPart(dayPartDTO);
        }

        public async Task<bool> DeleteDayPart(int id)
        {
            return await _repository.DeleteDayPart(id);
        }
    }
}
