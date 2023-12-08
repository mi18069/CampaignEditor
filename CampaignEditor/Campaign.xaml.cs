using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
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
        public string cmpname = "";
        ClientDTO _client = null;
        CampaignDTO _campaign = null;
        bool readOnly = true;

        private readonly IAbstractFactory<CampaignOverview> _factoryOverview;
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

            _factoryOverview = factoryOverview;
            _factoryForecastView = factoryForecastView;
            _factoryValidation = factoryValidation;

            _clientController = new ClientController(clientRepository);
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();


        }

        public async Task Initialize(string campaignName, bool isReadOnly)
        {

            cmpname = campaignName;
            _campaign = await _campaignController.GetCampaignByName(campaignName);
            _client = await _clientController.GetClientById(_campaign.clid);
            readOnly = isReadOnly || (TimeFormat.YMDStringToDateTime(_campaign.cmpedate) < DateTime.Now);
            this.Title = "Client: " + _client.clname.Trim() + "  Campaign: " + _campaign.cmpname.Trim();

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


            var factoryCampaignOverview = _factoryOverview.Create();
            await factoryCampaignOverview.Initialization(_client, _campaign, readOnly);
            tabOverview.Content = factoryCampaignOverview.Content;

            var factoryCampaignForecastView = _factoryForecastView.Create();
            factoryCampaignForecastView.tabForecast = tabForecast;
            await factoryCampaignForecastView.Initialize(_campaign, readOnly);

            var factoryCampaignValidation = _factoryValidation.Create();
            await factoryCampaignValidation.Initialize(_campaign);
            tabValidation.Content = factoryCampaignValidation.Content;


        }     

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            bool shouldClose = true;
            if (factoryCampaignOverview != null)
            {
                shouldClose = factoryCampaignOverview.Window_Closing();
                CampaignEventLinker.RemoveCampaign(_campaign.cmpid);
            }
            if (!shouldClose)
            {
                e.Cancel = true;
            }

        }

    }
}
