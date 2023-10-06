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

namespace CampaignEditor
{
    /// <summary>
    /// Page in which we'll graphically see times of reserved and realized ads
    /// </summary>
    public partial class CampaignValidation : Page
    {
        public CampaignValidation()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            rulerExpected.Initialize();
        }
    }
}
