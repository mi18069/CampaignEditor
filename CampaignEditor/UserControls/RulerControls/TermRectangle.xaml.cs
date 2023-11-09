using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Rectangle for representing MediaPlanTerm in CampaignVerification page
    /// </summary>
    public partial class TermRectangle : UserControl
    {
        public TermRectangle(int span, Brush color)
        {
            InitializeComponent();
            SetHeight(span); // for how long it will last 
            SetBorderColor(color); // different color for different spot
        }

        private void SetHeight(int span)
        {
            this.Height = span;
        }

        private void SetBorderColor(Brush color)
        {
            border.Background = color;
        }
    }
}
