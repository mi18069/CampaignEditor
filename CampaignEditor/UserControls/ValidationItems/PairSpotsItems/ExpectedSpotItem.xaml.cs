using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ValidationItems.PairSpotsItems
{
    /// <summary>
    /// Interaction logic for ExpectedSpotItem.xaml
    /// </summary>
    public partial class ExpectedSpotItem : UserControl
    {
        private SpotDTO _spot = null;
        public ExpectedSpotItem(SpotDTO spot)
        {
            InitializeComponent();
            _spot = spot;
            SetControl(_spot);
        }

        private void SetControl(SpotDTO spot)
        {
            lblSpot.Content = $"{_spot.spotcode}: {_spot.spotname.Trim()}";
            lblSpotlength.Content = $"Length: {_spot.spotlength}";
        }
    }
}
