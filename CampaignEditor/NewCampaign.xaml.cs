using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class NewCampaign : Window
    {

        private CampaignController _campaignController;
        private ClientController _clientController;
        
        private ClientDTO _client;

        public bool success = false;

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
            _client = await _clientController.GetClientByName(clientname);
            canClientBeDeleted = (await _campaignController.GetCampaignsByClientId(_client.clid)).Count() == 0;
            FillFields();
        }

        private void FillFields()
        {
            tbName.Text = "";
            tbClientname.Text = _client.clname.ToString().Trim();
            dpStartDate.SelectedDate = DateTime.Now;
            dpEndDate.SelectedDate = DateTime.Now;
            tbTbStartHours.Text = "02";
            tbTbStartMinutes.Text = "00";
            tbTbEndHours.Text = "25";
            tbTbEndMinutes.Text = "59";
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lblError.Content = "";
            string campaignName = tbName.Text.Trim();
            if (await CheckCampaign())
            {
                try
                {
                    await CreateCampaign();
                    success = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    lblError.Content = "Unable to make new campaign";
                }

            }
        }

        private async Task CreateCampaign()
        {
            int cmprev = 0;
            int cmpown = 1; // Don't know what this is
            string cmpname = tbName.Text.Trim();
            int clid = _client.clid;
            string cmpsdate = TimeFormat.DPToYMDString(dpStartDate);
            string cmpedate = TimeFormat.DPToYMDString(dpEndDate);
            string cmpstime = tbTbStartHours.Text.PadLeft(2, '0')+":"+tbTbStartMinutes.Text.PadLeft(2, '0')+":00";
            string cmpetime = tbTbEndHours.Text.PadLeft(2, '0')+":"+tbTbEndMinutes.Text.PadLeft(2, '0')+":59";
            int cmpstatus = 0;
            string sostring = "1;999;F;01234;012345";
            int activity = 0;
            DatePicker dpNow = new DatePicker();
            dpNow.SelectedDate = DateTime.Now;
            int cmpaddedon = int.Parse(TimeFormat.DPToYMDString(dpNow));
            int cmpaddedat = int.Parse(TimeFormat.DPToTimeString(dpNow)); 
            bool active = true;
            bool forcec = false;


            await _campaignController.CreateCampaign(new CreateCampaignDTO(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
                cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat, active, forcec));
        }

        private async Task<bool> CheckCampaign()
        {
            if (await _campaignController.GetCampaignByName(tbName.Text.Trim()) != null)
            {
                lblError.Content = "Already exists campaign with this name";
                return false;
            }
            else if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                lblError.Content = "Start date must be prior the end date";
                return false;
            }
            else
            {
                return true;
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
