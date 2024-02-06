﻿using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.GoalsDTO;
using Database.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// This is userControl that shows current total goals
    /// </summary>
    public partial class GoalsTotalItem : UserControl
    {
        private MPGoals mpGoals = new MPGoals();

        public GoalsTotalItem()
        {
            InitializeComponent();
        }

        public void InitializeFields(GoalsDTO goals)
        {
            if (goals.budget == 0)
            {
                lblBudget.FontWeight = FontWeights.Light;
                lblBudgetTarget.FontWeight = FontWeights.Light;
                lblBudgetValue.FontWeight = FontWeights.Light;
            }
            else
            {
                lblBudget.FontWeight = FontWeights.Bold;
                lblBudgetTarget.FontWeight = FontWeights.Bold;
                lblBudgetValue.FontWeight = FontWeights.Bold;
            }
            lblBudgetTarget.Content = "/" + (goals.budget != 0 ? goals.budget.ToString() : " - ");

            if (goals.grp == 0)
            {
                lblGRP.FontWeight = FontWeights.Light;
                lblGRPTarget.FontWeight = FontWeights.Light;
                lblGRPValue.FontWeight = FontWeights.Light;
            }
            else
            {
                lblGRP.FontWeight = FontWeights.Bold;
                lblGRPTarget.FontWeight = FontWeights.Bold;
                lblGRPValue.FontWeight = FontWeights.Bold;
            }
            lblGRPTarget.Content = "/" + (goals.grp != 0 ? goals.grp.ToString() : " - ");

            if (goals.ins == 0)
            {
                lblInsertations.FontWeight = FontWeights.Light;
                lblInsertationsTarget.FontWeight = FontWeights.Light;
                lblInsertationsValue.FontWeight = FontWeights.Light;
            }
            else
            {
                lblInsertations.FontWeight = FontWeights.Bold;
                lblInsertationsTarget.FontWeight = FontWeights.Bold;
                lblInsertationsValue.FontWeight = FontWeights.Bold;
            }
            lblInsertationsTarget.Content = "/" + (goals.ins != 0 ? goals.ins.ToString() : " - ");

            if (goals.rch == 0)
            {
                lblReach.FontWeight = FontWeights.Light;
                lblReachTarget.FontWeight = FontWeights.Light;
                lblReachValue.FontWeight = FontWeights.Light;
            }
            else
            {
                lblReach.FontWeight = FontWeights.Bold;
                lblReachTarget.FontWeight = FontWeights.Bold;
                lblReachValue.FontWeight = FontWeights.Bold;
            }
            lblReachTarget.Content = "/" + (goals.ins != 0 ? goals.rch.ToString() + "%" : " - ");
        }

        public void FillGoals(IEnumerable<MediaPlanTuple> allMediaPlans)
        {
            mpGoals.MediaPlans = new ObservableCollection<MediaPlan>(allMediaPlans.Select(mp => mp.MediaPlan));               
            lblBudgetValue.DataContext = mpGoals;           
            lblGRPValue.DataContext = mpGoals;              
            lblInsertationsValue.DataContext = mpGoals;              
            lblReachValue.DataContext = mpGoals;           
        }
    }
}
