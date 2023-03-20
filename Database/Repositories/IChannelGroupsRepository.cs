using Database.DTOs.ChannelGroupsDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IChannelGroupsRepository
    {
        Task<bool> CreateChannelGroups(CreateChannelGroupsDTO channelGroupsDTO);
        Task<ChannelGroupsDTO> GetChannelGroupsByIds(int chgrid, int chid);
        Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroupsByChgrid(int chgrid);
        Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroupsByChid(int chid);
        Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroups();
        Task<bool> UpdateChannelGroups(UpdateChannelGroupsDTO channelGroupsDTO);
        Task<bool> DeleteChannelGroupsByChgrid(int chgrid);
        Task<bool> DeleteChannelGroupsByChid(int chid);
        Task<bool> DeleteChannelGroupsByIds(int chgrid, int chid);
    }
}
