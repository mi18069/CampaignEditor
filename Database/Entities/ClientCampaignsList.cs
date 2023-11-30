using Database.DTOs.CampaignDTO;
using Database.Entities;
using Database.DTOs.ClientDTO;
using System.Collections.ObjectModel;
using System.Linq;

namespace Database.Entities
{
    public class ClientCampaignsList
    {
        public ClientDTO Client { get; set; }
        public ObservableCollection<CampaignDTO> Campaigns { get; set; } = new ObservableCollection<CampaignDTO>();
    
       
    }
}

