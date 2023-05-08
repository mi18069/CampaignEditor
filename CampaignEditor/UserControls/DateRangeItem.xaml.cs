using System;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    public partial class DateRangeItem : UserControl
    {
       
        public DateRangeItem()
        {
            InitializeComponent();
            SetDates();
        }

        public void SetDates(DateTime? first = null, DateTime? second = null)
        {
            if (first == null)
            {
                DateTime today = DateTime.Now;
                dpFrom.SelectedDate = today;
                dpTo.SelectedDate = today;
            }
            else if (first != null && second == null)
            {
                dpFrom.SelectedDate = first.Value;
                dpTo.SelectedDate = first.Value;
            }
            else
            {
                dpFrom.SelectedDate = first.Value;
                dpTo.SelectedDate = second.Value;
            }

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
