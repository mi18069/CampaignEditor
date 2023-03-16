using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class NewCampaign : Window
    {

        private CampaignController _campaignController;
        private ClientController _clientController;
        ClientDTO client;

        public bool canClientBeDeleted = false;
        public NewCampaign(ICampaignRepository campaignRepository, IClientRepository clientRepository)
        {
            InitializeComponent();
            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);
        }

        // For binding client to campaign
        public async Task Initialize(string clientname)
        {
            client = await _clientController.GetClientByName(clientname);
            canClientBeDeleted = (await _campaignController.GetCampaignsByClientId(client.clid)).Count() == 0;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lblError.Content = "";
            var a = client;
            string campaignName = tbName.Text.Trim();
            if (await CheckCampaignName(campaignName))
            {
                try
                {
                    await CreateCampaign(campaignName);
                    this.Close();
                }
                catch (Exception ex)
                {
                    lblError.Content = "Unable to make new campaign";
                }

            }
        }

        private async Task CreateCampaign(string campaignName)
        {
            DateTime now = DateTime.Now;

            int cmprev = 0;
            int cmpown = 1; // Don't know what this is
            string cmpname = campaignName;
            int clid = client.clid;
            string cmpsdate = DateToString(now);
            string cmpedate = DateToString(now);
            string cmpstime = "02:00:00";
            string cmpetime = "25:59:59";
            int cmpstatus = 0;
            string sostring = "1;999;F;01234;012345";
            int activity = 0;
            int cmpaddedon = DateToInt(now);
            int cmpaddedat = TimeToInt(now); 
            bool active = false;
            bool forcec = false;


            _ = await _campaignController.CreateCampaign(new CreateCampaignDTO(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
                cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat, active, forcec));
        }

        private int TimeToInt(DateTime now)
        {
            return Convert.ToInt32(now.ToString("HHmmss"));
        }

        private int DateToInt(DateTime now)
        {
            return Convert.ToInt32(now.ToString("yyyyMMdd"));
        }

        private string TimeToString(DateTime now)
        {
            return now.ToString("HH:mm:ss");
        }

        private string DateToString(DateTime now)
        {
            return now.ToString("yyyyMMdd");
        }

        private async Task<bool> CheckCampaignName(string campaignName)
        {
            if (await _campaignController.GetCampaignByName(campaignName) == null)
            {
                return true;
            }
            else
            {
                lblError.Content = "Already exist campaign with this name";
                return false;
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
