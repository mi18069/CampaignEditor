using System;
using System.Collections.Generic;
using System.Windows;

namespace CampaignEditor.UserControls
{
    public class DRIListBoxAddButton : ListBoxAddButton
    {
        private List<DateTime> _disabledDates;

        public DRIListBoxAddButton()
            : base()
        {
        }

        public void Initialize(List<DateTime> disabledDates, object objToAdd)
        {
            base.Initialize(objToAdd);
            _disabledDates = disabledDates;
        }

        override protected void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            DateRangeItem dri = new DateRangeItem();          
            dri.DisableDates(_disabledDates);
            this.Items.Insert(this.Items.Count - 1, dri);
            ResizeItems(this.Items);
        }
    }
}
