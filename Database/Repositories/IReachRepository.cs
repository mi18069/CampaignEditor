using Database.DTOs.ReachDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IReachRepository
    {
        Task<IEnumerable<ReachDTO>> GetReachByCmpid(int cmpid);
        Task<ReachDTO?> GetFinalReachByCmpid(int cmpid);
        Task<bool> DeleteReachByCmpid(int cmpid);
    }
}
