using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class AMRTrim : Window
    {

        public bool changed = false;
        private bool changedText = false;
        public int newValue = 100;
        public AMRTrim()
        {
            InitializeComponent();
        }

        public void Initialize(string message, int? current = null)
        {
            lblMessage.Content = message;
            if (current != null)
            {
                tbAmr.Text = current.ToString();
            }
            tbAmr.Focus();
            tbAmr.SelectAll();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (changedText)
            {
                int tryValue;
                var newValueTb = Int32.TryParse(tbAmr.Text.ToString(), out tryValue);
                if (newValueTb)
                {
                    newValue = tryValue;
                    changed = true;
                }
            }
            this.Close();
        }

        private void tbAmr_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            changedText = true;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check if the Enter key is pressed
            if (e.Key == Key.Enter)
            {
                btnSave_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                btnCancel_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
