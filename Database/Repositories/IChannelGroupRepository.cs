using Database.DTOs.ChannelGroupDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IChannelGroupRepository
    {
        Task<bool> CreateChannelGroup(CreateChannelGroupDTO channelGroupDTO);
        Task<ChannelGroupDTO> GetChannelGroupById(int id);
        Task<ChannelGroupDTO> GetChannelGroupByName(string name);
        Task<ChannelGroupDTO> GetChannelGroupByNameAndOwner(string name, int owner);
        Task<IEnumerable<ChannelGroupDTO>> GetAllOwnerChannelGroups(int ownerId);
        Task<IEnumerable<ChannelGroupDTO>> GetAllChannelGroups();
        Task<bool> UpdateChannelGroup(UpdateChannelGroupDTO channelGroupDTO);
        Task<bool> DeleteChannelGroupById(int id);
    }
}
