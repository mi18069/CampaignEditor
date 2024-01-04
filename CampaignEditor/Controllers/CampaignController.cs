using Database.DTOs.CampaignDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignRepository _repository;

        public CampaignController(ICampaignRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<CampaignDTO> CreateCampaign(CreateCampaignDTO campaignDTO)
        {
            await _repository.CreateCampaign(campaignDTO);
            var campaign = await _repository.GetCampaignByName(campaignDTO.cmpname);
            return campaign;
        }

        public async Task<CampaignDTO> GetCampaignById(int cmpid)
        {
            var campaign = await _repository.GetCampaignById(cmpid);
            return campaign;
        }

        public async Task<CampaignDTO> GetCampaignByName(string cmpname)
        {
            var campaign = await _repository.GetCampaignByName(cmpname);
            return campaign;
        }

        public async Task<IEnumerable<CampaignDTO>> GetAllCampaigns()
        {
            return await _repository.GetAllCampaigns();
        }

        public async Task<IEnumerable<CampaignDTO>> GetCampaignsByClientId(int userid)
        {
            return await _repository.GetCampaignsByClientId(userid);
        }

        public async Task<bool> UpdateCampaign(UpdateCampaignDTO campaignDTO)
        {
            return await _repository.UpdateCampaign(campaignDTO);
        }

        public async Task<bool> DeleteCampaignById(int cmpid)
        {
            return await _repository.DeleteCampaignById(cmpid);
        }

        public async Task<bool> DeleteCampaignsByUserId(int userid)
        {
            return await _repository.DeleteCampaignsByUserId(userid);

        }
    }
}
