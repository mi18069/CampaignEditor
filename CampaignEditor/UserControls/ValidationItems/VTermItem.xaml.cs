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

        public void Initialize(TermTuple termTuple = null)
        {
            if (termTuple == null)
            {
                MakeItemEmpty();
                return;
            }

            tbSpotName.Text = $"{termTuple.Spot.spotcode} : {termTuple.Spot.spotname.Trim()} ({termTuple.Spot.spotlength})";
            tbProgName.Text = termTuple.MediaPlan.name.Trim();
            if (termTuple.MediaPlan.blocktime == null)
            {
                lblTime.Content = termTuple.MediaPlan.stime.ToString();
            }
            else
            {
                lblTime.Content = "Block time: " + termTuple.MediaPlan.blocktime.ToString();
            }
            lblAMR1.Content = "Amr%1: " + Math.Round(termTuple.MediaPlan.amrp1, 2).ToString();
            lblAMR2.Content = "Amr%2: " + Math.Round(termTuple.MediaPlan.amrp2, 2).ToString();
            lblAMR3.Content = "Amr%3: " + Math.Round(termTuple.MediaPlan.amrp3, 2).ToString();

            if (termTuple.Cpp != null)
            {
                lblCpp.Content = "CPP: " + termTuple.Cpp.ToString();
                lblAMRSale.Content = "Amr sale: " + termTuple.Amrpsale.ToString();
            }

            lblDPCoef.Content = "Dp coef: " + termTuple.MediaPlan.Dpcoef.ToString();
            lblProgCoef.Content = "Prog coef: " + termTuple.MediaPlan.Progcoef.ToString();
            lblSeccoef.Content = "Sec coef: " + termTuple.Seccoef.ToString();
            lblSeascoef.Content = "Seas coef: " + termTuple.Seascoef.ToString();

            lblPrice.Content = "Price: " + Math.Round(termTuple.Price.Value, 2).ToString();
        }

        private void MakeItemEmpty()
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
