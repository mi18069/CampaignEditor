using Database.DTOs.CmpBrndDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICmpBrndRepository
    {
        Task<bool> CreateCmpBrnd(CmpBrndDTO cmpbrndDTO);
        Task<IEnumerable<CmpBrndDTO>> GetCmpBrndsByCmpId(int id);
        Task<bool> UpdateBrand(CmpBrndDTO cmpbrndDTO);
        Task<bool> DeleteBrandByCmpId(int id);
    }
}
