﻿using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using CampaignEditor.UserControls;
using Database.DTOs.MediaPlanVersionDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Database.DTOs.PricelistDTO;
using Database.Entities;
using Database.DTOs.CobrandDTO;
using Database.DTOs.SpotDTO;

namespace CampaignEditor
{
    public partial class CampaignForecastView : System.Windows.Controls.Page
    {

        private readonly IAbstractFactory<CampaignForecast> _factoryForecast; 
        private readonly IAbstractFactory<CampaignForecastDates> _factoryForecastDates;
        private ForecastDataManipulation _forecastDataManipulation;

        private MediaPlanRefController _mediaPlanRefController;
        private DatabaseFunctionsController _databaseFunctionsController;
        private MediaPlanVersionController _mediaPlanVersionController;

        private CampaignForecast _forecast = null;
        private CampaignForecastDates _forecastDates = null;

        LoadingPage loadingPage = new LoadingPage();
        public bool alreadyExists = false;
        public bool lockThis = false;

        private CampaignDTO _campaign;
        public TabItem tabForecast;

        public MediaPlanForecastData _forecastData;
        public MediaPlanConverter _mpConverter;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;

        List<DateTime> unavailableDates = new List<DateTime>();
        private bool isReadOnly = true;

        public event EventHandler UpdateValidation;
        public event EventHandler<UpdatedTermDateAndChannelEventArgs> UpdateTermDateAndChannel;
        public CampaignForecastView(IMediaPlanRefRepository mediaPlanRefRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IAbstractFactory<CampaignForecast> factoryForecast,
            IAbstractFactory<CampaignForecastDates> factoryForecastDates,
            IAbstractFactory<ForecastDataManipulation> factoryForecastDataManipulation)
        {
            _factoryForecast = factoryForecast;
            _factoryForecastDates = factoryForecastDates;
            _forecastDataManipulation = factoryForecastDataManipulation.Create();

            var onStartupController = new OnStartupContoller(databaseFunctionsRepository);

            // Let it run in the background on every starting
            onStartupController.RunUpdateUnavailableDates();

            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign, bool isReadOnly)
        {
            _campaign = campaign;
            this.isReadOnly = isReadOnly;

            var exists = (await _mediaPlanRefController.GetMediaPlanRef(_campaign.cmpid) != null);
            if (exists)
            {
                alreadyExists = true;
                if (_forecast == null)
                {
                    await LoadForecast();
                }

                tabForecast.Content = _forecast.Content;
            }
            else
            {
                alreadyExists = false;
                if (_forecastDates == null)
                {
                    await LoadForecastDates();
                }
                tabForecast.Content = _forecastDates.Content;
            }
        }

        private async Task LoadForecastDates()
        {
            unavailableDates = (await _databaseFunctionsController.GetAllUnavailableDates()).ToList();

            _forecastDates = _factoryForecastDates.Create();
            _forecastDates.CancelButtonClicked += ForecastDates_CancelButtonClicked;
            _forecastDates.InitializeButtonClicked += ForecastDates_InitializeButtonClicked;
            try
            {
                await _forecastDates.Initialize(_campaign, unavailableDates, isReadOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in initialization of Referenced data:\n" + ex.Message, "Error:",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private async Task LoadForecast()
        {
            _forecast = _factoryForecast.Create();
            _forecast._forecastData = _forecastData;
            _forecast._mpConverter = _mpConverter;
            _forecast._allMediaPlans = _allMediaPlans;

            try
            {               
                if (_forecast != null)
                {
                    // Unsubscribe from the SelectionChanged event
                    _forecast.cbVersions.SelectionChanged -= _forecast.CbVersions_SelectionChanged;
                }
                var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
                if (mpVer == null)
                {
                    mpVer = new MediaPlanVersionDTO(_campaign.cmpid, 0);
                    await _mediaPlanVersionController.CreateMediaPlanVersion(mpVer);
                }
                await _forecast.Initialize(_campaign, isReadOnly);
                await _forecast.LoadData(mpVer.version);
                if (_forecast != null)
                {
                    // Subscribe from the SelectionChanged event
                    _forecast.cbVersions.SelectionChanged += _forecast.CbVersions_SelectionChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in initialization of Forecast:\n" + ex.Message, "Error:",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }          
            _forecast.InitializeButtonClicked += Forecast_InitializeButtonClicked;
            _forecast.SetLoadingPage += _forecast_SetLoadingPage;
            _forecast.UpdateProgressBar += _forecast_UpdateProgressBar;
            _forecast.SetContentPage += _forecast_SetContentPage;
            _forecast.UpdatedTermDateAndChannel += _forecast_UpdatedTermDateAndChannel;
        }

        private void _forecast_UpdatedTermDateAndChannel(object? sender, UpdatedTermDateAndChannelEventArgs e)
        {
            UpdateTermDateAndChannel?.Invoke(this, e);
        }

        private void ForecastDates_CancelButtonClicked(object sender, EventArgs e)
        {
            tabForecast.Content = _forecast.Content;
        }

        // Inside CampaignForecastDates
        private async void ForecastDates_InitializeButtonClicked(object sender, EventArgs e)
        {
            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);

            if (alreadyExists)
            {
                if (MessageBox.Show("Current data will be lost\nAre you sure you want to initialize?", "Message: ",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return;
                }

                await _forecastDataManipulation.DeleteCampaignForecastVersion(_campaign.cmpid, mpVer.version);
                if (mpVer.version == 1)
                    mpVer = null;
                else
                    mpVer.version -= 1;
                await _mediaPlanRefController.DeleteMediaPlanRefById(_campaign.cmpid);
            }

            if (mpVer == null)
            {
                mpVer = new MediaPlanVersionDTO(_campaign.cmpid, 0);
                await _mediaPlanVersionController.CreateMediaPlanVersion(mpVer);
            }
            mpVer = await _mediaPlanVersionController.IncrementMediaPlanVersion(mpVer);
            int version = mpVer.version;

            tabForecast.Content = loadingPage.Content;
            var success = false;
            try
            {
                success = await InitializeNewForecast(version);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while initializing MediaPlan\n" + ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                await _forecastDataManipulation.DeleteCampaignForecastVersion(_campaign.cmpid, version);
                success = false;
            }
            if (success)
                tabForecast.Content = _forecast.Content;
            else
            {
                tabForecast.Content = _forecastDates.Content;
                return;
            }

            UpdateValidation?.Invoke(this, null);
            alreadyExists = true;
        }

        private async Task<bool> InitializeNewForecast(int version)
        {
            if (_forecastDates == null)
            {
                await LoadForecastDates();
            }

            if (_forecast == null)
            {            
                await LoadForecast();          
            }

            if (!await CheckPrerequisites(_campaign))
            {
                MessageBox.Show("Cannot start Forecast.\nNot all required parameters are given", "Message:",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            var success = await _forecastDates.InsertMediaPlanRefs();
            if (!success)
            {
                MessageBox.Show("An error occured, please try again", "Error: ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                await _forecast!.InsertAndLoadData(version);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while initializing Forecast!\n" + ex.Message, "Error: ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        // For events from overview
        public async Task UpdateCampaign(CampaignDTO campaign)
        {
            await _forecast.CampaignChanged(campaign);
        }
        public async Task UpdateGoals()
        {
            await _forecast.GoalsChanged();
        }

        public async Task UpdateTargets()
        {
            await _forecast.TargetsChanged();
        }

        public async Task UpdateSpots()
        {
            await _forecast.SpotsChanged();
        }

        public async Task UpdateDayParts()
        {
            await _forecast.DayPartsChanged();
        }

        public async Task UpdateChannels(List<int> channelsToDelete, List<int> channelsToAdd)
        {
            try
            {
                await _forecast.ChannelsChanged(channelsToDelete, channelsToAdd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating channels:\n" + ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

        public void ChannelsOrderChanged()
        {
            _forecast.ChannelsOrderChanged();
        }

        public async Task UpdatePricelist(PricelistDTO pricelist)
        {
            try
            {
                await _forecast.PricelistChanged(pricelist);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating pricelist:\n" + ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

        public async Task UpdateCobrands(IEnumerable<CobrandDTO> cobrands)
        {
            await _forecast.CobrandsChanged(cobrands);
        }


        #region Validation Forecast synchronizations

        public void AddRealizations(ObservableRangeCollection<MediaPlanRealized> mediaPlanRealized, DateOnly lastDateImport)
        {
            if (_forecast == null)
                return;

            _forecast.AddRealizations(mediaPlanRealized, lastDateImport);
        }

        public void AddIntoUpdatedRealizations(DateOnly date, int chrdsid, SpotDTO spot)
        {
            if (_forecast == null)
                return;

            _forecast.AddIntoUpdatedRealizations(date, chrdsid, spot);
        }

        public void CheckUpdatedRealizations()
        {
            if (_forecast == null)
                return;

            _forecast.CheckUpdatedRealizations();
        }

        #endregion

        // Inside CampaignForecast
        private async void Forecast_InitializeButtonClicked(object sender, EventArgs e)
        {
            if (_forecastDates == null)
            {
                await LoadForecastDates();
            }
            await _forecastDates!.LoadGridInit();
            tabForecast.Content = _forecastDates.Content;
        }

        private void _forecast_SetLoadingPage(object? sender, LoadingPageEventArgs e)
        {
            if (e != null)
            {
                if (e.progressBarValue > 0)
                {
                    loadingPage.SetProgressBarVisibility(Visibility.Visible);
                }
                else
                {
                    loadingPage.SetProgressBarVisibility(Visibility.Collapsed);
                }

                loadingPage.SetContent(e.Message);
            }

            tabForecast.Content = loadingPage.Content;
        }

        private void _forecast_UpdateProgressBar(object? sender, LoadingPageEventArgs e)
        {
            loadingPage.SetContent(e.Message);
            loadingPage.SetProgressBarValue(e.progressBarValue);
        }

        private void _forecast_SetContentPage(object? sender, ChangeVersionEventArgs e)
        {
            tabForecast.Content = _forecast.Content;
        }

        // Function which tests if we can make new forecast
        private async Task<bool> CheckPrerequisites(CampaignDTO campaign)
        {
            if (await _databaseFunctionsController.CheckForecastPrerequisites(campaign.cmpid))
                return true;
            else
                return false;
        }

        public void CloseForecast()
        {
            if (_forecast != null)
            {
                _forecast.InitializeButtonClicked -= Forecast_InitializeButtonClicked;
                _forecast.SetLoadingPage -= _forecast_SetLoadingPage;
                _forecast.UpdateProgressBar -= _forecast_UpdateProgressBar;
                _forecast.SetContentPage -= _forecast_SetContentPage;

                _forecast.ClosePage();
            }

            if (_forecastDates != null)
            {
                _forecastDates.CancelButtonClicked -= ForecastDates_CancelButtonClicked;
                _forecastDates.InitializeButtonClicked -= ForecastDates_InitializeButtonClicked;
            }
        }

    }
}
