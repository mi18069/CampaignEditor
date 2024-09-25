using Database.DTOs.CobrandDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICobrandRepository
    {
        Task<bool> CreateCobrand(CreateCobrandDTO cobrandDTO);
        Task<IEnumerable<CobrandDTO>> GetAllCampaignCobrands(int cmpid);
        Task<IEnumerable<CobrandDTO>> GetAllChannelCobrands(int cmpid, int chid);
        Task<bool> UpdateCobrand(UpdateCobrandDTO cobrandDTO);
        Task<bool> DeleteCobrand(int cmpid, int chid, char spotcode);
        Task<bool> DeleteCobrandsForChannel(int cmpid, int chid);
        Task<bool> DeleteCampaignCobrands(int cmpid);

    }
}
