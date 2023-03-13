using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.GoalsDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class Goals : Window
    {

        private GoalsController _goalsController;

        private CampaignDTO _campaign = null;

        private GoalsDTO _goals = null;

        public GoalsDTO Goal
        {
            get { return _goals; }
            set { _goals = value; }
        }

        public bool goalsModified = false;
        public Goals(IGoalsRepository goalsRepository)
        {
            _goalsController = new GoalsController(goalsRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign, GoalsDTO goals = null)
        {
            _campaign = campaign;

            if (goals == null)
            {
                var maybeGoals = await _goalsController.GetGoalsByCmpid(_campaign.cmpid);
                Goal = maybeGoals;
                InitializeFields(maybeGoals);
            }
            else
            {
                InitializeFields(goals);
            }
        }

        private void InitializeFields(GoalsDTO goals = null)
        {
            if (goals == null)
            {
                tbBudget.Text = "0";
                tbGRP.Text = "0";
                tbInsertations.Text = "0";
                tbRCHFrom.Text = "1";
                tbRCHTo.Text = "9999";
                tbRCH.Text = "0";
            }
            else
            {
                tbBudget.Text = goals.budget.ToString().Trim();
                tbGRP.Text = goals.grp.ToString().Trim();
                tbInsertations.Text = goals.ins.ToString().Trim();
                tbRCHFrom.Text = goals.rch_f1.ToString().Trim() == "0" ? "1" :  goals.rch_f1.ToString().Trim();
                tbRCHTo.Text = goals.rch_f2.ToString().Trim() == "0" ? "9999" : goals.rch_f2.ToString().Trim();
                tbRCH.Text = goals.rch.ToString().Trim();
            }
            CheckCorrectFields(goals);
        }

        private void CheckCorrectFields(GoalsDTO goals = null)
        {
            if (goals == null)
            {
                cbBudget.IsChecked = false;
                cbGRP.IsChecked = false;
                cbInsertations.IsChecked = false;
                cbRCH.IsChecked = false;
            }
            else
            {
                if (goals.budget.ToString().Trim() == "0")
                    cbBudget.IsChecked = false;
                else
                    cbBudget.IsChecked = true;

                if (goals.grp.ToString().Trim() == "0")
                    cbGRP.IsChecked = false;
                else
                    cbGRP.IsChecked = true;

                if (goals.ins.ToString().Trim() == "0")
                    cbInsertations.IsChecked = false; 
                else
                    cbInsertations.IsChecked = true;

                if (goals.rch.ToString().Trim() == "0")
                    cbRCH.IsChecked = false;
                else
                    cbRCH.IsChecked = true;
            }
        }

        #region Save and Cancel
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool passCheckTest = false;
            if (goalsModified)
            {
                if (CheckValues())
                {
                    passCheckTest = true;
                    int budget = (bool)cbBudget.IsChecked ? int.Parse(tbBudget.Text) : 0;
                    int grp = (bool)cbGRP.IsChecked ? int.Parse(tbGRP.Text) : 0;
                    int insertations = (bool)cbInsertations.IsChecked ? int.Parse(tbInsertations.Text) : 0;
                    int rchFrom = (bool)cbRCH.IsChecked ? int.Parse(tbRCHFrom.Text) : 0;
                    int rchTo = (bool)cbRCH.IsChecked ? int.Parse(tbRCHTo.Text) : 0;
                    int rch = (bool)cbRCH.IsChecked ? int.Parse(tbRCH.Text) : 0;
                    Goal = new GoalsDTO(_campaign.cmpid, budget, grp, insertations, rchFrom, rchTo, rch);
                }
            }
            if (!goalsModified || goalsModified && passCheckTest)
                this.Hide();
        }

        private bool CheckValues()
        {
            int budget; 
            int grp; 
            int insertations; 
            int rchFrom; 
            int rchTo; 
            int rch; 

            if ((bool)cbBudget.IsChecked && !int.TryParse(tbBudget.Text, out budget))
            {
                MessageBox.Show("Invalid value for budget");
                return false;
            }
            if ((bool)cbGRP.IsChecked && !int.TryParse(tbGRP.Text, out grp))
            {
                MessageBox.Show("Invalid value for GRP");
                return false;
            }
            if ((bool)cbInsertations.IsChecked && !int.TryParse(tbInsertations.Text, out insertations))
            {
                MessageBox.Show("Invalid value for Insertations");
                return false;
            }
            if ((bool)cbRCH.IsChecked && !int.TryParse(tbRCHFrom.Text, out rchFrom))
            {
                MessageBox.Show("Invalid value for Reach interval");
                return false;
            }
            if ((bool)cbRCH.IsChecked && !int.TryParse(tbRCHTo.Text, out rchTo))
            {
                MessageBox.Show("Invalid value for Reach interval");
                return false;
            }
            if ((bool)cbRCH.IsChecked && !int.TryParse(tbRCH.Text, out rch))
            {
                MessageBox.Show("Invalid value for Reach interval");
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            goalsModified = false;
            this.Hide();
        }
        #endregion

        public async Task UpdateDatabase(GoalsDTO goals)
        {
            await _goalsController.DeleteGoalsByCmpid(goals.cmpid);
            await _goalsController.CreateGoals(new CreateGoalsDTO(goals.cmpid, goals.budget,
                goals.grp, goals.ins, goals.rch_f1, goals.rch_f2, goals.rch));
        }

        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        // For allowing only numbers to be entered in textboxes
        Regex onlyNumbersRegex = new Regex("[^0-9]+");
        private void tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = onlyNumbersRegex.IsMatch(e.Text);
            if(!e.Handled)
                goalsModified = true;
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            goalsModified = true;
        }

        private void cb_Unchecked(object sender, RoutedEventArgs e)
        {
            goalsModified = true;
        }
    }
}
