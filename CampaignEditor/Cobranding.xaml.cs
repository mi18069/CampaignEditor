using CampaignEditor.Controllers;
using CampaignEditor.Entities;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.CobrandDTO;
using Database.DTOs.SpotDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for Cobranding.xaml
    /// </summary>
    public partial class Cobranding : Window
    {
        private CampaignDTO _campaign;
        private CobrandController _cobrandController;

        public Cobranding(ICobrandRepository cobrandRepository)
        {
            InitializeComponent();

            _cobrandController = new CobrandController(cobrandRepository);
        }

        public void Initialize(CampaignDTO campaign, IEnumerable<CobrandDTO> cobrands, IEnumerable<ChannelDTO> channels, IEnumerable<SpotDTO> spots)
        {
            _campaign = campaign;

            // Making all necessary ChannelSpotModels
            var spotcodes = spots.Select(s => s.spotcode.Trim()[0]).ToList();
            List<ChannelSpotModel> data = new List<ChannelSpotModel>();

            foreach (var channel in channels)
            {
                var channelSpotModel = new ChannelSpotModel(channel, spotcodes);
                data.Add(channelSpotModel);
            }

            // Adding initial coefs
            foreach (var cobrand in cobrands)
            {
                var channelSpotModel = data.FirstOrDefault(d => d.Channel.chid == cobrand.chid);
                
                if (channelSpotModel == null)
                    continue; // This shouldn't happen, when we delete channel then delete cobrand

                channelSpotModel.InitializeCoef(cobrand.spotcode, cobrand.coef);
            }

            GenerateDynamicColumns(spotcodes);
            dgGrid.ItemsSource = data;
        }

        private void GenerateDynamicColumns(IEnumerable<char> spotcodes)
        {
            foreach (var spotcode in spotcodes)
            {
                // Create a new DataGridTextColumn for each spot
                var column = new DataGridTextColumn
                {
                    Header = spotcode,
                    Binding = new Binding($"SpotCoefficients[{spotcode}]") // Bind to the dictionary value
                };
                dgGrid.Columns.Add(column);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
