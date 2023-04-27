using System;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    public partial class DateRangeItem : UserControl
    {

        DateTime today = DateTime.Now;
        public DateRangeItem()
        {
            InitializeComponent();
            dpFrom.SelectedDate = today;
            dpTo.SelectedDate = today;
        }

        // For checking validity of date order
        public bool CheckValidity()
        {
            if (dpFrom.SelectedDate != null && dpTo.SelectedDate != null && 
                dpFrom.SelectedDate <= dpTo.SelectedDate)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public bool checkIntercepting(DateRangeItem dri)
        {
            if (dpFrom.SelectedDate <= dri.dpFrom.SelectedDate && dpTo.SelectedDate >= dri.dpTo.SelectedDate ||
                dpFrom.SelectedDate >= dri.dpFrom.SelectedDate && dpFrom.SelectedDate <= dri.dpTo.SelectedDate ||
                dpTo.SelectedDate >= dri.dpFrom.SelectedDate && dpTo.SelectedDate <= dri.dpTo.SelectedDate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ListBox)this.Parent).Items.Remove(this);
        }
    }
}
