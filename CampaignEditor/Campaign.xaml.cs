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

namespace CampaignEditor
{
    public partial class Campaign : Window
    {
        public CampaignDTO _campaign = null;
        ClientDTO _client = null;
        bool readOnly = true;
        // Variable for checking if forecast is initialized
        bool isCampaignInitialized = false;

        //private readonly IAbstractFactory<CampaignOverview> _factoryOverview;
        private readonly IAbstractFactory<CampaignForecastView> _factoryForecastView;

        private ClientController _clientController;
        private CampaignController _campaignController;

        CampaignOverview factoryCampaignOverview;
        CampaignForecastView factoryCampaignForecastView;
        CampaignValidation factoryCampaignValidation;

        public Campaign(IClientRepository clientRepository, ICampaignRepository campaignRepository, 
            IAbstractFactory<CampaignOverview> factoryOverview, 
            IAbstractFactory<CampaignForecastView> factoryForecastView,
            IAbstractFactory<CampaignValidation> factoryValidation)
        {

            factoryCampaignOverview = factoryOverview.Create();
            factoryCampaignForecastView = factoryForecastView.Create();
            factoryCampaignValidation = factoryValidation.Create();


            _clientController = new ClientController(clientRepository);
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();


        }

        public async Task Initialize(CampaignDTO campaign, bool isReadOnly)
        {

            _campaign = campaign;
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

            //var factoryCampaignForecastView = _factoryForecastView.Create();
            factoryCampaignForecastView.tabForecast = tabForecast;
            await factoryCampaignForecastView.Initialize(_campaign, readOnly);
            BindOverviewForecastEvents();
            isCampaignInitialized = factoryCampaignForecastView.alreadyExists;

            await factoryCampaignValidation.Initialize(_campaign);
            tabValidation.Content = factoryCampaignValidation.Content;

            factoryCampaignForecastView.UpdateValidation += ForecastView_UpdateValidation;
        }

        private void BindOverviewForecastEvents()
        {
            factoryCampaignOverview.GoalsUpdatedEvent += CampaignForecastView_GoalsUpdatedEvent;
            factoryCampaignOverview.ChannelsUpdatedEvent += CampaignForecastView_ChannelsUpdatedEvent;
            factoryCampaignOverview.PricelistUpdatedEvent += CampaignForecastView_PricelistUpdatedEvent;
            factoryCampaignOverview.SpotsUpdatedEvent += CampaignForecastView_SpotsUpdatedEvent;
            factoryCampaignOverview.DayPartsUpdatedEvent += CampaignForecastView_DayPartsUpdatedEvent;
        }

        private void UnbindOverviewForecastEvents()
        {
            factoryCampaignOverview.GoalsUpdatedEvent -= CampaignForecastView_GoalsUpdatedEvent;
            factoryCampaignOverview.ChannelsUpdatedEvent -= CampaignForecastView_ChannelsUpdatedEvent;
            factoryCampaignOverview.PricelistUpdatedEvent -= CampaignForecastView_PricelistUpdatedEvent;
            factoryCampaignOverview.SpotsUpdatedEvent -= CampaignForecastView_SpotsUpdatedEvent;
            factoryCampaignOverview.DayPartsUpdatedEvent -= CampaignForecastView_DayPartsUpdatedEvent;

        }

        private async void CampaignForecastView_DayPartsUpdatedEvent(object? sender, EventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            await factoryCampaignForecastView.UpdateDayParts();
        }

        private async void CampaignForecastView_SpotsUpdatedEvent(object? sender, EventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

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

            await factoryCampaignForecastView.UpdateChannels(channelsToDelete, channelsToAdd);
        }

        private async void CampaignForecastView_PricelistUpdatedEvent(object? sender, UpdatePricelistEventArgs e)
        {
            if (!isCampaignInitialized)
            {
                return;
            }

            var pricelist = e.pricelist;

            await factoryCampaignForecastView.UpdatePricelist(pricelist);
        }

        private async void ForecastView_UpdateValidation(object? sender, EventArgs e)
        {
            await factoryCampaignValidation.Initialize(_campaign);
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
            UnbindOverviewForecastEvents();

            CampaignEventLinker.RemoveCampaign(_campaign.cmpid);
        }

    }
}
