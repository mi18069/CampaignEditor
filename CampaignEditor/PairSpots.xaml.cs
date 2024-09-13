using CampaignEditor.Controllers;
using CampaignEditor.UserControls.ValidationItems.PairSpotsItems;
using Database.DTOs.CampaignDTO;
using Database.DTOs.RealizedSpotDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.SpotPairDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for PairSpots.xaml
    /// </summary>
    public partial class PairSpots : Window
    {
        private CampaignDTO _campaign;
        private SpotPairController _spotPairController;
        private RealizedSpotController _realizedSpotController;
        List<EqualSpots> equalSpots = new List<EqualSpots>();
        public PairSpots(ISpotPairRepository spotPairRepository,
            IRealizedSpotRepository realizedSpotRepository)
        {
            InitializeComponent();

            _spotPairController = new SpotPairController(spotPairRepository);
            _realizedSpotController = new RealizedSpotController(realizedSpotRepository);
        }

        public async Task Initialize(CampaignDTO campaign, IEnumerable<SpotDTO> spots, IEnumerable<int> spotNums)
        {
            _campaign = campaign;
            equalSpots.Clear();
            Dictionary<SpotDTO, List<RealizedSpotDTO>> dictionary = new Dictionary<SpotDTO, List<RealizedSpotDTO>>();
            foreach (var spot in spots)
            {
                dictionary[spot] = new List<RealizedSpotDTO>();
            }
            var unpairedSpot = new SpotDTO(_campaign.cmpid, "None", "Unpaired", 0, false);
            var spotsForPairing = new List<SpotDTO>(spots);
            spotsForPairing.Insert(0, unpairedSpot);

            // Add empty spot for when we have null values
            dictionary[unpairedSpot] = new List<RealizedSpotDTO>();

            List<RealizedSpotDTO> realizedSpots = new List<RealizedSpotDTO>();
            foreach (var spotnum in spotNums)
            {
                var realizedSpot = await _realizedSpotController.GetRealizedSpot(spotnum);
                realizedSpots.Add(realizedSpot);
            }

            // Initialize EqualSpot pairs
            var spotPairs = await _spotPairController.GetAllCampaignSpotPairs(_campaign.cmpid);

            // Make dictionary where keys are expected spots and values list of realized spots
            foreach (var spotNum in spotNums)
            {
                var realizedSpot = realizedSpots.First(rs => rs.spotnum == spotNum);
                if (!spotPairs.Any(sp => sp.spotnum == spotNum))
                {
                    await _spotPairController.CreateSpotPair(new CreateSpotPairDTO(_campaign.cmpid, null, spotNum));
                    dictionary[unpairedSpot].Add(realizedSpot);
                }
                else
                {
                    var spotcode = spotPairs.First(sp => sp.spotnum == spotNum).spotcode;
                    if (spotcode == null)
                    {
                        dictionary[unpairedSpot].Add(realizedSpot);
                    }
                    else
                    {
                        var spot = spots.First(s => s.spotcode == spotcode);
                        dictionary[spot].Add(realizedSpot);
                    }
                }
            }

            foreach (SpotDTO expectedSpot in dictionary.Keys)
            {
                var equalSpot = new EqualSpots(expectedSpot, dictionary[expectedSpot]);
                equalSpots.Add(equalSpot); 
            }

            var spotcodes = spotsForPairing.Select(s => s.spotcode);

            spotsPannel.Initialize(equalSpots, spotcodes);
            spotsPannel.AssignedSpotChanged += SpotsPannel_AssignedSpotChanged;
        }

        private async void SpotsPannel_AssignedSpotChanged(object? sender, Helpers.RealizedSpotPairChangedEventArgs e)
        {
            string? newSpotcode = e.Spotcode;
            var realizedSpot = e.RealizedSpot;
            if (string.Compare(newSpotcode, "None") == 0)
                newSpotcode = null;
            await _spotPairController.UpdateSpotPair(new UpdateSpotPairDTO(_campaign.cmpid, newSpotcode, realizedSpot.spotnum));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            spotsPannel.UnbindEvents();
        }
    }
}
