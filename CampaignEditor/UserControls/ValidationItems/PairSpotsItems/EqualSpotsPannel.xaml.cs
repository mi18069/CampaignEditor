using CampaignEditor.Helpers;
using Database.DTOs.RealizedSpotDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ValidationItems.PairSpotsItems
{
    /// <summary>
    /// Interaction logic for EqualSpotsPannel.xaml
    /// </summary>
    public partial class EqualSpotsPannel : UserControl
    {
        List<EqualSpots> _equalSpots = new List<EqualSpots>();
        List<string> _spotcodes = new List<string>();
        public event EventHandler<RealizedSpotPairChangedEventArgs> AssignedSpotChanged;

        public EqualSpotsPannel()
        {
            InitializeComponent();
        }

        public void Initialize(IEnumerable<EqualSpots> equalSpots, IEnumerable<string> spotcodes)
        {
            _equalSpots.AddRange(equalSpots);
            _spotcodes.AddRange(spotcodes);
            SetControl();
        }

        private void SetControl()
        {
            spEqualSpots.Children.Clear();
            foreach (var equalSpots in _equalSpots)
            {
                EqualSpotsItem equalSpotsItem = new EqualSpotsItem(equalSpots, _spotcodes);
                equalSpotsItem.AssignedSpotChanged += EqualSpotsItem_AssignedSpotChanged;
                spEqualSpots.Children.Add(equalSpotsItem);
            }
        }

        private void EqualSpotsItem_AssignedSpotChanged(object? sender, Helpers.RealizedSpotPairChangedEventArgs e)
        {
            var equalSpotsItem = sender as EqualSpotsItem;
            var realizedSpot = e.RealizedSpot;

            // Add to list
            var newRealizedSpot = new RealizedSpotDTO(realizedSpot);
            var expectedSpotcodeToAddInto = e.Spotcode;
            _equalSpots
                .First(es => es.ExpectedSpot.spotcode == expectedSpotcodeToAddInto)
                .RealizedSpots
                .Add(newRealizedSpot);

            // Add to user item
            var equalSpotsItemToAddInto = spEqualSpots.Children
                .Cast<EqualSpotsItem>()
                .First(esi => esi.GetExpectedSpotcode() == expectedSpotcodeToAddInto);
            equalSpotsItemToAddInto.AddRealized(newRealizedSpot);

            // Delete from list
            // Sender have information about spotcode of expected spot where we want to delete realized spot
            var expectedSpotcodeToDeleteFrom = equalSpotsItem.GetExpectedSpotcode();
            var expectedSpotsToDeleteFrom = _equalSpots
                .First(es => es.ExpectedSpot.spotcode == expectedSpotcodeToDeleteFrom);
            var realizedSpotToDelete = expectedSpotsToDeleteFrom
                .RealizedSpots
                .First(rs => rs.spotnum == realizedSpot.spotnum);
            expectedSpotsToDeleteFrom.RealizedSpots.Remove(realizedSpotToDelete);

            // Delete from user item
            equalSpotsItem.DeleteRealized(realizedSpot);

            // Send event to notify for changes in database
            AssignedSpotChanged?.Invoke(this, e);
        }

        public void UnbindEvents()
        {
            foreach (EqualSpotsItem esi in spEqualSpots.Children)
            {
                esi.UnbindEvents();
                esi.AssignedSpotChanged -= EqualSpotsItem_AssignedSpotChanged;
            }
        }
    }
}
