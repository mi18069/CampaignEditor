using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using System.Collections.ObjectModel;

namespace Database.Entities
{
    public class ClientCampaignsList
    {
        public ClientDTO Client { get; set; }
        public ObservableCollection<CampaignDTO> Campaigns { get; set; } = new ObservableCollection<CampaignDTO>();


    }
}