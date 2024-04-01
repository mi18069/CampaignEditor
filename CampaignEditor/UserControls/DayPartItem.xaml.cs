using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{

    public partial class DayPartItem : UserControl
    {

        private DayPartDTO _dayPart = null;
        private List<DPTimeDTO> _dpTimes = new List<DPTimeDTO>();
        private List<DPTimeDTO> _dpTimesToDelete = new List<DPTimeDTO>();

        public bool modified = false;

        public event EventHandler DayPartItemDeleted;

        public DayPartItem()
        {
            InitializeComponent();
        }

        public void Initialize(DayPartDTO dayPart, List<DPTimeDTO> dpTimes)
        {
            _dayPart = dayPart; 
            _dpTimes = dpTimes;
            tbName.Text = _dayPart.name.Trim();

            FillDPTimes();
        }

        private void FillDPTimes()
        {
            foreach (var dpTime in _dpTimes)
            {
                var dpTimeItem = MakeDPTimeItem(dpTime);
                spDPTimes.Children.Add(dpTimeItem);
            }
        }
        private DPTimeItem MakeDPTimeItem(DPTimeDTO dpTime = null) 
        {
            DPTimeItem dpTimeItem = new DPTimeItem();
            if (dpTime != null)
            {
                dpTimeItem.Initialize(dpTime);
            }
            dpTimeItem.DPTimeItemDeleted += DpTimeItem_DPTimeItemDeleted;
            return dpTimeItem;
        }

        private void DpTimeItem_DPTimeItemDeleted(object? sender, System.EventArgs e)
        {
            DPTimeItem dpTimeItem= (DPTimeItem)sender;
            dpTimeItem.DPTimeItemDeleted -= DpTimeItem_DPTimeItemDeleted;
            modified = true;
        }

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dpTimeItem = MakeDPTimeItem();
            spDPTimes.Children.Add(dpTimeItem);
            modified = true;
        }

        private void btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DayPartItemDeleted?.Invoke(this, null);
            ((ListBoxAddButton)this.Parent).Items.Remove(this);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (DPTimeItem dpTimeItem in spDPTimes.Children)
            {
                dpTimeItem.DPTimeItemDeleted -= DpTimeItem_DPTimeItemDeleted;
            }
        }

        public string GetName()
        {
            return tbName.Text.Trim();
        }

        public bool CheckModified()
        {
            foreach (DPTimeItem dpTimeItem in spDPTimes.Children)
            {
                if (dpTimeItem.modified)
                {
                    return true;
                }
            }

            return modified;
        }

        public bool isGoodTime()
        {
            if (tbName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter a name for time period", "Message", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            foreach (DPTimeItem dpTimeItem in spDPTimes.Children)
            {
                if (!dpTimeItem.IsGoodTime())
                {
                    MessageBox.Show("Invalid time values in \n" + tbName.Text, "Message",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        public List<Tuple<string, string>> GetTimeStringPairs()
        {
            List<Tuple<string, string>> timeStringPairs = new List<Tuple<string, string>>();

            foreach (DPTimeItem dpTimeItem in spDPTimes.Children)
            {
                timeStringPairs.Add(dpTimeItem.GetStringTimePair());
            }

            return timeStringPairs;
        }

        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            modified = true;
        }
    }
}
