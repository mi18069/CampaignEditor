using System;
using System.Windows;

namespace CampaignEditor
{
    public partial class AMRTrim : Window
    {

        public bool changed = false;
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
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            changed = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (changed)
            {
                int tryValue;
                var newValueTb = Int32.TryParse(tbAmr.Text.ToString(), out tryValue);
                if (newValueTb)
                {
                    newValue = tryValue;
                }
            }
            this.Close();
        }

        private void tbAmr_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            changed = true;
        }
    }
}
