using Database.Entities;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using CampaignEditor.Controllers;

namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Interaction logic for ValidationDay.xaml
    /// </summary>
    public partial class ValidationDay : UserControl
    {
        private List<TermTuple> _termTuples = new List<TermTuple>();
        private List<MediaPlanRealized> _mpRealizedTuples = new List<MediaPlanRealized>();
        private DateOnly date;

        public MediaPlanRealizedController _mediaPlanRealizedController;

        public ValidationDay(DateOnly date,
            List<TermTuple> termTuples,
            List<MediaPlanRealized> mpRealizedTuples)
        {
            InitializeComponent();

            spExpected.Children.Clear();
            spRealized.Children.Clear();

            _termTuples.Clear();
            _mpRealizedTuples.Clear();

            this.date = date;
            _termTuples = termTuples;
            _mpRealizedTuples = mpRealizedTuples;

        }

        public void Initialize(DateOnly date,
            List<TermTuple> termTuples,
            List<MediaPlanRealized> mpRealizedTuples)
        {
            spExpected.Children.Clear();
            spRealized.Children.Clear();

            _termTuples.Clear();
            _mpRealizedTuples.Clear();

            this.date = date;
            _termTuples = termTuples;
            _mpRealizedTuples = mpRealizedTuples;
        }

        public void SetUserControl()
        {
            int exCount = _termTuples.Count;
            int realCount = _mpRealizedTuples.Count;

            lblExCount.Content = exCount;
            lblRealCount.Content = realCount;
            lblDate.Content = date.ToShortDateString();

            if (exCount == 0 && realCount == 0)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (var termTuple in _termTuples)
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
            }

            while (exCount < realCount)
            {
                VTermEmptyItem vTermEmptyItem = new VTermEmptyItem();
                spExpected.Children.Add(vTermEmptyItem);
                exCount += 1;
            }

            while (exCount > realCount)
            {
                VTermEmptyItem vTermEmptyItem = new VTermEmptyItem();
                spRealized.Children.Add(vTermEmptyItem);
                realCount += 1;
            }
        }

    }
}
