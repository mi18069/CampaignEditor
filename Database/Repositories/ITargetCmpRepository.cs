using Database.DTOs.TargetCmpDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ITargetCmpRepository
    {
        Task<bool> CreateTargetCmp(CreateTargetCmpDTO targetCmpDTO);
        Task<IEnumerable<TargetCmpDTO>> GetTargetCmpByCmpid(int id);
        Task<TargetCmpDTO> GetTargetCmpByIds(int cmpid, int targid);
        Task<IEnumerable<TargetCmpDTO>> GetAllTargetCmps();
        Task<bool> UpdateTargetCmp(UpdateTargetCmpDTO targetCmpDTO);
        Task<bool> DeleteTargetCmpByCmpid(int id);
        Task<bool> DeleteTargetCmpByIds(int cmpid, int targid);
    }
}
