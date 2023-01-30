using Database.DTOs.TargetClassDTO;
using Database.DTOs.TargetValueDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class TargetClassValuesController
    {
        private readonly ITargetClassRepository _classRepository;
        private readonly ITargetValueRepository _valueRepository;
        public TargetClassValuesController(ITargetClassRepository classRepository, ITargetValueRepository valueRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
        }
        #region TargetClassController
        public async Task<TargetClassDTO> CreateTarget(CreateTargetClassDTO targetDTO)
        {
            await _classRepository.CreateTargetClass(targetDTO);
            var target = await _classRepository.GetTargetClassByName(targetDTO.name);
            return target;
        }

        public async Task<TargetClassDTO> GetTargetClassById(int id)
        {
            var target = await _classRepository.GetTargetClassById(id);
            return target;
        }
        public async Task<TargetClassDTO> GetTargetClassByName(string targname)
        {
            var target = await _classRepository.GetTargetClassByName(targname);
            return target;
        }

        public async Task<bool> UpdateTargetClass(UpdateTargetClassDTO targetClassDTO)
        {
            return await _classRepository.UpdateTargetClass(targetClassDTO);
        }

        public async Task<bool> DeleteTargetClassById(int id)
        {
            return await _classRepository.DeleteTargetClassById(id);
        }
        public async Task<bool> DeleteTargetClassByName(string targname)
        {
            return await _classRepository.DeleteTargetClassByName(targname);
        }
        public async Task<IEnumerable<TargetClassDTO>> GetAllTargetClasses()
        {
            return await _classRepository.GetAllTargetClasses();
        }
        #endregion

        #region Value controller

        public async Task<TargetValueDTO> CreateTargetValue(CreateTargetValueDTO targetValueDTO)
        {
            await _valueRepository.CreateTargetValue(targetValueDTO);
            var target = await _valueRepository.GetTargetValueByName(targetValueDTO.name);
            return target;
        }

        public async Task<TargetValueDTO> GetTargetValueById(int id)
        {
            var target = await _valueRepository.GetTargetValueById(id);
            return target;
        }
        public async Task<TargetValueDTO> GetTargetValueByName(string targname)
        {
            var target = await _valueRepository.GetTargetValueByName(targname);
            return target;
        }

        public async Task<bool> UpdateTarget(UpdateTargetValueDTO targetValueDTO)
        {
            return await _valueRepository.UpdateTargetValue(targetValueDTO);
        }

        public async Task<bool> DeleteTargetValueById(int id)
        {
            return await _valueRepository.DeleteTargetValueById(id);
        }
        public async Task<bool> DeleteTargetValueByName(string targname)
        {
            return await _valueRepository.DeleteTargetValueByName(targname);
        }
        public async Task<IEnumerable<TargetValueDTO>> GetAllTargetValues()
        {
            return await _valueRepository.GetAllTargetValues();
        }
        public async Task<IEnumerable<TargetValueDTO>> GetValuesOfClass(int classId)
        {
            return await _valueRepository.GetAllTargetValuesWithId(classId);
        }

        #endregion

        #region Combined

        #endregion

    }
}
