using CampaignEditor.Helpers;
using Database.DTOs.RealizedSpotDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ValidationItems.PairSpotsItems
{
    /// <summary>
    /// Interaction logic for EqualSpots.xaml
    /// </summary>
    public partial class EqualSpotsItem : UserControl
    {
        EqualSpots _equalSpots;
        List<string> _spotcodesList = new List<string>();
        public event EventHandler<RealizedSpotPairChangedEventArgs> AssignedSpotChanged;

        public EqualSpotsItem(EqualSpots equalSpots, IEnumerable<string> spotcodes)
        {
            InitializeComponent();
            _equalSpots = equalSpots;
            _spotcodesList.AddRange(spotcodes);
            SetControl();
        }

        private void SetControl()
        {
            spExpectedSpots.Children.Clear();
            spRealizedSpots.Children.Clear();

            ExpectedSpotItem expectedItem = new ExpectedSpotItem(_equalSpots.ExpectedSpot);
            spExpectedSpots.Children.Add(expectedItem);

            foreach (var realizedSpot in _equalSpots.RealizedSpots)
            {
                RealizedSpotItem realizedItem = new RealizedSpotItem(realizedSpot, _spotcodesList);
                realizedItem.AssignedSpotChanged += RealizedItem_AssignedSpotChanged;
                spRealizedSpots.Children.Add(realizedItem);
            }
        }

        private void RealizedItem_AssignedSpotChanged(object? sender, RealizedSpotPairChangedEventArgs e)
        {
            AssignedSpotChanged?.Invoke(this, e);
        }

        public void DeleteRealized(RealizedSpotDTO realizedSpotDTO)
        {
            // Delete from list
            /*var realizedSpot = _realizedSpots.First(rs => rs.spotnum == realizedSpotDTO.spotnum);
            var indexToDelete = _realizedSpots.IndexOf(realizedSpot);
            if (indexToDelete != -1)
            {
                _realizedSpots.RemoveAt(indexToDelete);
            }*/

            // Delete from stack pannel
            var realizedSpotItem = spRealizedSpots.Children
                .Cast<RealizedSpotItem>()
                .First(rsi => rsi.GetSpotnum() == realizedSpotDTO.spotnum);
            realizedSpotItem.AssignedSpotChanged -= RealizedItem_AssignedSpotChanged;
            spRealizedSpots.Children.Remove(realizedSpotItem);
        }

        public void AddRealized(RealizedSpotDTO realizedSpotDTO)
        {
            // Add to list
            /*_realizedSpots.Add(realizedSpotDTO);
            _realizedSpots.OrderBy(rs => rs.spotnum);
            var index = _realizedSpots.IndexOf(realizedSpotDTO);*/

            // Index corresponds to index where new realizedSpotItem should be added
            // Add to stack pannel
            RealizedSpotItem realizedItem = new RealizedSpotItem(realizedSpotDTO, _spotcodesList);
            realizedItem.AssignedSpotChanged += RealizedItem_AssignedSpotChanged;
            spRealizedSpots.Children.Add(realizedItem);

        }

        public void UnbindEvents()
        {
            foreach (RealizedSpotItem realizedSpotItem in spRealizedSpots.Children)
            {
                realizedSpotItem.AssignedSpotChanged -= RealizedItem_AssignedSpotChanged;
            }
        }

        public string GetExpectedSpotcode()
        {
            return _equalSpots.ExpectedSpot.spotcode;
        }
        
    }
}
