using Database.DTOs.CampaignDTO;
using System;

namespace CampaignEditor.Helpers
{
    public class CampaignContextMenuEventArgs
    {
        public CampaignDTO Campaign { get; private set; }
        public enum Options { EditCampaign = 0, RenameCampaign = 1, DeleteCampaign = 2, DuplicateCampaign = 3};
        public Enum Option { get; private set; }
        public CampaignContextMenuEventArgs(CampaignDTO campaign, Options options)
        {
            Campaign = campaign;
            Option = options;
        }
    }
}
