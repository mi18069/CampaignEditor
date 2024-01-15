using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class Rename : Window
    {

        private readonly ClientController _clientController;
        private readonly CampaignController _campaignController;

        public ClientDTO _client = null;
        private CampaignDTO _campaign = null;

        private bool clientRenaming = false;
        private bool campaignRenaming = false;

        public bool renamed = false;
        public string newName = "";
        public Rename(IClientRepository clientRepository, ICampaignRepository campaignRepository)
        {
            _clientController = new ClientController(clientRepository);
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();
        }

        public async Task RenameClient(ClientDTO client)
        {
            _client = client;
            lblRename.Content = "Rename client";
            tbRename.Text = client.clname.Trim();
            renamed = false;
            clientRenaming = true;
        }

        public async Task RenameCampaign(CampaignDTO campaign)
        {
            _campaign = campaign;
            lblRename.Content = "Rename campaign";
            tbRename.Text = campaign.cmpname.Trim();
            renamed = false;
            campaignRenaming = true;
        }

        private void tbRename_TextChanged(object sender, TextChangedEventArgs e)
        {
            renamed = true;
            lblError.Content = "";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            renamed = false;
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (clientRenaming)
            {
                if (await CheckClientName())
                {
                    string clname = tbRename.Text.Trim();
                    _client.clname = clname;
                    await _clientController.UpdateClient(new UpdateClientDTO(_client.clid, _client.clname, _client.clactive, _client.spid));
                    this.Close();
                }
            }
            else
            {
                if (await CheckCampaignName())
                {
                    string cmpname = tbRename.Text.Trim();
                    _campaign.cmpname = cmpname;
                    await _campaignController.UpdateCampaign(new UpdateCampaignDTO(_campaign.cmpid,
                    _campaign.cmprev, _campaign.cmpown, _campaign.cmpname, _campaign.clid,
                    _campaign.cmpsdate, _campaign.cmpedate, _campaign.cmpstime, _campaign.cmpetime,
                    _campaign.cmpstatus, _campaign.sostring, _campaign.activity, _campaign.cmpaddedon,
                    _campaign.cmpaddedat, _campaign.active, _campaign.forcec));
                    this.Close();
                }
            }
        }

        private async Task<bool> CheckCampaignName()
        {
            string campaignName = tbRename.Text.Trim();
            if (await _campaignController.GetCampaignByName(campaignName) != null)
            {
                lblError.Content = "Campaign already exists";
                return false;
            }
            else
            {
                return true;
            }

        }

        private async Task<bool> CheckClientName()
        {
            string clientName = tbRename.Text.Trim();

            if (clientName == "")
            {
                lblError.Content = "Enter client name";
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
