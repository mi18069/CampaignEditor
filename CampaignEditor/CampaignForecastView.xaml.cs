using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using CampaignEditor.UserControls;
using Database.DTOs.MediaPlanVersionDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private MediaPlanVersionController _mediaPlanVersionController;

        private CampaignForecast _forecast = null;
        private CampaignForecastDates _forecastDates = null;

        LoadingPage loadingPage = new LoadingPage();
        private bool alreadyExists = false;

        private CampaignDTO _campaign;
        public TabItem tabForecast;

        private OnStartupContoller _onStartupController;

        List<DateTime> unavailableDates = new List<DateTime>();

        public CampaignForecastView(IMediaPlanRefRepository mediaPlanRefRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IAbstractFactory<CampaignForecast> factoryForecast,
            IAbstractFactory<CampaignForecastDates> factoryForecastDates,
            IDatabaseFunctionsRepository dfRepository)
        {
            _factoryForecast = factoryForecast;
            _factoryForecastDates = factoryForecastDates;

            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);
            _onStartupController = new OnStartupContoller(dfRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            await _onStartupController.RunUpdateUnavailableDates();

            var exists = (await _mediaPlanRefController.GetMediaPlanRef(_campaign.cmpid) != null);
            if (exists)
            {
                alreadyExists = true;
                if (_forecast == null)
                {
                    await LoadForecast();
                }
                var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
                if (mpVersion == null)
                {
                    var mpVerDTO = new MediaPlanVersionDTO(_campaign.cmpid, 1);
                    await _mediaPlanVersionController.CreateMediaPlanVersion(mpVerDTO);
                    await _forecast.LoadData(1, true);
                }
                else
                {
                    await _forecast.LoadData(mpVersion.version, true);
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
            // Changed to execute in startup
            //await _databaseFunctionsController.RunUpdateUnavailableDates();
            unavailableDates = (await _databaseFunctionsController.GetAllUnavailableDates()).ToList();

            _forecastDates = _factoryForecastDates.Create();
            _forecastDates.CancelButtonClicked += ForecastDates_CancelButtonClicked;
            _forecastDates.InitializeButtonClicked += ForecastDates_InitializeButtonClicked;
            await _forecastDates.Initialize(_campaign, unavailableDates);
        }

        private async Task LoadForecast()
        {
            _forecast = _factoryForecast.Create();
            await _forecast.Initialize(_campaign);
            _forecast.InitializeButtonClicked += Forecast_InitializeButtonClicked;
            _forecast.VersionChanged += Forecast_ChangeVersionClicked;
            _forecast.NewVersionClicked += _forecast_NewVersionClicked;
            _forecast.SetLoadingPage += _forecast_SetLoadingPage;
            _forecast.SetContentPage += _forecast_SetContentPage;
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
                if (MessageBox.Show("Current data will be lost\nAre you sure you want to initialize?", "Message: ",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    tabForecast.Content = loadingPage.Content;
                    var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
                    int version = 1;
                    if (mpVer != null)
                        version = mpVer.version;

                    await _forecast.DeleteData(version);
                    var success = await InitializeNewForecast();
                    if (success)
                    {
                        tabForecast.Content = _forecast.Content;
                    }
                    else
                    {
                        tabForecast.Content = _forecastDates.Content;
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                tabForecast.Content = loadingPage.Content;
                var success = await InitializeNewForecast();
                if (success)
                    tabForecast.Content = _forecast.Content;
                else
                {
                    tabForecast.Content = _forecastDates.Content;
                    return;
                }
                    
            }
            alreadyExists = true;
        }

        private async Task<bool> InitializeNewForecast()
        {
            if (_forecastDates == null)
            {
                await LoadForecastDates();
            }

            if (! await CheckPrerequisites(_campaign))
            {
                MessageBox.Show("Cannot start Forecast.\nNot all required parameters are given", "Message:",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            var success = await _forecastDates.InsertMediaPlanRefs();
            if (!success)
            {
                MessageBox.Show("An error occured, please try again", "Message: ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);

            if (_forecast == null)
            {
                await LoadForecast();
            }
            if (mpVer == null)
            {
                var mpVerDTO = new MediaPlanVersionDTO(_campaign.cmpid, 1);
                await _mediaPlanVersionController.CreateMediaPlanVersion(mpVerDTO);
                await _forecast.InsertAndLoadData(1);
            }
            else
            {
                mpVer = await _mediaPlanVersionController.IncrementMediaPlanVersion(mpVer);
                await LoadForecast();

                await _forecast.InsertAndLoadData(mpVer.version);
            }
            

            return true;
        }

        // Inside CampaignForecast
        private async void Forecast_InitializeButtonClicked(object sender, EventArgs e)
        {
            if (_forecastDates == null)
            {
                await LoadForecastDates();
            }
            await _forecastDates.LoadGridInit();
            tabForecast.Content = _forecastDates.Content;
        }

        private async void Forecast_ChangeVersionClicked(object sender, ChangeVersionEventArgs e)
        {
            tabForecast.Content = loadingPage.Content;
            //_forecast.EmptyFields();
            int version = e.Version;
            await _forecast.LoadData(version, false);
            tabForecast.Content = _forecast.Content;
        }

        private async void _forecast_NewVersionClicked(object? sender, ChangeVersionEventArgs e)
        {
            int version = _forecast.CmpVersion;

            if (MessageBox.Show("Make new media plan from version: " + version + "?", 
                "Message: ", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                tabForecast.Content = loadingPage.Content;
                ObservableCollection<MediaPlanTuple> mpTuplesToCopy = _forecast.dgMediaPlans._allMediaPlans;
                await _forecast.MakeNewVersion(mpTuplesToCopy);
                tabForecast.Content = _forecast.Content;
            }

        }

        private void _forecast_SetLoadingPage(object? sender, ChangeVersionEventArgs e)
        {
            tabForecast.Content = loadingPage.Content;
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

    }
}
