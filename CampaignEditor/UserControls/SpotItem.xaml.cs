using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        // Disable paste mechanism
        private void tbPreviewExecuted_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void tbLength_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(Convert.ToChar(e.Text)))
                e.Handled = true;
        }
    }
}
