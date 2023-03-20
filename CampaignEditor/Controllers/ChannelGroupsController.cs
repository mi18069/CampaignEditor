using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.ChannelGroupsDTO;

namespace CampaignEditor.Controllers
{
    public class ChannelGroupsController : ControllerBase
    {
        private readonly IChannelGroupsRepository _repository;
        public ChannelGroupsController(IChannelGroupsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateChannelGroups(CreateChannelGroupsDTO channelGroupsDTO)
        {
            return await _repository.CreateChannelGroups(channelGroupsDTO);
        }

        public async Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroupsByChgrid(int chgrid)
        {
            return await _repository.GetAllChannelGroupsByChgrid(chgrid);

        }

        public async Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroupsByChid(int chid)
        {
            return await _repository.GetAllChannelGroupsByChid(chid);

        }

        public async Task<ChannelGroupsDTO> GetChannelGroupsByIds(int chgrid, int chid)
        {
            var channelGroups = await _repository.GetChannelGroupsByIds(chgrid, chid);
            return channelGroups;
        }

        public async Task<IEnumerable<ChannelGroupsDTO>> GetAllChannelGroups()
        {
            return await _repository.GetAllChannelGroups();

        }

        public async Task<bool> UpdateChannelGroups(UpdateChannelGroupsDTO channelGroupsDTO)
        {
            return await _repository.UpdateChannelGroups(channelGroupsDTO);

        }

        public async Task<bool> DeleteChannelGroupsByChgrid(int chgrid)
        {
            return await _repository.DeleteChannelGroupsByChgrid(chgrid);

        }

        public async Task<bool> DeleteChannelGroupsByChid(int chid)
        {
            return await _repository.DeleteChannelGroupsByChid(chid);

        }

        public async Task<bool> DeleteChannelGroupsByIds(int chgrid, int chid)
        {
            return await _repository.DeleteChannelGroupsByIds(chgrid, chid);

        }
    }
}
