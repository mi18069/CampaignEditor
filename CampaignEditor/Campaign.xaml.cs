using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class Campaign : Window
    {
        bool isFirstRender = true;
        bool loaded = false;

        public string cmpname = "";
        ClientDTO _client = null;
        CampaignDTO _campaign = null;
        bool readOnly = true;

        private readonly IAbstractFactory<CampaignOverview> _factoryOverview;
        private readonly IAbstractFactory<CampaignForecastView> _factoryForecastView;

        private ClientController _clientController;
        private CampaignController _campaignController;



        public Campaign(IClientRepository clientRepository, ICampaignRepository campaignRepository, 
            IAbstractFactory<CampaignOverview> factoryOverview, IAbstractFactory<CampaignForecastView> factoryForecastView)
        {
            _factoryOverview = factoryOverview;
            _factoryForecastView = factoryForecastView;

            _clientController = new ClientController(clientRepository);
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();
            
        }

        public async Task Initialize(string campaignName, bool isReadOnly)
        {
            cmpname = campaignName;
            _campaign = await _campaignController.GetCampaignByName(campaignName);
            _client = await _clientController.GetClientById(_campaign.clid);
            readOnly = isReadOnly;
            this.Title = "Client: " + _client.clname.Trim() + "  Campaign: " + _campaign.cmpname.Trim();


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

            var factoryCampaignOverview = _factoryOverview.Create();
            await factoryCampaignOverview.Initialization(_client, _campaign, readOnly);
            tabOverview.Content = factoryCampaignOverview.Content;

            var factoryCampaignForecastView = _factoryForecastView.Create();
            factoryCampaignForecastView.tabForecast = tabForecast;
            // Don't want to await this, as this will just slow down our campaign
            await factoryCampaignForecastView.Initialize(_campaign);
        }

    }
}
