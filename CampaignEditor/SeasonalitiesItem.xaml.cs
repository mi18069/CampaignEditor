using System;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class SeasonalitiesItem : UserControl
    {
        public bool changed = false;
        public SeasonalitiesItem()
        {
            InitializeComponent();
        }
        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        public string? FromDPToString(DatePicker dp)
        {
            if (dp.SelectedDate != null)
            {
                string format = dp.SelectedDate.Value.ToString("yyyyMMdd");
                return format;
            }
            else
                return null;
        }

        public DateTime? FromStringToDateTime(string format)
        {
            int year = 0;
            int month = 0;
            int day = 0;

            if (int.TryParse(format.Substring(0, 4), out year) &&
                int.TryParse(format.Substring(4, 2), out month) &&
                int.TryParse(format.Substring(6, 2), out day))
            {
                return new DateTime(year, month, day);
            }
            else
                return null;
            
        }

    }
}
