using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
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

        //private readonly IAbstractFactory<CampaignOverview> _factoryOverview;
        private readonly IAbstractFactory<CampaignForecastView> _factoryForecastView;
        private readonly IAbstractFactory<CampaignValidation> _factoryValidation;

        private ClientController _clientController;
        private CampaignController _campaignController;

        CampaignOverview factoryCampaignOverview;


        public Campaign(IClientRepository clientRepository, ICampaignRepository campaignRepository, 
            IAbstractFactory<CampaignOverview> factoryOverview, 
            IAbstractFactory<CampaignForecastView> factoryForecastView,
            IAbstractFactory<CampaignValidation> factoryValidation)
        {

            factoryCampaignOverview = factoryOverview.Create();
            _factoryForecastView = factoryForecastView;
            _factoryValidation = factoryValidation;

            _clientController = new ClientController(clientRepository);
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();


        }

        public async Task Initialize(CampaignDTO campaign, bool isReadOnly)
        {

            _campaign = campaign;
            _client = await _clientController.GetClientById(_campaign.clid);
            readOnly = isReadOnly || (TimeFormat.YMDStringToDateTime(_campaign.cmpedate) < DateTime.Now);
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

            var factoryCampaignForecastView = _factoryForecastView.Create();
            factoryCampaignForecastView.tabForecast = tabForecast;
            await factoryCampaignForecastView.Initialize(_campaign, readOnly);

            var factoryCampaignValidation = _factoryValidation.Create();
            await factoryCampaignValidation.Initialize(_campaign);
            tabValidation.Content = factoryCampaignValidation.Content;


        }

        private void Page_ClosePageEvent(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // event unbinding
            factoryCampaignOverview.ClosePageEvent -= Page_ClosePageEvent;

            CampaignEventLinker.RemoveCampaign(_campaign.cmpid);
        }

    }
}
