using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.ReachDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// Interaction logic for GoalsTreeView.xaml
    /// </summary>
    public partial class GoalsTreeView : UserControl
    {

        CampaignDTO _campaign;

        public GoalsController _goalsController;

        private MPGoals mpGoals = new MPGoals();
        private ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        private GoalsDTO _goals;

        public GoalsTreeView()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            _goals = await _goalsController.GetGoalsByCmpid(_campaign.cmpid);
            totalItem.InitializeFields(_goals);
        }

        public void FillGoals(IEnumerable<MediaPlanTuple> allMediaPlans)
        {
            _allMediaPlans = (ObservableRangeCollection<MediaPlanTuple>)allMediaPlans;

            totalItem.FillGoals(_allMediaPlans);
            selectedItem.FillGoals();
        }

        public void UpdateTotalReach(ReachDTO reach)
        {
            int lowerBound = _goals.rch_f1;
            int upperBound = _goals.rch_f2;

            double reachValue = ReturnSubtractedReach(lowerBound, upperBound, reach);
            totalItem.UpdateReach(reachValue);
        }

        public void SelectedTupleChanged(MediaPlanTuple newTuple)
        {
            selectedItem.FillGoals(newTuple);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnTotal)
            {
                totalItem.Visibility = btnTotal.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (sender == btnSelected)
            {
                selectedItem.Visibility = btnSelected.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private double ReturnSubtractedReach(int lowerBound, int upperBound, ReachDTO reach)
        {
            double minuend = ReturnReachBoundValue(lowerBound, reach);
            double subtrahend = ReturnReachBoundValue(upperBound, reach);
          
            if (minuend == subtrahend)
            {
                return minuend;
            }
            double difference = minuend - subtrahend;
            return Math.Max(0, difference);
        }

        private double ReturnReachBoundValue(int bound, ReachDTO reach)
        {
            decimal value = 0;
            switch (bound)
            {
                case 0:
                case 1:
                    value = reach.rch1;
                    break;
                case 2:
                    value = reach.rch12;
                    break;
                case 3:
                    value = reach.rch13;
                    break;
                case 4:
                    value = reach.rch14;
                    break;
                case 5:
                    value = reach.rch15;
                    break;
                case 6:
                    value = reach.rch16;
                    break;
                case 7:
                    value = reach.rch17;
                    break;
                case 8:
                    value = reach.rch18;
                    break;
                case 9:
                    value = reach.rch19 == null ? 0 : reach.rch19.Value;
                    break;
                default:
                    return 0;
            }

            return Decimal.ToDouble(value);
        }
    }
}
