using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using CampaignEditor.UserControls;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CampaignEditor
{
    public partial class Campaign : Window
    {
        public string cmpname = "";
        ClientDTO _client = null;
        CampaignDTO _campaign = null;
        bool readOnly = true;

        private readonly IAbstractFactory<CampaignOverview> _factoryOverview;
        private readonly IAbstractFactory<CampaignForecast> _factoryForecast;

        private ClientController _clientController;
        private CampaignController _campaignController;

        private CampaignOverview factoryCampaignOverview = null;
        private CampaignForecast factoryCampaignForecast = null;
        private object currentPage = null;


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

            factoryCampaignOverview = _factoryOverview.Create();
            factoryCampaignOverview.Initialization(_client, _campaign, isReadOnly);
            btnOverview_Click(btnOverview, null);
        }

        private void btnOverview_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != factoryCampaignOverview)
            {
                currentPage = factoryCampaignOverview;
                ClientCampaign.Content = currentPage;
            }
        }

        private async void btnForecast_Click(object sender, RoutedEventArgs e)
        {
            if (factoryCampaignForecast == null)
            {
                factoryCampaignForecast = _factoryForecast.Create();
                await factoryCampaignForecast.Initialize(_client, _campaign);
            }
            if (currentPage != factoryCampaignForecast)
            {
                currentPage = factoryCampaignForecast;
                ClientCampaign.Content = currentPage;
            }
        }

    }
}
