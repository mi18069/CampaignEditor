using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.GoalsDTO;
using Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
    }
}
