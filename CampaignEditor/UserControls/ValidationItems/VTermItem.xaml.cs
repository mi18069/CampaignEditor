using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
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
                lblTime.Content = termTuple.MediaPlan.blocktime.ToString();
            }
            
        }
    }
}
