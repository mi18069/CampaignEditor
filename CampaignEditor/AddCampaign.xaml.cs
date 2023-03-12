using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampaignEditor
{

    public partial class AddCampaign : Window
    {
        private readonly IAbstractFactory<AssignTargets> _factoryAssignTargets;
        private readonly IAbstractFactory<Channels> _factoryChannels;
        private readonly IAbstractFactory<Spots> _factorySpots;


        private CampaignController _campaignController;
        private ClientController _clientController;
        private TargetCmpController _targetCmpController;

        private CampaignDTO campaign = null;
        public ClientDTO client = null;

        public static AddCampaign instance;

        AssignTargets assignTargetsFactory = null;
        Channels assignChannelsFactory = null;

        private bool spotsModified = false;
        private Spots fSpots = null;
        private List<SpotDTO> _spotlist = new List<SpotDTO>();

        private bool targetsModified = false;
        private AssignTargets fTargets = null;
        private List<TargetDTO> _targetlist = new List<TargetDTO>();

        public AddCampaign(ICampaignRepository campaignRepository, ITargetCmpRepository targetCmpRepository, 
            IClientRepository clientRepository, IAbstractFactory<AssignTargets> factoryAssignTargets,
            IAbstractFactory<Channels> factoryChannels, IAbstractFactory<Spots> factorySpots)
        {
            instance = this;

            _factoryAssignTargets = factoryAssignTargets;
            _factoryChannels = factoryChannels;
            _factorySpots = factorySpots;

            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
            InitializeComponent();            

        }

        public async Task Initialization(string campaignName)
        {
            campaign = await _campaignController.GetCampaignByName(campaignName);
            client = await _clientController.GetClientById(campaign.clid);

            InitializeFields(campaignName);
            await InitializeTargets();
            await InitializeSpots();
            
        }

        private async Task InitializeTargets()
        {
            fTargets = _factoryAssignTargets.Create();
            await fTargets.Initialize(campaign);
            _targetlist = fTargets.SelectedTargetsList.ToList();
            FillDGTargets(_targetlist);
        }

        private async Task InitializeSpots()
        {
            fSpots = _factorySpots.Create();
            await fSpots.Initialize(campaign);
            dgSpots.ItemsSource = fSpots.Spotlist;
            _spotlist = fSpots.Spotlist.ToList();
        }
        private void InitializeFields(string campaignName)
        {
            tbName.Text = campaignName;
            lblClientname.Content = client.clname;

            dpStartDate.SelectedDate = ConvertStringToDate(campaign.cmpsdate);
            dpEndDate.SelectedDate = ConvertStringToDate(campaign.cmpedate);

            FillTBTextBoxes();
        }

        #region Info
        private void FillTBTextBoxes()
        {
            tbTbStartHours.Text = campaign.cmpstime[0].ToString() + campaign.cmpstime[1].ToString();
            tbTbStartMinutes.Text = campaign.cmpstime[3].ToString() + campaign.cmpstime[4].ToString();

            tbTbEndHours.Text = campaign.cmpetime[0].ToString() + campaign.cmpetime[1].ToString();
            tbTbEndMinutes.Text = campaign.cmpetime[3].ToString() + campaign.cmpetime[4].ToString();
        }
        #region Converters
        private string ConvertDateTimeToDateString(DateTime dateTime)
        {
            string year = dateTime.Year.ToString("0000");
            string month = dateTime.Month.ToString("00");
            string day = dateTime.Day.ToString("00");

            return year + month + day;
        }
        private string ConvertDateTimeToTimeString(DateTime dateTime)
        {
            string hour = dateTime.Hour.ToString("00");
            string minute = dateTime.Minute.ToString("00");
            string second = dateTime.Second.ToString("00");

            return hour + minute + second;
        }

        private DateTime ConvertStringToDate(string timeString)
        {
            int year = Convert.ToInt32(timeString[0].ToString() + timeString[1].ToString() + timeString[2].ToString() + timeString[3].ToString());
            int month = Convert.ToInt32(timeString[4].ToString() + timeString[5].ToString());
            int day = Convert.ToInt32(timeString[6].ToString() + timeString[7].ToString());

            return new DateTime(year, month, day);
        }

        #endregion

        private void TxtBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).MaxLength == ((TextBox)sender).Text.Length)
            {
                // move focus to the next
                var ue = e.OriginalSource as FrameworkElement;
                e.Handled = true;
                ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        #endregion

        #region Targets
        private async void btnAssignTargets_Click(object sender, RoutedEventArgs e)
        {
            await fTargets.Initialize(campaign, _targetlist);
            fTargets.ShowDialog();
            if (fTargets.targetsModified)
            {
                _targetlist = fTargets.SelectedTargetsList.ToList();
                FillDGTargets(_targetlist);
                targetsModified = true;
                fTargets.targetsModified = false;
            }
        }
        private void FillDGTargets(List<TargetDTO> selectedTargetsList)
        {

            List<Tuple<string, TargetDTO>> targets = new List<Tuple<string, TargetDTO>>();
            int i = 0;
            foreach (TargetDTO target in selectedTargetsList)
            {
                if (i == 0)
                    targets.Add(Tuple.Create("Primary", target));
                else if (i == 1)
                    targets.Add(Tuple.Create("Secondary", target));
                else if (i == 2)
                    targets.Add(Tuple.Create("Tertiary", target));
                i++;
            }
            dgTargets.ItemsSource = targets;
        }
       
        #endregion

        #region Channels
        private void btnChannels_Click(object sender, RoutedEventArgs e)
        {

            if (assignChannelsFactory == null)
            {
                assignChannelsFactory = _factoryChannels.Create();
                assignChannelsFactory.Initialize(client);
            }
            assignChannelsFactory.ShowDialog();
            if (assignChannelsFactory.success)
            {
                dgChannels.ItemsSource = assignChannelsFactory.LastSelected;
                assignChannelsFactory.success = false;
            }
        }
        #endregion

        #region Spots
        private async void btnSpots_Click(object sender, RoutedEventArgs e)
        {
            await fSpots.Initialize(campaign, _spotlist);
            fSpots.ShowDialog();
            if (fSpots.spotsModified)
            {
                _spotlist = fSpots.Spotlist.ToList();
                spotsModified = true;
                fSpots.spotsModified = false;
            }
        }
        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (spotsModified)
            {
                await fSpots.UpdateDatabase(_spotlist);
            }
            if (targetsModified)
            {
                await fTargets.UpdateDatabase(_targetlist);
            }
        }
    }
}
