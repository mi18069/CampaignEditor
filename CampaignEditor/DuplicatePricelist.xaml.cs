using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Entities;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.PricelistChannels;
using Database.DTOs.PricelistDTO;
using Database.DTOs.PricesDTO;
using Database.DTOs.SeasonalityDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.TargetDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for DuplicatePricelist.xaml
    /// </summary>
    public partial class DuplicatePricelist : Window
    {

        PricelistDTO _pricelist;

        private ClientController _clientController;
        private UserClientsController _userClientsController;
        private PricelistController _pricelistController;
        private PricelistChannelsController _pricelistChannelsController;
        private SectableController _sectableController;
        private SectablesController _sectablesController;
        private SeasonalityController _seasonalityController;
        private SeasonalitiesController _seasonalitiesController;
        private PricesController _pricesController;
        private TargetController _targetController;

        public DuplicatePricelist(IClientRepository clientRepository, IUserClientsRepository userClientsRepository,
            IPricelistRepository pricelistRepository,
            IPricelistChannelsRepository pricelistChannelsRepository, 
            ISectableRepository sectableRepository, ISectablesRepository sectablesRepository,
            ISeasonalityRepository seasonalityRepository, ISeasonalitiesRepository seasonalitiesRepository,
            IPricesRepository pricesRepository, ITargetRepository targetRepository)
        {
            InitializeComponent();

            _clientController = new ClientController(clientRepository);
            _userClientsController = new UserClientsController(userClientsRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _sectableController = new SectableController(sectableRepository);
            _sectablesController = new SectablesController(sectablesRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _seasonalitiesController = new SeasonalitiesController(seasonalitiesRepository);
            _pricesController = new PricesController(pricesRepository);
            _targetController = new TargetController(targetRepository);

        }

        public async Task Initialize(PricelistDTO pricelist)
        {
            _pricelist = pricelist;
            await FillClients();
        }        

        private async Task FillClients()
        {
            var clids = new List<int>();
            var user = MainWindow.user;
            if (user == null || user.usrlevel < 0)
            {
                clids = (await _clientController.GetAllClients()).Select(client => client.clid).ToList();
            }
            else
                clids = (await _userClientsController.GetAllUserClientsByUserId(user.usrid)).Select(c => c.cliid).ToList();

            var clients = new List<ClientDTO>();
            foreach (var clid in clids)
            {
                clients.Add(await _clientController.GetClientById(clid));
            }
            clients = clients.OrderBy(cl => cl.clname).ToList();
            cbClients.ItemsSource = clients;
        }

        public async Task DuplicatePricelistToClient(PricelistDTO pricelist, ClientDTO client)
        {
            var newPricelist = await DuplicatePricelistInfoToClient(pricelist, client.clid);
            if (newPricelist != null)
            {
                await DuplicatePricelistChannels(pricelist, newPricelist);
                await DuplicateDayParts(pricelist, newPricelist);               
            }
        }

        private async Task<PricelistDTO> DuplicatePricelistInfoToClient(PricelistDTO pricelist, int clid)
        {
            CreatePricelistDTO createPricelist = new CreatePricelistDTO(pricelist);
            createPricelist.clid = clid;
            var newPricelist =  await _pricelistController.CreatePricelist(createPricelist);
            return newPricelist;
        }

        private async Task DuplicatePricelistChannels(PricelistDTO pricelist, PricelistDTO newPricelist)
        {
            var channels = await _pricelistChannelsController.GetAllPricelistChannelsByPlid(pricelist.plid);
            foreach (var channel in channels)
            {
                await _pricelistChannelsController.CreatePricelistChannels(
                    new CreatePricelistChannelsDTO(newPricelist.plid, channel.chid, channel.chcoef));
            }
        }

        private async Task DuplicateDayParts(PricelistDTO pricelist, PricelistDTO newPricelist)
        {
            var dayParts = await _pricesController.GetAllPricesByPlId(pricelist.plid);
            foreach (var dayPart in dayParts)
            {
                await _pricesController.CreatePrices(new CreatePricesDTO(newPricelist.plid, dayPart.dps, dayPart.dpe, dayPart.price, dayPart.ispt, dayPart.days));
            }
        }

        private async void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (cbClients.SelectedIndex != -1)
            {
                try
                {
                    ClientDTO client = cbClients.SelectedItem as ClientDTO;
                    if (client != null)
                    {
                        await DuplicatePricelistToClient(_pricelist, client);
                        MessageBox.Show("Pricelist \n" + _pricelist.plname.Trim() + "\n Succesfully added to client\n" + client.clname.Trim(),
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while duplicating pricelist:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
