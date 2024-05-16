
using Database.DTOs.CampaignDTO;

namespace CampaignEditor.Helpers
{
    public class UpdateCampaignEventArgs
    {
        public CampaignDTO Campaign { get; private set; }

        public UpdateCampaignEventArgs(CampaignDTO campaign)
        {
            this.Campaign = campaign;
        }
    }
}
