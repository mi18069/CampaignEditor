using Database.Entities;
using System;
using System.Windows.Controls;
namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Items which will be stacked in verification stack pannels
    /// </summary>
    public partial class VTermItem : UserControl
    {
        public VTermItem()
        {
            InitializeComponent();
        }

        public void Initialize(TermTuple termTuple)
        {
            tbName.Text = termTuple.Spot.spotname.Trim();
            if (termTuple.MediaPlan.blocktime == null)
            {
                lblTime.Content = termTuple.MediaPlan.stime.ToString();
            }
            else
            {
                lblTime.Content = "Block time: " + termTuple.MediaPlan.blocktime.ToString();
            }
            if (termTuple.MediaPlan.DayPart == null)
            {
                lblDayPart.Content = "Day part: - ";
            }
            else
            {
                lblDayPart.Content = "Day part: " + termTuple.MediaPlan.DayPart.name.ToString();
            }
            lblPosition.Content = "Pos: " + termTuple.MediaPlan.position.ToString();
            lblAMR1.Content = "Amr%1: " + Math.Round(termTuple.MediaPlan.amrp1, 2).ToString();
            lblAMR2.Content = "Amr%2: " + Math.Round(termTuple.MediaPlan.amrp2, 2).ToString();
            lblAMR3.Content = "Amr%3: " + Math.Round(termTuple.MediaPlan.amrp3, 2).ToString();
            lblPrice.Content = "Price: " + Math.Round(termTuple.Price, 2).ToString();
        }
    }
}
