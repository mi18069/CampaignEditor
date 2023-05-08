using Database.DTOs.ChannelCmpDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IChannelCmpRepository
    {
        Task<bool> CreateChannelCmp(CreateChannelCmpDTO channelCmpDTO);
        Task<IEnumerable<ChannelCmpDTO>> GetChannelCmpsByCmpid(int id);
        Task<ChannelCmpDTO> GetChannelCmpByIds(int cmpid, int chid);
        Task<IEnumerable<ChannelCmpDTO>> GetAllChannelCmps();
        Task<bool> UpdateChannelCmp(UpdateChannelCmpDTO channelCmpDTO);
        Task<bool> DeleteChannelCmpByCmpid(int id);
        Task<bool> DeleteChannelCmpByIds(int cmpid, int plid);
    }
}
