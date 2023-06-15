﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using CampaignEditor.UserControls;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class CampaignForecastView : Page
    {

        private readonly IAbstractFactory<CampaignForecast> _factoryForecast; 
        private readonly IAbstractFactory<CampaignForecastDates> _factoryForecastDates;

        private MediaPlanRefController _mediaPlanRefController;
        private DatabaseFunctionsController _databaseFunctionsController;

        private CampaignForecast _forecast;
        private CampaignForecastDates _forecastDates;

        LoadingPage loadingPage = new LoadingPage();
        private bool alreadyExists = false;

        private CampaignDTO _campaign;
        public TabItem tabForecast;

        List<DateTime> unavailableDates = new List<DateTime>();

        public CampaignForecastView(IMediaPlanRefRepository mediaPlanRefRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IAbstractFactory<CampaignForecast> factoryForecast,
            IAbstractFactory<CampaignForecastDates> factoryForecastDates)
        {
            _factoryForecast = factoryForecast;
            _factoryForecastDates = factoryForecastDates;

            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;

            await _databaseFunctionsController.RunUpdateUnavailableDates();
            unavailableDates = (await _databaseFunctionsController.GetAllUnavailableDates()).ToList();

            _forecast = _factoryForecast.Create();
            await _forecast.Initialize(_campaign, unavailableDates);
            _forecast.InitializeButtonClicked += Forecast_InitializeButtonClicked;

            _forecastDates = _factoryForecastDates.Create();
            _forecastDates.CancelButtonClicked += ForecastDates_CancelButtonClicked;
            _forecastDates.InitializeButtonClicked += ForecastDates_InitializeButtonClicked;
            await _forecastDates.Initialize(_campaign, unavailableDates);

            var exists = (await _mediaPlanRefController.GetMediaPlanRef(_campaign.cmpid) != null);
            if (exists)
            {
                alreadyExists = true;
                await _forecast.LoadData();
                tabForecast.Content = _forecast.Content;
            }
            else
            {
                alreadyExists = false;
                tabForecast.Content = _forecastDates.Content;
            }
        }

        private void ForecastDates_CancelButtonClicked(object sender, EventArgs e)
        {
            tabForecast.Content = _forecast.Content;
        }

        // Inside CampaignForecastDates
        private async void ForecastDates_InitializeButtonClicked(object sender, EventArgs e)
        {
            if (alreadyExists)
            {
                if (MessageBox.Show("All data will be lost\nAre you sure you want to initialize?", "Message: ",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    tabForecast.Content = loadingPage.Content;
                    await _forecast.DeleteData();
                    await InitializeNewForecast();
                    tabForecast.Content = _forecast.Content;
                }
                else
                {
                    return;
                }
            }
            else
            {
                tabForecast.Content = loadingPage.Content;
                await InitializeNewForecast();
                tabForecast.Content = _forecast.Content;
            }
            alreadyExists = true;
        }

        private async Task InitializeNewForecast()
        {
            var success = await _forecastDates.InsertMediaPlanRefs();
            if (!success)
            {
                MessageBox.Show("An error occured, please try again", "Message: ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await _forecast.InsertAndLoadData();
        }

        // Inside CampaignForecast
        private async void Forecast_InitializeButtonClicked(object sender, EventArgs e)
        {
            await _forecastDates.LoadGridInit();
            tabForecast.Content = _forecastDates.Content;
        }
    }
}
