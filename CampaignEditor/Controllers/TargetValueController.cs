using Database.DTOs.TargetValueDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class TargetValueController
    {
        private readonly ITargetValueRepository _repository;
        public TargetValueController(ITargetValueRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<TargetValueDTO> CreateTargetValue(CreateTargetValueDTO targetValueDTO)
        {
            await _repository.CreateTargetValue(targetValueDTO);
            var target = await _repository.GetTargetValueByName(targetValueDTO.name);
            return target;
        }

        public async Task<TargetValueDTO> GetTargetValueById(int id)
        {
            var target = await _repository.GetTargetValueById(id);
            return target;
        }
        public async Task<TargetValueDTO> GetTargetValueByName(string targname)
        {
            var target = await _repository.GetTargetValueByName(targname);
            return target;
        }
        public async Task<TargetValueDTO> GetTargetValueByIdAndValue(int id, string value)
        {
            var target = await _repository.GetTargetValueByIdAndValue(id, value);
            return target;
        }

        public async Task<bool> UpdateTarget(UpdateTargetValueDTO targetValueDTO)
        {
            return await _repository.UpdateTargetValue(targetValueDTO);
        }

        public async Task<bool> DeleteTargetValueById(int id)
        {
            return await _repository.DeleteTargetValueById(id);
        }
        public async Task<bool> DeleteTargetValueByName(string targname)
        {
            return await _repository.DeleteTargetValueByName(targname);
        }
        public async Task<IEnumerable<TargetValueDTO>> GetAllTargetValues()
        {
            return await _repository.GetAllTargetValues();
        }
        public async Task<IEnumerable<TargetValueDTO>> GetAllTargetValuesWithId(int classId)
        {
            return await _repository.GetAllTargetValuesWithId(classId);
        }

    }
}
