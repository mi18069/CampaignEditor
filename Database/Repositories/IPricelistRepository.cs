using CampaignEditor.DTOs.UserDTO;
using Database.DTOs.PricelistDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IPricelistRepository
    {
        Task<bool> CreatePricelist(CreatePricelistDTO pricelistDTO);
        Task<PricelistDTO> GetPricelistById(int id);
        Task<IEnumerable<PricelistDTO>> GetAllPricelists();
        Task<bool> UpdatePricelist(UpdatePricelistDTO pricelistDTO);
        Task<bool> DeletePricelistById(int id);
    }
}
