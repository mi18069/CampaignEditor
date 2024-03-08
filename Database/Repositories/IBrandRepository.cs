using Database.DTOs.BrandDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IBrandRepository
    {
        Task<bool> CreateBrand(CreateBrandDTO brandDTO);
        Task<BrandDTO> GetBrandById(int id);
        Task<BrandDTO> GetBrandByName(string brandname);
        Task<IEnumerable<BrandDTO>> GetAllBrands();
        Task<bool> UpdateBrand(UpdateBrandDTO brandDTO);
        Task<bool> DeleteBrandById(int id);
    }
}
