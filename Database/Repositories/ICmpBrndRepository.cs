using Database.DTOs.CmpBrndDTO;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICmpBrndRepository
    {
        Task<bool> CreateCmpBrnd(CmpBrndDTO cmpbrndDTO);
        Task<CmpBrndDTO> GetCmpBrndByCmpId(int id);
        Task<bool> UpdateBrand(CmpBrndDTO cmpbrndDTO);
        Task<bool> DeleteBrandByCmpId(int id);
    }
}
