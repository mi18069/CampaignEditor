using CampaignEditor.Helpers;
using Database.DTOs.RealizedSpotDTO;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ValidationItems.PairSpotsItems
{
    /// <summary>
    /// Interaction logic for RealizedSpotItem.xaml
    /// </summary>
    public partial class RealizedSpotItem : UserControl
    {
        private RealizedSpotDTO _spot;
        public event EventHandler<RealizedSpotPairChangedEventArgs> AssignedSpotChanged;

        public RealizedSpotItem(RealizedSpotDTO spot, IEnumerable<string> spotcodesList)
        {
            InitializeComponent();
            _spot = spot;
            SetControl(_spot, spotcodesList);
        }

        private void SetControl(RealizedSpotDTO spot, IEnumerable<string> spotcodesList)
        {
            lblSpot.Text = $"{spot.spotname}";
            lblSpotlength.Content = $"Length: {spot.spotlength}";
            cbAssignedSpot.ItemsSource = null;
            cbAssignedSpot.ItemsSource = spotcodesList;
        }

        private void cbAssignedSpot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string spotcode = cbAssignedSpot.SelectedValue.ToString()!.Trim();
            AssignedSpotChanged?.Invoke(this, new RealizedSpotPairChangedEventArgs(_spot, spotcode));
        }

        public int GetSpotnum()
        {
            return _spot.spotnum;
        }
    }
}
