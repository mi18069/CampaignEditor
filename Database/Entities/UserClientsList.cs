using CampaignEditor.DTOs.UserDTO;
using System.Collections.ObjectModel;

namespace Database.Entities
{
    public class UserClientsList
    {
        public UserDTO User { get; set; }
        public ObservableCollection<ClientCampaignsList> ClientCampaigns { get; set; } = new ObservableCollection<ClientCampaignsList>();

    }
}
