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
using CampaignEditor.Helpers;
using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using Database.Entities;
using Database.DTOs.CobrandDTO;

namespace CampaignEditor
{
    public partial class CampaignOverview : Page
    {
        private readonly IAbstractFactory<AssignTargets> _factoryAssignTargets;
        private readonly IAbstractFactory<Channels> _factoryChannels;
        private readonly IAbstractFactory<Spots> _factorySpots;
        private readonly IAbstractFactory<Goals> _factoryGoals;
        private readonly IAbstractFactory<CmpInfo> _factoryInfo;
        private readonly IAbstractFactory<ClientDayParts> _factoryClientDayParts;
        private readonly IAbstractFactory<ClientBrands> _factoryClientBrands;
        private readonly IAbstractFactory<Cobranding> _factoryCobranding;

        private readonly IAbstractFactory<PriceList> _factoryPriceList;

        private CampaignOverviewData _campaignOverviewData;

        public ClientDTO _client = null;
        private CampaignDTO _campaign = null;

        public bool isReadOnly = false;

        private List<SpotDTO> _spotlist = new List<SpotDTO>();

        private List<CobrandDTO> _cobrands = new List<CobrandDTO>();

        private List<TargetDTO> _targetlist = new List<TargetDTO>();

        private GoalsDTO _goals = null;

        private Channels fChannels = null;
        private List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _channels = null;

        private CampaignDTO _campaignInfo = null;
        private List<BrandDTO> _brands = null;

        private Dictionary<DayPartDTO, List<DPTimeDTO>> _dayPartsDict = null;

        public event EventHandler ClosePageEvent;
        public event EventHandler GoalsUpdatedEvent;
        public event EventHandler<UpdateCampaignEventArgs> CampaignUpdatedEvent;
        public event EventHandler<UpdateChannelsEventArgs> ChannelsUpdatedEvent;
        public event EventHandler ChannelsOrderChangedEvent;
        public event EventHandler<UpdatePricelistEventArgs> PricelistUpdatedEvent;
        public event EventHandler TargetsUpdatedEvent;
        public event EventHandler SpotsUpdatedEvent;
        public event EventHandler DayPartsUpdatedEvent;
        public event EventHandler<UpdateCobrandsEventArgs> CobrandsUpdatedEvent;

        public CampaignOverview(IAbstractFactory<AssignTargets> factoryAssignTargets,
            IAbstractFactory<Channels> factoryChannels, IAbstractFactory<Spots> factorySpots,
            IAbstractFactory<Goals> factoryGoals, IAbstractFactory<CmpInfo> factoryInfo,
            IAbstractFactory<CampaignOverviewData> campaignOverviewData, 
            IAbstractFactory<ClientDayParts> factoryClientDayParts,
            IAbstractFactory<ClientBrands> factoryClientBrands,
            IAbstractFactory<PriceList> factoryPriceList,
            IAbstractFactory<Cobranding> factoryCobranding)
        {
            this.DataContext = this;
            InitializeComponent();

            _factoryAssignTargets = factoryAssignTargets;
            _factoryChannels = factoryChannels;
            _factorySpots = factorySpots;
            _factoryGoals = factoryGoals;
            _factoryInfo = factoryInfo;
            _factoryClientDayParts = factoryClientDayParts;
            _factoryClientBrands = factoryClientBrands;

            _campaignOverviewData = campaignOverviewData.Create();
            _factoryPriceList = factoryPriceList;
            _factoryCobranding = factoryCobranding;
        }

        #region Initialization
        public async Task Initialization(ClientDTO client, CampaignDTO campaign, bool _isReadOnly = true)
        {
            _client = client;
            _campaign = campaign;
            isReadOnly = _isReadOnly;

            /*if (isReadOnly){
                btnCmpInfo.IsEnabled = false;
                btnAssignTargets.IsEnabled = false;
                btnSpots.IsEnabled = false;
                btnGoals.IsEnabled = false;
                btnChannels.IsEnabled = false;
                btnDayParts.IsEnabled = false;
                btnCobranding.IsEnabled = false;
                //btnBrands.IsEnabled = false;
            }*/

            try
            {

                // Create tasks for each initialization method
                Task infoTask = Task.Run(() => InitializeInfo());
                Task targetsTask = Task.Run(() => InitializeTargets());
                Task spotsTask = Task.Run(() => InitializeSpots());
                Task cobrandsTask = Task.Run(() => InitializeCobrands());
                Task goalsTask = Task.Run(() => InitializeGoals());
                Task channelsTask = Task.Run(() => InitializeChannels());
                Task dayPartsTask = Task.Run(() => InitializeDayParts());
                Task brandsTask = Task.Run(() => InitializeBrands());

                // Wait for all tasks to complete
                await Task.WhenAll(infoTask, targetsTask, spotsTask, cobrandsTask, goalsTask, channelsTask, dayPartsTask);

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
            await FillInfo(_campaign);
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
        private async Task InitializeCobrands()
        {
            _cobrands = (await _campaignOverviewData.GetCobrands(_campaign.cmpid)).ToList();
        }
        private async Task InitializeInfo()
        {

        }

        private async Task InitializeGoals()
        {

            _goals = await _campaignOverviewData.GetGoals(_campaign.cmpid);
        }

        private async Task InitializeChannels()
        {
            _channels = await _campaignOverviewData.GetChannelTuples(_campaign.cmpid);

        }
        private async Task InitializeBrands()
        {
            _brands = (await _campaignOverviewData.GetBrands(_campaign.cmpid)).ToList();
        }
        private async Task InitializeDayParts()
        {
            _dayPartsDict = await _campaignOverviewData.GetClientDayParts(_client.clid);

        }

        #endregion

        #region Info

        private async void btnCmpInfo_Click(object sender, RoutedEventArgs e)
        {
            btnCmpInfo.IsEnabled = false;
            CmpInfo fInfo = null;
            try
            {
                fInfo = _factoryInfo.Create();
                await fInfo.Initialize(_client, _campaign);
                fInfo.ShowDialog();
            }
            catch
            {
                btnCmpInfo.IsEnabled = true;
                return;
            }

            if (fInfo != null && fInfo.infoModified)
            {
                _campaign = fInfo.Campaign;
                CampaignUpdatedEvent?.Invoke(this, new UpdateCampaignEventArgs(fInfo.Campaign));
                _campaignInfo = fInfo.Campaign;
                await FillInfo(_campaignInfo);

            }
            btnCmpInfo.IsEnabled = true;

        }
        private async Task FillInfo(CampaignDTO campaign = null)
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
            }
        }

        #endregion 

        #region Targets
        private async void btnAssignTargets_Click(object sender, RoutedEventArgs e)
        {
            btnAssignTargets.IsEnabled = false;
            AssignTargets fTargets = null;
            try
            {
                fTargets = _factoryAssignTargets.Create();
                await fTargets.Initialize(_campaign, _targetlist);
                fTargets.ShowDialog();
            }
            catch
            {
                btnAssignTargets.IsEnabled = true;
                return;
            }

            if (fTargets != null && fTargets.targetsModified)
            {
                _targetlist = fTargets.SelectedTargetsList.ToList();
                FillDGTargets(_targetlist);
                TargetsUpdatedEvent?.Invoke(this, null);
            }
            btnAssignTargets.IsEnabled = true;

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
            btnChannels.IsEnabled = false;
            Channels fChannels = null;
            try
            {
                fChannels = _factoryChannels.Create();
                await fChannels.Initialize(_client, _campaign, _channels);
                fChannels.PricelistUpdatedEvent += FChannels_PricelistUpdatedEvent;
                fChannels.ShowDialog();
                fChannels.PricelistUpdatedEvent -= FChannels_PricelistUpdatedEvent;

            }
            catch
            {
                btnChannels.IsEnabled = true;
                return;
            }
            if (fChannels != null && (fChannels.channelsModified || fChannels.pricelistChanged))
            {
                _channels = await _campaignOverviewData.GetChannelTuples(_campaign.cmpid);
                dgChannels.ItemsSource = _channels;
                await InitializeCobrands();

            }
            if (fChannels != null && !fChannels.channelsModified && fChannels.channelsOrderChanged)
            {
                _channels = await _campaignOverviewData.GetChannelTuples(_campaign.cmpid);
                dgChannels.ItemsSource = _channels;
                ChannelsOrderChangedEvent?.Invoke(this, null);
            }
            if (fChannels != null && fChannels.updateChannels)
            {
                ChannelsUpdatedEvent?.Invoke(this, new UpdateChannelsEventArgs(fChannels.channelsToDelete, fChannels.channelsToAdd));
            }
            if (fChannels != null)
                fChannels.Close();

            btnChannels.IsEnabled = true;

        }

        private void FChannels_PricelistUpdatedEvent(object? sender, UpdatePricelistEventArgs e)
        {
            PricelistUpdatedEvent?.Invoke(this, e);
        }

        private async void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            var tuple = row.Item as Tuple<ChannelDTO, PricelistDTO, ActivityDTO>;
            if (tuple != null)
            {
                var pricelist = tuple.Item2;
                var f = _factoryPriceList.Create();
                await f.Initialize(_campaign, pricelist);
                f.MakeReadonly();
                f.ShowDialog();
            }
        }
        #endregion

        #region Spots
        private async void btnSpots_Click(object sender, RoutedEventArgs e)
        {
            btnSpots.IsEnabled = false;
            Spots fSpots = null;

            try
            {
                fSpots = _factorySpots.Create();
                await fSpots.Initialize(_campaign, _spotlist);
                fSpots.ShowDialog();
            }
            catch
            {
                btnSpots.IsEnabled = true;
                return;
            }
            
            if (fSpots != null && fSpots.spotsModified)
            {
                _spotlist = fSpots.Spotlist.ToList();
                dgSpots.ItemsSource = _spotlist;
                SpotsUpdatedEvent?.Invoke(this, null);
                await InitializeCobrands();
            }
            btnSpots.IsEnabled = true;

        }
        #endregion

        #region Cobranding

        private async void btnCobranding_Click(object sender, RoutedEventArgs e)
        {
            btnCobranding.IsEnabled = false;
            Cobranding fCobranding = null;
            try
            {
                fCobranding = _factoryCobranding.Create();
                fCobranding.Initialize(_campaign, _cobrands, _channels.Select(chn => chn.Item1), _spotlist.Select(s => s.spotcode.Trim()[0]));
                fCobranding.ShowDialog();
            }
            catch
            {
                btnCobranding.IsEnabled = true;
                return;
            }
            if (fCobranding != null && fCobranding.Modified)
            {
                // Propagating change
                var cobrands = fCobranding.GetChangedCobrands();
                CobrandsUpdatedEvent?.Invoke(this, new UpdateCobrandsEventArgs(cobrands));
                // Getting new data
                await InitializeCobrands();

            }

            btnCobranding.IsEnabled = true;

        }

        #endregion

        #region Goals
        private async void btnGoals_Click(object sender, RoutedEventArgs e)
        {
            btnGoals.IsEnabled = false;
            Goals fGoals = null;

            try
            {
                fGoals = _factoryGoals.Create();
                await fGoals.Initialize(_campaign, _goals);
                fGoals.ShowDialog();
            }
            catch
            {
                btnGoals.IsEnabled = true;
                return;
            }

            if (fGoals.goalsModified)
            {
                _goals = fGoals.Goal;
                FillGoals(_goals);
                // For updating goals in forecast
                GoalsUpdatedEvent?.Invoke(this, null);
            }

            btnGoals.IsEnabled = true;

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

        #region DayParts

        private void btnDayParts_Click(object sender, RoutedEventArgs e)
        {
            btnDayParts.IsEnabled = false;
            ClientDayParts fDayParts = null;

            try
            {
                fDayParts = _factoryClientDayParts.Create();
                fDayParts.Initialize(_client, _dayPartsDict);
                fDayParts.ShowDialog();
            }
            catch
            {
                btnDayParts.IsEnabled = true;
                return;
            }

            if (fDayParts.dpModified)
            {
                _dayPartsDict = fDayParts.DayPartsDict;
                //FillGoals(_goals);
                // For updating goals in forecast
                DayPartsUpdatedEvent?.Invoke(this, null);
            }

            btnDayParts.IsEnabled = true;
        }

        #endregion

        #region Brands

        private async void btnBrands_Click(object sender, RoutedEventArgs e)
        {
            btnBrands.IsEnabled = false;
            ClientBrands fClientBrands = null;

            try
            {
                fClientBrands = _factoryClientBrands.Create();
                await fClientBrands.Initialize(_campaign, _brands);
                fClientBrands.ShowDialog();
            }
            catch
            {
                btnBrands.IsEnabled = true;
                return;
            }

            if (fClientBrands.isModified)
            {
                _brands = fClientBrands.SelectedBrands;
                //FillGoals(_goals);
                // For updating goals in forecast
                //DayPartsUpdatedEvent?.Invoke(this, null);
            }

            btnBrands.IsEnabled = true;
        }

        #endregion


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClosePageEvent?.Invoke(this, new EventArgs());
        }

        private void dataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;

            // Get the ScrollViewer containing the DataGrid
            ScrollViewer scrollViewer;
            if ((sender as DataGrid).Name == "dgSpots")
                scrollViewer = svSpots;
            else if ((sender as DataGrid).Name == "dgChannels")
                scrollViewer = svChannels;
            else
                return;
            // Calculate the new vertical offset based on the mouse wheel delta
            double newVerticalOffset = scrollViewer.VerticalOffset - e.Delta*0.1;

            // Ensure the new vertical offset is within bounds
            if (newVerticalOffset < 0)
                newVerticalOffset = 0;
            else if (newVerticalOffset > scrollViewer.ScrollableHeight)
                newVerticalOffset = scrollViewer.ScrollableHeight;

            // Set the new vertical offset
            scrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }


    }
}
