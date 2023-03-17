using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ActivityDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.PricelistDTO;
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
        private readonly IAbstractFactory<Goals> _factoryGoals;
        private readonly IAbstractFactory<CmpInfo> _factoryInfo;


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

        private bool goalsModified = false;
        private Goals fGoals = null;
        private GoalsDTO _goals = null;

        private bool channelsModified = false;
        private Channels fChannels = null;
        private List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _channels = null;

        private bool infoModified = false;
        private CmpInfo fInfo = null;
        private CampaignDTO _campaign = null;

        public AddCampaign(ICampaignRepository campaignRepository, ITargetCmpRepository targetCmpRepository, 
            IClientRepository clientRepository, IAbstractFactory<AssignTargets> factoryAssignTargets,
            IAbstractFactory<Channels> factoryChannels, IAbstractFactory<Spots> factorySpots,
            IAbstractFactory<Goals> factoryGoals, IAbstractFactory<CmpInfo> factoryInfo)
        {
            instance = this;

            _factoryAssignTargets = factoryAssignTargets;
            _factoryChannels = factoryChannels;
            _factorySpots = factorySpots;
            _factoryGoals = factoryGoals;
            _factoryInfo = factoryInfo;

            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
            InitializeComponent();            
            
        }

        #region Initialization
        public async Task Initialization(string campaignName)
        {
            campaign = await _campaignController.GetCampaignByName(campaignName);
            client = await _clientController.GetClientById(campaign.clid);

            this.Title = "Client: " + client.clname.Trim() + "  Campaign: " + campaign.cmpname.Trim();

            InitializeInfo();
            await InitializeTargets();
            await InitializeSpots();
            await InitializeGoals();
            await InitializeChannels();
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
        private void InitializeInfo()
        {
            fInfo = _factoryInfo.Create();
            fInfo.Initialize(client, campaign);
            _campaign = fInfo.Campaign;
            FillInfo(_campaign);
        }

        private async Task InitializeGoals()
        {
            fGoals = _factoryGoals.Create();
            await fGoals.Initialize(campaign);
            _goals = fGoals.Goal;
            FillGoals(_goals);
        }

        private async Task InitializeChannels()
        {
            fChannels = _factoryChannels.Create();
            await fChannels.Initialize(client, campaign);
            _channels = fChannels.SelectedChannels;
            dgChannels.ItemsSource = _channels;
        }

        #endregion

        #region Info

        private async void btnCmpInfo_Click(object sender, RoutedEventArgs e)
        {
            fInfo.Initialize(client, _campaign);
            fInfo.ShowDialog();
            if (fInfo.infoModified)
            {
                _campaign = fInfo.Campaign;
                infoModified = true;
                FillInfo(_campaign);
                fInfo.infoModified = false;
            }
        }
        private void FillInfo(CampaignDTO campaign = null)
        {
            if (campaign != null)
            {

                lblClientValue.Content = client.clname.ToString().Trim();
                if (lblClientValue.Content.ToString().Length > 15)
                {
                    lblClientValue.Content = lblClientValue.Content.ToString().Substring(0, 14) + "...";
                }
                lblCampaignValue.Content = campaign.cmpname.ToString().Trim();
                if (lblCampaignValue.Content.ToString().Length > 15)
                {
                    lblCampaignValue.Content = lblCampaignValue.Content.ToString().Substring(0, 14) + "...";
                }
                lblStartDateValue.Content = TimeFormat.YMDStringToRepresentative(campaign.cmpsdate);
                lblEndDateValue.Content = TimeFormat.YMDStringToRepresentative(campaign.cmpedate);
                lblDPStartValue.Content = campaign.cmpstime.ToString().Trim();
                lblDPEndValue.Content = campaign.cmpetime.ToString().Trim();
                lblActiveValue.Content = campaign.active;
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
        private async void btnChannels_Click(object sender, RoutedEventArgs e)
        {

            await fChannels.Initialize(client, campaign, _channels);
            fChannels.ShowDialog();
            if (fChannels.channelsModified)
            {
                _channels = fChannels.SelectedChannels;
                dgChannels.ItemsSource = _channels;
                channelsModified = true;
                fChannels.channelsModified = false;
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

        #region Goals
        private async void btnGoals_Click(object sender, RoutedEventArgs e)
        {
            await fGoals.Initialize(campaign, _goals);
            fGoals.ShowDialog();
            if (fGoals.goalsModified)
            {
                _goals = fGoals.Goal;
                goalsModified = true;
                FillGoals(_goals);
                fGoals.goalsModified = false;
            }
        }

        private void FillGoals(GoalsDTO goals = null)
        {
            if (goals != null)
            {
                if (goals.budget == 0)
                {
                    bBudget.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bBudget.Visibility = Visibility.Visible;
                    lblBudgetValue.Content = goals.budget.ToString();
                }

                if (goals.grp == 0)
                {
                    bGRP.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bGRP.Visibility = Visibility.Visible;
                    lblGRPValue.Content = goals.grp.ToString();
                }

                if (goals.ins == 0)
                {
                    bInsertations.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bInsertations.Visibility = Visibility.Visible;
                    lblInsertationsValue.Content = goals.ins.ToString();
                }

                if (goals.rch == 0)
                {
                    bReach.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bReach.Visibility = Visibility.Visible;
                    lblReachValue.Content = "from " + goals.rch_f1.ToString().Trim() + " to " + goals.rch_f2.ToString().Trim() +
                    " , " + goals.rch.ToString().Trim() + " %";
                }                  
            }
        }
        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (targetsModified)
            {
                await fTargets.UpdateDatabase(_targetlist);
            }
            if (spotsModified)
            {
                await fSpots.UpdateDatabase(_spotlist);
            }
            if (goalsModified)
            {
                await fGoals.UpdateDatabase(_goals);
            }
            if (channelsModified)
            {
                await fChannels.UpdateDatabase(_channels);
            }
            if (infoModified)
            {
                await fInfo.UpdateDatabase(_campaign);
            }
        }

        private void dgSpots_Loaded(object sender, RoutedEventArgs e)
        {
            svSpots.MaxHeight = dgSpots.ActualHeight;
        }
    }
}
