using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Database.Entities;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;

namespace CampaignEditor
{
    public partial class Campaign : Window
    {
        public CampaignDTO _campaign = null;
        ClientDTO _client = null;
        bool readOnly = true;
        // Variable for checking if forecast is initialized
        bool isCampaignInitialized = false;


        private ClientController _clientController;

        CampaignOverview factoryCampaignOverview;
        CampaignForecastView factoryCampaignForecastView;
        CampaignValidation factoryCampaignValidation;

        private MediaPlanForecastData _forecastData;
        private MediaPlanConverter _mpConverter;

        private ObservableRangeCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableRangeCollection<MediaPlanTuple>();

        public Campaign(IClientRepository clientRepository, 
            IAbstractFactory<CampaignOverview> factoryOverview, 
            IAbstractFactory<CampaignForecastView> factoryForecastView,
            IAbstractFactory<CampaignValidation> factoryValidation,
            IAbstractFactory<MediaPlanForecastData> factoryForecastData,
            IAbstractFactory<MediaPlanConverter> factoryMpConverter)
        {

            factoryCampaignOverview = factoryOverview.Create();
            factoryCampaignForecastView = factoryForecastView.Create();
            factoryCampaignValidation = factoryValidation.Create();

            _forecastData = factoryForecastData.Create();
            _mpConverter = factoryMpConverter.Create();


            _clientController = new ClientController(clientRepository);

            InitializeComponent();


        }

        public async Task Initialize(CampaignDTO campaign, bool isReadOnly)
        {
            _campaign = campaign;

            await _forecastData.Initialize(_campaign);
            _mpConverter.Initialize(_forecastData);

            _client = await _clientController.GetClientById(_campaign.clid);
            int authenticationLevel = 2;
            try
            {
                if (MainWindow.user.usrlevel <= 0)
                    authenticationLevel = 0;
                else
                    authenticationLevel = Clients.UserClientsAuthentication[campaign.clid];
            }
            catch { }
            readOnly = isReadOnly || authenticationLevel == 2 || 
                (TimeFormat.YMDStringToDateTime(_campaign.cmpedate) < DateTime.Now);
            this.Title = "Client: " + _client.clname.Trim() + "  Campaign: " + _campaign.cmpname.Trim()
                 + (_campaign.tv ? " TV" : " Radio");

            CampaignEventLinker.AddCampaign(_campaign.cmpid);
            AssignPagesToTabs();

        }

        private async Task AssignPagesToTabs()
        {
            //Placing loading page
            var loadingPage = new LoadingPage();

            TabItem tabOverview = (TabItem)tcTabs.FindName("tiOverview");
            tabOverview.Content = loadingPage.Content;

            TabItem tabForecast = (TabItem)tcTabs.FindName("tiForecast");
            tabForecast.Content = loadingPage.Content;

            TabItem tabValidation = (TabItem)tcTabs.FindName("tiValidation");
            tabValidation.Content = loadingPage.Content;


            factoryCampaignOverview.ClosePageEvent += Page_ClosePageEvent;
            await factoryCampaignOverview.Initialization(_client, _campaign, readOnly);
            tabOverview.Content = factoryCampaignOverview.Content;

            factoryCampaignForecastView.tabForecast = tabForecast;
            factoryCampaignForecastView._forecastData = _forecastData;
            factoryCampaignForecastView._mpConverter = _mpConverter;
            factoryCampaignForecastView._allMediaPlans = _allMediaPlans;
            await factoryCampaignForecastView.Initialize(_campaign, readOnly);
            BindOverviewForecastEvents();
            isCampaignInitialized = factoryCampaignForecastView.alreadyExists;

            factoryCampaignValidation._forecastData = _forecastData;
            factoryCampaignValidation._mpConverter = _mpConverter;
            factoryCampaignValidation.RealizationsAcquired += FactoryCampaignValidation_RealizationsAcquired;
            await factoryCampaignValidation.Initialize(_campaign, _allMediaPlans);
            tabValidation.Content = factoryCampaignValidation.Content;
            factoryCampaignValidation.SetLoadingPage += FactoryCampaignValidation_SetLoadingPage;
            factoryCampaignValidation.SetContentPage += FactoryCampaignValidation_SetContentPage;
            
            factoryCampaignForecastView.UpdateValidation += ForecastView_UpdateValidation;
            factoryCampaignForecastView.UpdateTermDateAndChannel += FactoryCampaignForecastView_UpdateTermDateAndChannel;

            factoryCampaignValidation.UpdatedRealization += FactoryCampaignValidation_UpdatedRealization;
            
            BindEvents();
        }



        private void FactoryCampaignForecastView_UpdateTermDateAndChannel(object? sender, UpdatedTermDateAndChannelEventArgs e)
        {
            var date = e.Date;
            var channel = e.Channel;
            TermUpdatedInForecast(date, channel);
        }

        private void FactoryCampaignValidation_SetContentPage(object? sender, EventArgs e)
        {
            TabItem tabValidation = (TabItem)tcTabs.FindName("tiValidation");
            tabValidation.Content = factoryCampaignValidation.Content;
        }

        private void FactoryCampaignValidation_RealizationsAcquired(object? sender, EventArgs e)
        {
            if (factoryCampaignValidation == null || factoryCampaignForecastView == null)
                return;

            var mpRealized = factoryCampaignValidation.MediaPlanRealized;
            if (mpRealized.Count == 0)
                return;

            factoryCampaignForecastView.AddRealizations(mpRealized);
        }

        private void FactoryCampaignValidation_SetLoadingPage(object? sender, EventArgs e)
        {
            var loadingPage = new LoadingPage();
            TabItem tabValidation = (TabItem)tcTabs.FindName("tiValidation");
            tabValidation.Content = loadingPage.Content;
        }

        private void FactoryCampaignValidation_UpdatedRealization(object? sender, UpdatedRealizationEventArgs e)
        {
            var date = e.Date;
            var chrdsid = e.Chrdsid;
            var spot = e.Spot;
            RealizationUpdatedInValidation(date, chrdsid, spot);
        }

        private void BindEvents()
        {
            factoryCampaignOverview.ChannelsOrderChangedEvent += Campaign_ChannelsOrderChangedEvent;
        }

        private void UnbindEvents()
        {
            factoryCampaignOverview.ChannelsOrderChangedEvent -= Campaign_ChannelsOrderChangedEvent;
        }

        private void BindOverviewForecastEvents()
        {
            factoryCampaignOverview.CampaignUpdatedEvent += CampaignForecastView_CampaignUpdatedEvent;
            factoryCampaignOverview.GoalsUpdatedEvent += CampaignForecastView_GoalsUpdatedEvent;
            factoryCampaignOverview.ChannelsUpdatedEvent += CampaignForecastView_ChannelsUpdatedEvent;
            factoryCampaignOverview.PricelistUpdatedEvent += CampaignForecastView_PricelistUpdatedEvent;
            factoryCampaignOverview.TargetsUpdatedEvent += CampaignForecastView_TargetsUpdatedEvent;
            factoryCampaignOverview.SpotsUpdatedEvent += CampaignForecastView_SpotsUpdatedEvent;
            factoryCampaignOverview.DayPartsUpdatedEvent += CampaignForecastView_DayPartsUpdatedEvent;
            factoryCampaignOverview.CobrandsUpdatedEvent += CampaignForecastView_CobrandsUpdatedEvent;
        }

        private void UnbindOverviewForecastEvents()
        {
            factoryCampaignOverview.CampaignUpdatedEvent -= CampaignForecastView_CampaignUpdatedEvent;
            factoryCampaignOverview.GoalsUpdatedEvent -= CampaignForecastView_GoalsUpdatedEvent;
            factoryCampaignOverview.ChannelsUpdatedEvent -= CampaignForecastView_ChannelsUpdatedEvent;
            factoryCampaignOverview.PricelistUpdatedEvent -= CampaignForecastView_PricelistUpdatedEvent;
            factoryCampaignOverview.TargetsUpdatedEvent -= CampaignForecastView_TargetsUpdatedEvent;
            factoryCampaignOverview.SpotsUpdatedEvent -= CampaignForecastView_SpotsUpdatedEvent;
            factoryCampaignOverview.DayPartsUpdatedEvent -= CampaignForecastView_DayPartsUpdatedEvent;
            factoryCampaignOverview.CobrandsUpdatedEvent -= CampaignForecastView_CobrandsUpdatedEvent;


        }


        private async void CampaignForecastView_CampaignUpdatedEvent(object? sender, UpdateCampaignEventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            var campaign = e.Campaign;
            if (campaign == null)
            {
                return;
            }

            _forecastData.Campaign = campaign;
            await factoryCampaignForecastView.UpdateCampaign(campaign);
        }

        private async void CampaignForecastView_DayPartsUpdatedEvent(object? sender, EventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            await _forecastData.InitializeDayParts();
            _mpConverter.Initialize(_forecastData);
            await factoryCampaignForecastView.UpdateDayParts();
        }

        private async void CampaignForecastView_CobrandsUpdatedEvent(object? sender, UpdateCobrandsEventArgs e)
        {
            await _forecastData.InitializeCobrands();
            _mpConverter.Initialize(_forecastData);
            var cobrands = e.Cobrands;

            if (isCampaignInitialized)
            {
                factoryCampaignForecastView.UpdateCobrands(cobrands);
            }


            if (factoryCampaignValidation != null)
                factoryCampaignValidation.CobrandsChanged(cobrands);




        }

        private async void CampaignForecastView_TargetsUpdatedEvent(object? sender, EventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            await _forecastData.InitializeTargets();
            await factoryCampaignForecastView.UpdateTargets();
        }

        private async void CampaignForecastView_SpotsUpdatedEvent(object? sender, EventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            await _forecastData.InitializeSpots();
            _mpConverter.Initialize(_forecastData);
            await factoryCampaignForecastView.UpdateSpots();
        }

        private async void CampaignForecastView_GoalsUpdatedEvent(object? sender, EventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            await factoryCampaignForecastView.UpdateGoals();
        }

        private async void CampaignForecastView_ChannelsUpdatedEvent(object? sender, UpdateChannelsEventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }
            var channelsToDelete = e.channelsToDelete;
            var channelsToAdd = e.channelsToAdd;

            await _forecastData.InitializeChannels(true);
            _mpConverter.Initialize(_forecastData);
            await factoryCampaignForecastView.UpdateChannels(channelsToDelete, channelsToAdd);
        }

        private async void Campaign_ChannelsOrderChangedEvent(object? sender, EventArgs e)
        {
            await _forecastData.InitializeChannels();
            // For Forecast
            if (isCampaignInitialized)
            {
                factoryCampaignForecastView.ChannelsOrderChanged();
            }
            // For validation
            if (factoryCampaignValidation != null)
            {
                factoryCampaignValidation.FillChannels();
            }

        }

        private async void CampaignForecastView_PricelistUpdatedEvent(object? sender, UpdatePricelistEventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            var pricelist = e.pricelist;
            await _forecastData.InitializePricelists();
            _mpConverter.Initialize(_forecastData);
            await factoryCampaignForecastView.UpdatePricelist(pricelist);
        }

        private async void ForecastView_UpdateValidation(object? sender, EventArgs e)
        {
            await factoryCampaignValidation.Initialize(_campaign, _allMediaPlans);
            if (!isCampaignInitialized)
                isCampaignInitialized = true;
        }


        private void Page_ClosePageEvent(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // event unbinding
            factoryCampaignOverview.ClosePageEvent -= Page_ClosePageEvent;
            factoryCampaignForecastView.CloseForecast();
            factoryCampaignForecastView.UpdateValidation -= ForecastView_UpdateValidation;
            factoryCampaignForecastView.UpdateTermDateAndChannel -= FactoryCampaignForecastView_UpdateTermDateAndChannel;
            UnbindOverviewForecastEvents();
            UnbindEvents();
            factoryCampaignValidation.CloseValidation();

            CampaignEventLinker.RemoveCampaign(_campaign.cmpid);
        }

        private async void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcTabs.SelectedIndex == 2)
            {
                if (factoryCampaignValidation == null)
                    return;

                await factoryCampaignValidation.CheckUpdatedTerms();
            }
            else if (tcTabs.SelectedIndex == 1)
            {
                if (factoryCampaignForecastView == null)
                    return;

                factoryCampaignForecastView.CheckUpdatedRealizations();
            }

        }

        // Different approach would be to pass the term, and in validation extract informations 
        // for date and channel. That proach would enable not to propagate changes
        // if some term is changed back to original state, but with additional checkings
        private void TermUpdatedInForecast(DateOnly date, ChannelDTO channel)
        {
            if (factoryCampaignValidation == null)
                return;

            factoryCampaignValidation.AddIntoUpdatedTerms(date, channel);
        }

        // When we change some realization, we want to delegate that change to Forecast
        private void RealizationUpdatedInValidation(DateOnly date, int chrdsid, SpotDTO spot)
        {
            if (factoryCampaignForecastView == null)
                return;

            factoryCampaignForecastView.AddIntoUpdatedRealizations(date, chrdsid, spot);
        }
    }
}
