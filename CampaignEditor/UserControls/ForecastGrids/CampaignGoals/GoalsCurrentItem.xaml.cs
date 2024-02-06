using Database.Entities;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// Shows goals for currently selected MediaPlanTuple
    /// </summary>
    public partial class GoalsCurrentItem : UserControl
    {
        private SelectedMPGoals _selectedMediaPlan = new SelectedMPGoals();

        public SelectedMPGoals SelectedMediaPlan
        {
            get { return _selectedMediaPlan; }
            set { _selectedMediaPlan = value; }
        }

        public GoalsCurrentItem()
        {
            InitializeComponent();
        }

        public void FillGoals(MediaPlanTuple currentTuple = null)
        {
            if (currentTuple != null)
            {
                SelectedMediaPlan.MediaPlan = currentTuple.MediaPlan;
            }    
            
            lblBudgetValue.DataContext = SelectedMediaPlan;
            lblGRPValue.DataContext = SelectedMediaPlan;
            lblInsertationsValue.DataContext = SelectedMediaPlan;                          
        }
    }
}
