using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    public partial class SpotItem : UserControl
    {
        public bool modified = false;
        public SpotItem()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            modified = true;
        }

    }
}
