using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Design for Ruler in RulerTimeline
    /// </summary>
    public partial class Ruler : UserControl
    {
        public Ruler()
        {
            InitializeComponent();
        }

        // For passed "hh:mm" values create intervals
        public void Initialize(int height)
        {

            // From 02:00 to 25:59 in minutes
            for (int i=2*60; i<25*60+59; i++)
            {
                RulerLine rl = new RulerLine(i, height);
                spRuler.Children.Add(rl);               
            } 
        }
    }
}
