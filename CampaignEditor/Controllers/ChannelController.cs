using Database.DTOs.ChannelDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ChannelController : ControllerBase
    {
        private readonly IChannelRepository _repository;
        public ChannelController(IChannelRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ChannelDTO> CreateChannel(CreateChannelDTO channelDTO)
        {
            await _repository.CreateChannel(channelDTO);
            var channel = await _repository.GetChannelByName(channelDTO.chname);
            return channel;
        }

        public async Task<ChannelDTO> GetChannelById(int id)
        {
            var channel = await _repository.GetChannelById(id);
            return channel;
        }
        public async Task<ChannelDTO> GetChannelByName(string channelname)
        {
            var channel = await _repository.GetChannelByName(channelname);
            return channel;
        }
        public async Task<IEnumerable<ChannelDTO>> GetAllChannels()
        {
            return await _repository.GetAllChannels();
        }

        public async Task<bool> UpdateChannel(UpdateChannelDTO channelDTO)
        {
            return await _repository.UpdateChannel(channelDTO);
        }

        public async Task<bool> DeleteChannelById(int id)
        {
            return await _repository.DeleteChannelById(id);
        }
    }
}
