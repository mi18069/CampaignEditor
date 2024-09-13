using Database.DTOs.SpotPairDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISpotPairRepository
    {
        Task<bool> CreateSpotPair(CreateSpotPairDTO spotPairDTO);
        Task<SpotPairDTO> GetSpotPairBySpotnum(int cmpid, int spotnum);
        Task<IEnumerable<SpotPairDTO>> GetAllCampaignSpotPairs(int cmpid);
        Task<bool> UpdateSpotPair(UpdateSpotPairDTO spotPairDTO);
        Task<bool> DeleteSpotcodeFromSpotPairs(int cmpid, string spotcode);
        Task<bool> DeleteAllCampaignSpotPairs(int cmpid);
    }
}
