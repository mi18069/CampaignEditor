using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    public partial class CampaignForecast : Page
    {
        private SchemaController _schemaController;
        private ChannelController _channelController;

        private ClientDTO _client;
        private CampaignDTO _campaign;

        private DateTime initFrom;
        private DateTime initTo;

        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository)
        {
            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);

            InitializeComponent();
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;

            DateTime now =  DateTime.Now;
            dpFrom.SelectedDate = now;
            dpTo.SelectedDate = now;
        }

        // When we initialize forecast, we need to do this
        private void Init_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if ((DateTime)dpFrom.SelectedDate! < (DateTime)dpTo.SelectedDate!)
            {
                initFrom = (DateTime)dpFrom.SelectedDate!;
                initTo = (DateTime)dpTo.SelectedDate!;

                gridInit.Visibility = Visibility.Hidden;
                gridForecast.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Invalid dates");
            }
            


        }
    
    
    }
}
