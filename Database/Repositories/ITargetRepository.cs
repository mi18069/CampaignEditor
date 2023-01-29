using Database.DTOs.TargetDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ITargetRepository
    {
        Task<bool> CreateTarget(CreateTargetDTO targetDTO);
        Task<TargetDTO> GetTargetById(int id);
        Task<TargetDTO> GetTargetByName(string targname);
        Task<IEnumerable<TargetDTO>> GetAllTargets();
        Task<IEnumerable<TargetDTO>> GetAllClientTargets(int clientId);
        Task<bool> UpdateTarget(UpdateTargetDTO targetDTO);
        Task<bool> DeleteTargetById(int id);
        Task<bool> DeleteTargetByName(string targname);
    }
}
