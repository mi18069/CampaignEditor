using Database.DTOs.TargetClassDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ITargetClassRepository
    {
        Task<bool> CreateTargetClass(CreateTargetClassDTO targetClassDTO);
        Task<TargetClassDTO> GetTargetClassById(int id);
        Task<TargetClassDTO> GetTargetClassByName(string classname);
        Task<IEnumerable<TargetClassDTO>> GetAllTargetClasses();
        Task<bool> UpdateTargetClass(UpdateTargetClassDTO targetClassDTO);
        Task<bool> DeleteTargetClassById(int id);
        Task<bool> DeleteTargetClassByName(string classname);
    }
}
