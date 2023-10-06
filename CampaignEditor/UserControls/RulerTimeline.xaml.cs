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
    /// Interaction logic for RulerTimeline.xaml
    /// </summary>
    public partial class RulerTimeline : UserControl
    {
        int lineHeight = 20;
        public RulerTimeline()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            ruler.Initialize(lineHeight);
            canvas.Initialize(lineHeight);
        }
    }
}
