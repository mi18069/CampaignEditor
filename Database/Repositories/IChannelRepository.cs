using Database.DTOs.ChannelDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IChannelRepository
    {
        Task<bool> CreateChannel(CreateChannelDTO channelDTO);
        Task<ChannelDTO> GetChannelById(int id);
        Task<string> GetChannelNameById(int id);
        Task<ChannelDTO> GetChannelByName(string channelname);
        Task<IEnumerable<ChannelDTO>> GetAllChannels();
        Task<bool> UpdateChannel(UpdateChannelDTO channelDTO);
        Task<bool> DeleteChannelById(int id);
    }
}
