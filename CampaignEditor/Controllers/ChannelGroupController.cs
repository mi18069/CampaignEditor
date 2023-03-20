using Database.DTOs.ChannelGroupDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ChannelGroupController : ControllerBase
    {
        private readonly IChannelGroupRepository _repository;
        public ChannelGroupController(IChannelGroupRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ChannelGroupDTO> CreateChannelGroup(CreateChannelGroupDTO channelGroupDTO)
        {
            await _repository.CreateChannelGroup(channelGroupDTO);
            var channelGroup = await _repository.GetChannelGroupByName(channelGroupDTO.chgrname);
            return channelGroup;
        }

        public async Task<ChannelGroupDTO> GetChannelGroupById(int id)
        {
            var channelGroup = await _repository.GetChannelGroupById(id);
            return channelGroup;
        }

        public async Task<ChannelGroupDTO> GetChannelGroupByName(string name)
        {
            var channelGroup = await _repository.GetChannelGroupByName(name);
            return channelGroup;
        }

        public async Task<IEnumerable<ChannelGroupDTO>> GetAllChannelGroups()
        {
            return await _repository.GetAllChannelGroups();
        }

        public async Task<bool> UpdateChannelGroup(UpdateChannelGroupDTO channelGroupDTO)
        {
            return await _repository.UpdateChannelGroup(channelGroupDTO);
        }

        public async Task<bool> DeleteChannelGroupById(int id)
        {
            return await _repository.DeleteChannelGroupById(id);
        }
    }
}