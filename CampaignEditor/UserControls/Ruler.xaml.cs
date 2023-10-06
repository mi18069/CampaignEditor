using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
