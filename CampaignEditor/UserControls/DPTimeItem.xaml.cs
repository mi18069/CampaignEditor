using CampaignEditor.Helpers;
using Database.DTOs.DPTimeDTO;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for DPTime.xaml
    /// </summary>
    public partial class DPTimeItem : UserControl
    {

        private DPTimeDTO dpTime = null; 
        public bool modified = false;

        public event EventHandler DPTimeItemDeleted;
        public DPTimeItem()
        {
            InitializeComponent();
        }

        public void Initialize(DPTimeDTO dpTime = null)
        {          
            this.dpTime = dpTime;
            tbTimeFrom.Text = dpTime.stime;
            tbTimeTo.Text = dpTime.etime;
            modified = false;           
        } 

        public Tuple<string, string> GetStringTimePair()
        {
            return Tuple.Create(tbTimeFrom.Text, tbTimeTo.Text);
        }

        public bool IsGoodTime()
        {
            if (tbTimeFrom.Text.Length <= 0 || tbTimeTo.Text.Length <= 0) 
            {
                return false;
            }
            // String comparison is enough, there is no need for conversions
            if (String.Compare(tbTimeFrom.Text, "02:00") < 0)
            {
                return false;
            }
            if (String.Compare(tbTimeTo.Text, "25:59") > 0)
            {
                return false;
            }
            if (String.Compare(tbTimeFrom.Text, tbTimeTo.Text) > 0)
            {
                return false;
            }

            return true;
        }

        private void tbTime_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                try
                {
                    string timeString = textBox.Text.Trim();
                    textBox.Text = TimeFormat.ReturnGoodTimeFormat(timeString);
                }
                catch
                {
                    MessageBox.Show("Invalid time format", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBox.Text = "";
                }
            }
            
        }

        private void tbTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            modified = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DPTimeItemDeleted?.Invoke(this, null);
            ((StackPanel)this.Parent).Children.Remove(this);
        }
    }
}
