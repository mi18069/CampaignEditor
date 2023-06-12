using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using CampaignEditor.UserControls;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class Campaign : Window
    {
        bool isFirstRender = true;

        public string cmpname = "";
        ClientDTO _client = null;
        CampaignDTO _campaign = null;
        bool readOnly = true;

        private readonly IAbstractFactory<CampaignOverview> _factoryOverview;
        private readonly IAbstractFactory<CampaignForecast> _factoryForecast;

        private ClientController _clientController;
        private CampaignController _campaignController;



        public Campaign(IClientRepository clientRepository, ICampaignRepository campaignRepository, 
            IAbstractFactory<CampaignOverview> factoryOverview, IAbstractFactory<CampaignForecast> factoryForecast)
        {
            _factoryOverview = factoryOverview;
            _factoryForecast = factoryForecast;

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

            await AssignPagesToTabs();
            
        }

        private async Task AssignPagesToTabs()
        {
            var factoryCampaignOverview = _factoryOverview.Create();
            await factoryCampaignOverview.Initialization(_client, _campaign, readOnly);

            var factoryCampaignForecast = _factoryForecast.Create();
            await factoryCampaignForecast.Initialize(_client, _campaign);

            TabItem tabOverview = (TabItem)tcTabs.FindName("tiOverview");
            tabOverview.Content = factoryCampaignOverview.Content;

            TabItem tabForecast = (TabItem)tcTabs.FindName("tiForecast");
            tabForecast.Content = factoryCampaignForecast.Content;
        }

        // For resizing tabItems width
        private void tcTabs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          /* int n = tcTabs.Items.Count;

            double tabSize = tcTabs.ActualWidth / n;
            foreach (TabItem tab in tcTabs.Items)
            {
                tab.Width = tabSize-5;
            }*/
        }
    }
}
