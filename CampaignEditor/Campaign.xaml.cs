using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Entities;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class Campaign : Window
    {

        ClientDTO _client = null;
        CampaignDTO _campaign = null;
        bool isReadOnly = true;

        private readonly IAbstractFactory<CampaignOverview> _factoryOverview;

        private ClientController _clientController;
        private CampaignController _campaignController;

        private CampaignOverview factoryCampaignOverview = null;
        private object currentPage = null;


        public Campaign(IClientRepository clientRepository, ICampaignRepository campaignRepository, 
            IAbstractFactory<CampaignOverview> factoryOverview)
        {
            _factoryOverview = factoryOverview;

            _clientController = new ClientController(clientRepository);
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();
        }

        public async Task Initialize(string campaignName, bool isReadOnly)
        {
            _campaign = await _campaignController.GetCampaignByName(campaignName);
            _client = await _clientController.GetClientById(_campaign.clid);
            isReadOnly = isReadOnly;
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

        private void btnForecast_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
