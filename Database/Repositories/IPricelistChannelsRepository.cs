
using CampaignEditor.DTOs.UserDTO;
using Database.DTOs.PricelistChannels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IPricelistChannelsRepository
    {
        Task<bool> CreatePricelistChannels(CreatePricelistChannelsDTO pricelistChannelsDTO);
        Task<PricelistChannelsDTO> GetPricelistChannelsByIds(int plid, int chid);
        Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannelsByPlid(int plid);
        Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannelsByChid(int chid);
        Task<IEnumerable<PricelistChannelsDTO>> GetAllPricelistChannels();
        Task<bool> UpdatePricelistChannels(UpdatePricelistChannelsDTO pricelistChannelsDTO);
        Task<bool> DeletePricelistChannelsByIds(int plid, int chid);
    }
}
