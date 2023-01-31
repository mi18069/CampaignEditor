using Database.DTOs.TargetClassDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class TargetClassController
    {
        private readonly ITargetClassRepository _repository;

        public TargetClassController(ITargetClassRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<TargetClassDTO> CreateTarget(CreateTargetClassDTO targetDTO)
        {
            await _repository.CreateTargetClass(targetDTO);
            var target = await _repository.GetTargetClassByName(targetDTO.name);
            return target;
        }

        public async Task<TargetClassDTO> GetTargetClassById(int id)
        {
            var target = await _repository.GetTargetClassById(id);
            return target;
        }
        public async Task<TargetClassDTO> GetTargetClassByName(string targname)
        {
            var target = await _repository.GetTargetClassByName(targname);
            return target;
        }

        public async Task<bool> UpdateTargetClass(UpdateTargetClassDTO targetClassDTO)
        {
            return await _repository.UpdateTargetClass(targetClassDTO);
        }

        public async Task<bool> DeleteTargetClassById(int id)
        {
            return await _repository.DeleteTargetClassById(id);
        }
        public async Task<bool> DeleteTargetClassByName(string targname)
        {
            return await _repository.DeleteTargetClassByName(targname);
        }
        public async Task<IEnumerable<TargetClassDTO>> GetAllTargetClasses()
        {
            return await _repository.GetAllTargetClasses();
        }
    }
}
