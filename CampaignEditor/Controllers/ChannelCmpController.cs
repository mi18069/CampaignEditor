using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.TargetCmpDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class ChannelCmpController : ControllerBase
    {
        private readonly IChannelCmpRepository _repository;
        public ChannelCmpController(IChannelCmpRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ChannelCmpDTO> CreateChannelCmp(CreateChannelCmpDTO channelCmpDTO)
        {
            await _repository.CreateChannelCmp(channelCmpDTO);
            return await _repository.GetChannelCmpByIds(channelCmpDTO.cmpid, channelCmpDTO.plid);
        }

        public async Task<ChannelCmpDTO> GetChannelCmpByIds(int cmpid, int plid)
        {
            return await _repository.GetChannelCmpByIds(cmpid, plid);
        }

        public async Task<IEnumerable<ChannelCmpDTO>> GetChannelCmpsByCmpid(int id)
        {
            return await _repository.GetChannelCmpsByCmpid(id);
        }

        public async Task<IEnumerable<ChannelCmpDTO>> GetAllChannelCmps()
        {
            return await _repository.GetAllChannelCmps();

        }

        public async Task<bool> UpdateChannelCmp(UpdateChannelCmpDTO channelCmpDTO)
        {
            return await _repository.UpdateChannelCmp(channelCmpDTO);
        }

        public async Task<bool> DeleteChannelCmpByCmpid(int id)
        {
            return await _repository.DeleteChannelCmpByCmpid(id);
        }

        public async Task<bool> DeleteChannelCmpByIds(int cmpid, int plid)
        {
            return await _repository.DeleteChannelCmpByIds(cmpid, plid);
        }
    }
}
