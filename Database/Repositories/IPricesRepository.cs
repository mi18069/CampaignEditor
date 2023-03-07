using Database.DTOs.PricesDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IPricesRepository
    {
        Task<bool> CreatePrices(CreatePricesDTO pricesDTO);
        Task<PricesDTO> GetPricesById(int id);
        Task<IEnumerable<PricesDTO>> GetAllPricesByPlId(int id);
        Task<IEnumerable<PricesDTO>> GetAllPrices();
        Task<bool> UpdatePrices(UpdatePricesDTO pricesDTO);
        Task<bool> DeletePricesById(int id);
    }
}
