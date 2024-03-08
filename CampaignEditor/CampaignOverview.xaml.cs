﻿using Database.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ActivityDTO;
using Database.DTOs.BrandDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.PricelistDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.TargetDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Database.Entities;

namespace CampaignEditor
{
    public partial class CampaignOverview : Page
    {
        private readonly IAbstractFactory<AssignTargets> _factoryAssignTargets;
        private readonly IAbstractFactory<Channels> _factoryChannels;
        private readonly IAbstractFactory<Spots> _factorySpots;
        private readonly IAbstractFactory<Goals> _factoryGoals;
        private readonly IAbstractFactory<CmpInfo> _factoryInfo;

        private CampaignOverviewData _campaignOverviewData;

        public ClientDTO _client = null;
        private CampaignDTO _campaign = null;

        public bool isReadOnly = false;

        private List<SpotDTO> _spotlist = new List<SpotDTO>();

        private List<TargetDTO> _targetlist = new List<TargetDTO>();

        private GoalsDTO _goals = null;

        private Channels fChannels = null;
        private List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _channels = null;

        private CampaignDTO _campaignInfo = null;
        private BrandDTO[] _brands = null;

        public event EventHandler ClosePageEvent;
        public CampaignOverview(IAbstractFactory<AssignTargets> factoryAssignTargets,
            IAbstractFactory<Channels> factoryChannels, IAbstractFactory<Spots> factorySpots,
            IAbstractFactory<Goals> factoryGoals, IAbstractFactory<CmpInfo> factoryInfo,
            IAbstractFactory<CampaignOverviewData> campaignOverviewData)
        {
            this.DataContext = this;
            
            _factoryAssignTargets = factoryAssignTargets;
            _factoryChannels = factoryChannels;
            _factorySpots = factorySpots;
            _factoryGoals = factoryGoals;
            _factoryInfo = factoryInfo;

            _campaignOverviewData = campaignOverviewData.Create();

            InitializeComponent();
        }

        #region Initialization
        public async Task Initialization(ClientDTO client, CampaignDTO campaign, bool _isReadOnly = true)
        {
            _client = client;
            _campaign = campaign;
            isReadOnly = _isReadOnly;

            if (isReadOnly){
                btnCmpInfo.IsEnabled = false;
                btnAssignTargets.IsEnabled = false;
                btnSpots.IsEnabled = false;
                btnGoals.IsEnabled = false;
                btnChannels.IsEnabled = false;
            }

            try
            {

                // Create tasks for each initialization method
                Task infoTask = Task.Run(() => InitializeInfo());
                Task targetsTask = Task.Run(() => InitializeTargets());
                Task spotsTask = Task.Run(() => InitializeSpots());
                Task goalsTask = Task.Run(() => InitializeGoals());
                Task channelsTask = Task.Run(() => InitializeChannels());

                // Wait for all tasks to complete
                await Task.WhenAll(infoTask, targetsTask, spotsTask, goalsTask, channelsTask);

                FillFields();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot retrieve data for campaign!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ClosePage();
            }

        }
        private void ClosePage()
        {
            ClosePageEvent?.Invoke(this, new EventArgs());
        }

        private async Task FillFields()
        {
            FillDGTargets(_targetlist);
            await FillInfo(_campaign, _brands);
            FillGoals(_goals);
            dgSpots.ItemsSource = _spotlist;
            dgChannels.ItemsSource = _channels;
        }

        private async Task InitializeTargets()
        {
            _targetlist = await _campaignOverviewData.GetTargets(_campaign.cmpid);
        }

        private async Task InitializeSpots()
        {
            _spotlist = await _campaignOverviewData.GetSpots(_campaign.cmpid);
        }
        private async Task InitializeInfo()
        {

            _brands = await _campaignOverviewData.GetBrands(_campaign.cmpid);

        }

        private async Task InitializeGoals()
        {

            _goals = await _campaignOverviewData.GetGoals(_campaign.cmpid);
        }

        private async Task InitializeChannels()
        {
            _channels = await _campaignOverviewData.GetChannelTuples(_campaign.cmpid);

        }

        #endregion

        #region Info

        private async void btnCmpInfo_Click(object sender, RoutedEventArgs e)
        {
            CmpInfo fInfo = null;
            try
            {
                fInfo = _factoryInfo.Create();
                await fInfo.Initialize(_client, _campaign, _brands);
                fInfo.ShowDialog();
            }
            catch
            {
                return;
            }

            if (fInfo != null && fInfo.infoModified)
            {
                _campaign = fInfo.Campaign;
                _campaignInfo = fInfo.Campaign;
                _brands = fInfo.SelectedBrands;
                await FillInfo(_campaignInfo, _brands);

            }
        }
        private async Task FillInfo(CampaignDTO campaign = null, BrandDTO[] brands = null)
        {
            if (campaign != null)
            {

                lblClientValue.Content = _client.clname.ToString().Trim();
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
                var activity = await _campaignOverviewData.GetActivity(_campaign);
                lblActivityValue.Content = activity.act.Trim();
                if (brands[0] != null)
                {
                    lblBrand1Value.Content = brands[0].brand;
                    if (lblBrand1Value.Content.ToString().Length > 15)
                    {
                        lblBrand1Value.Content = lblBrand1Value.Content.ToString().Substring(0, 14) + "...";
                    }
                }
                else
                {
                    lblBrand1Value.Content = "-";
                }
                if (brands[1] != null)
                {
                    lblBrand2Value.Content = brands[1].brand;
                    if (lblBrand2Value.Content.ToString().Length > 15)
                    {
                        lblBrand2Value.Content = lblBrand2Value.Content.ToString().Substring(0, 14) + "...";
                    }
                }
                else
                {
                    lblBrand2Value.Content = "-";
                }
            }
        }

        #endregion 

        #region Targets
        private async void btnAssignTargets_Click(object sender, RoutedEventArgs e)
        {
            AssignTargets fTargets = null;
            try
            {
                fTargets = _factoryAssignTargets.Create();
                await fTargets.Initialize(_campaign, _targetlist);
                fTargets.ShowDialog();
            }
            catch
            {
                return;
            }

            if (fTargets != null && fTargets.targetsModified)
            {
                _targetlist = fTargets.SelectedTargetsList.ToList();
                FillDGTargets(_targetlist);
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
            Channels fChannels = null;
            try
            {
                fChannels = _factoryChannels.Create();
                await fChannels.Initialize(_client, _campaign, _channels);
                fChannels.ShowDialog();
            }
            catch
            {
                return;
            }
            if (fChannels != null && (fChannels.channelsModified || fChannels.pricelistChanged))
            {
                _channels = await _campaignOverviewData.GetChannelTuples(_campaign.cmpid);
                dgChannels.ItemsSource = _channels;
            }
            if (fChannels != null)
                fChannels.Close();
        }
        #endregion

        #region Spots
        private async void btnSpots_Click(object sender, RoutedEventArgs e)
        {
            Spots fSpots = null;

            try
            {
                fSpots = _factorySpots.Create();
                await fSpots.Initialize(_campaign, _spotlist);
                fSpots.ShowDialog();
            }
            catch
            {
                return;
            }
            
            if (fSpots != null && fSpots.spotsModified)
            {
                _spotlist = fSpots.Spotlist.ToList();
                dgSpots.ItemsSource = _spotlist;
            }
        }
        #endregion

        #region Goals
        private async void btnGoals_Click(object sender, RoutedEventArgs e)
        {
            Goals fGoals = null;

            try
            {
                fGoals = _factoryGoals.Create();
                await fGoals.Initialize(_campaign, _goals);
                fGoals.ShowDialog();
            }
            catch
            {
                return;
            }

            if (fGoals.goalsModified)
            {
                _goals = fGoals.Goal;
                FillGoals(_goals);
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
            ClosePageEvent?.Invoke(this, new EventArgs());
        }


    }
}
