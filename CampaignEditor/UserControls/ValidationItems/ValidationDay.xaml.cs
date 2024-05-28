using Database.Entities;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using CampaignEditor.Controllers;
using System.Linq;

namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Interaction logic for ValidationDay.xaml
    /// </summary>
    public partial class ValidationDay : UserControl
    {
        public List<TermTuple?> _termTuples;
        public List<MediaPlanRealized?> _mpRealizedTuples;
        private DateOnly date;

        public MediaPlanRealizedController _mediaPlanRealizedController;

        public ValidationDay(DateOnly date,
            List<TermTuple> termTuples,
            List<MediaPlanRealized> mpRealizedTuples)
        {
            InitializeComponent();

            dgExpected.ItemsSource = null;
            dgRealized.ItemsSource = null;

            _termTuples = termTuples;
            _mpRealizedTuples = mpRealizedTuples;

            this.date = date;
            if (_termTuples.Count > 0)
                dgExpected.ItemsSource = _termTuples;
            if (_mpRealizedTuples.Count > 0)
                dgRealized.ItemsSource = _mpRealizedTuples;
            SetUserControl();
        }
      

        private void SetUserControl()
        {
            int exCount = _termTuples.Where(tt => tt != null && tt.Status != -1).Count();
            int realCount = _mpRealizedTuples.Where(rt => rt != null && rt.status != -1).Count();

            lblExCount.Content = exCount;
            lblRealCount.Content = realCount;
            lblDate.Content = date.ToShortDateString();

            if (exCount == 0 && realCount == 0)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            /*foreach (var termTuple in _termTuples)
            {
                VTermItem vTermItem = new VTermItem();
                vTermItem.Initialize(termTuple);
                spExpected.Children.Add(vTermItem);
            }

            foreach (var mpRealized in _mpRealizedTuples)
            {
                VRealizedItem vRealizedItem = new VRealizedItem();
                vRealizedItem._mpRController = _mediaPlanRealizedController;
                vRealizedItem.Initialize(mpRealized);

                spRealized.Children.Add(vRealizedItem);
            }*/

            /*dgExpected.ItemsSource = _termTuples;
            dgRealized.ItemsSource = _mpRealizedTuples;*/

            /*while (exCount < realCount)
            {
                VTermEmptyItem vTermEmptyItem = new VTermEmptyItem();
                dgExpected.Items.Add(vTermEmptyItem);
                exCount += 1;
            }

            while (exCount > realCount)
            {
                VTermEmptyItem vTermEmptyItem = new VTermEmptyItem();
                spRealized.Children.Add(vTermEmptyItem);
                realCount += 1;
            }*/
        }

    }
}
