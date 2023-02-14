using CampaignEditor.DTOs.UserDTO;
using Database.DTOs.ChannelDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IChannelRepository
    {
        Task<bool> CreateChannel(CreateChannelDTO channelDTO);
        Task<ChannelDTO> GetChannelById(int id);
        Task<IEnumerable<ChannelDTO>> GetAllChannels();
        Task<bool> UpdateChannel(UpdateChannelDTO channelDTO);
        Task<bool> DeleteChannelById(int id);
    }
}
