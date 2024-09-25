using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Database.DTOs.ActivityDTO;
using Database.DTOs.GoalsDTO;

namespace CampaignEditor
{
    public partial class NewCampaign : Window
    {

        private CampaignController _campaignController;
        private ClientController _clientController;
        private ActivityController _activityController;
        private GoalsController _goalsController;

        private ClientDTO _client;

        private List<ActivityDTO> _activities = new List<ActivityDTO>();
        
        // In order to know which textBox to update
        int tbToEditIndex = 0;

        public bool success = false;
        public CampaignDTO _campaign = null;
        public bool canClientBeDeleted = false;
        public NewCampaign(ICampaignRepository campaignRepository, IClientRepository clientRepository,
                           IActivityRepository activityRepository, IGoalsRepository goalsRepository)
        {
            this.DataContext = this;
            InitializeComponent();
            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);            
            _activityController = new ActivityController(activityRepository);
            _goalsController = new GoalsController(goalsRepository);
        }

        // For binding client to campaign
        public async Task Initialize(int clientId, CampaignDTO campaign = null)
        {
            _client = await _clientController.GetClientById(clientId);
            canClientBeDeleted = (await _campaignController.GetCampaignsByClientId(_client.clid)).Count() == 0;      
            FillFields(campaign);
            await FillCBAcitivities();
            if (campaign != null)
            {
                FillRbTv(campaign);
                SetActivity(campaign);
            }
        }

        private void FillRbTv(CampaignDTO campaign)
        {
            if (campaign.tv == true)
            {
                rbTv.IsChecked = true;
            }
            else
                rbRadio.IsChecked = true;
        }

        private void SetActivity(CampaignDTO campaign)
        {
            var index = campaign.activity;
            cbActivities.SelectedIndex = index;
        }       

        private async Task FillCBAcitivities()
        {
            var activities = (await _activityController.GetAllActivities()).OrderBy(act => act.actid);
            foreach (var activity in activities)
            {
                _activities.Add(activity);
            }
            cbActivities.ItemsSource = _activities;
        }
      
        private void FillFields(CampaignDTO campaign = null)
        {
            if (campaign == null)
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
            else
            {
                tbName.Text = campaign.cmpname.Trim() + "_duplicated";
                tbClientname.Text = _client.clname.ToString().Trim();
                dpStartDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
                dpEndDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);
                tbTbStartHours.Text = campaign.cmpstime.Substring(0, 2);
                tbTbStartMinutes.Text = campaign.cmpstime.Substring(3, 2);
                tbTbEndHours.Text = campaign.cmpetime.Substring(0, 2);
                tbTbEndMinutes.Text = campaign.cmpetime.Substring(3, 2);
            }
        }       

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string campaignName = tbName.Text.Trim();
            if (await CheckCampaign())
            {
                try
                {
                    var campaign = await CreateCampaign();                   
                    // Add default goals
                    await _goalsController.CreateGoals(new CreateGoalsDTO(campaign.cmpid, 0, 0, 0, 0, 999, 0));
                    _campaign = campaign;
                    success = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create new campaign", "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }


        private async Task<CampaignDTO> CreateCampaign()
        {
            int cmprev = 0;
            int cmpown = MainWindow.user.usrid;
            string cmpname = tbName.Text.Trim();
            int clid = _client.clid;
            string cmpsdate = TimeFormat.DPToYMDString(dpStartDate);
            string cmpedate = TimeFormat.DPToYMDString(dpEndDate);
            string cmpstime = tbTbStartHours.Text.PadLeft(2, '0')+":"+tbTbStartMinutes.Text.PadLeft(2, '0')+":00";
            string cmpetime = tbTbEndHours.Text.PadLeft(2, '0')+":"+tbTbEndMinutes.Text.PadLeft(2, '0')+":59";
            int cmpstatus = 0;
            string sostring = "1;999;F;01234;012345";
            int activity = cbActivities.SelectedIndex;
            DatePicker dpNow = new DatePicker();
            var dateTimeNow = DateTime.Now;
            dpNow.SelectedDate = dateTimeNow;
            int cmpaddedon = int.Parse(TimeFormat.DPToYMDString(dpNow));
            int cmpaddedat = int.Parse(TimeFormat.DTToTimeString(dateTimeNow)); 
            bool active = true;
            bool forcec = false;
            bool tv = rbTv.IsChecked == true ? true : false;

            var campaign = await _campaignController.CreateCampaign(new CreateCampaignDTO(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
                cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat, active, forcec, tv));
            
            return campaign;
        }

        private async Task<bool> CheckCampaign()
        {
            if (tbName.Text.Trim() == "")
            {
                MessageBox.Show("Enter campaign name", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (await _campaignController.GetCampaignByName(tbName.Text.Trim()) != null)
            {
                MessageBox.Show("Already exists campaign with this name", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Select start and end date of campaign", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                MessageBox.Show("Start date must be prior the end date", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if ((int)(dpEndDate.SelectedDate - dpStartDate.SelectedDate).Value.TotalDays > 365)
            {
                MessageBox.Show("Campaign cannot be longer than a year", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (!(dpStartDate.SelectedDate.Value.Date > DateTime.Today.Date))
            {
                if (MessageBox.Show("Start date is set in the past. Are you sure you want to proceed?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    return false;
                else
                    return true;
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
