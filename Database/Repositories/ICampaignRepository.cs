using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.DTOs.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICampaignRepository
    {
        Task<bool> CreateCampaign(CreateCampaignDTO campaignDTO);
        Task<CampaignDTO> GetCampaignById(int cmpid);
        Task<CampaignDTO> GetCampaignByName(string cmpname);
        Task<IEnumerable<CampaignDTO>> GetAllCampaigns();
        Task<IEnumerable<CampaignDTO>> GetCampaignsByClientId(int clid);
        Task<bool> UpdateCampaign(UpdateCampaignDTO campaignDTO);
        Task<bool> DeleteCampaignById(int cmpid);
        Task<bool> DeleteCampaignsByUserId(int userid);

    }
}
