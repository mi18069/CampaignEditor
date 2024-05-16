using Database.DTOs.PricelistDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IPricelistRepository
    {
        Task<int> CreatePricelist(CreatePricelistDTO pricelistDTO);
        Task<bool> IsPricelistInUse(int plid);
        Task<PricelistDTO> GetPricelistById(int id);
        Task<PricelistDTO> GetPricelistByName(string pricelistname);
        Task<PricelistDTO> GetClientPricelistByName(int clid, string pricelistname);
        Task<IEnumerable<PricelistDTO>> GetAllPricelists();
        Task<IEnumerable<PricelistDTO>> GetAllClientPricelists(int clid);
        Task<bool> UpdatePricelist(UpdatePricelistDTO pricelistDTO);
        Task<bool> DeletePricelistById(int id);
    }
}
