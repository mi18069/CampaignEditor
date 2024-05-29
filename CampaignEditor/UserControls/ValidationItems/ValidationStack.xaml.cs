using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Two stacks within the same ScrollViewer
    /// </summary>
    public partial class ValidationStack : UserControl
    {

        private CampaignDTO _campaign;
        private int _campaignVersion;

        public List<ChannelDTO> _channels = new List<ChannelDTO>();
        public List<DateOnly> _dates = new List<DateOnly>();

        public Dictionary<DateOnly, List<TermTuple?>> _dateExpectedDict;
        public Dictionary<DateOnly, List<MediaPlanRealized?>> _dateRealizedDict;

        public ChannelCmpController _channelCmpController;
        public ChannelController _channelController;
        public MediaPlanVersionController _mediaPlanVersionController;

        public MediaPlanController _mediaPlanController;
        public MediaPlanTermController _mediaPlanTermController;
        public MediaPlanRealizedController _mediaPlanRealizedController;
        public SpotController _spotController;
        public MediaPlanConverter _mpConverter;
        public MediaPlanForecastData _forecastData;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        public ObservableRangeCollection<MediaPlanRealized> _mediaPlanRealized;

        private bool[] dgExpectedMask = new bool[18]
        { true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true};

        private bool[] dgRealizedMask = new bool[16]
        { true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true};

        public ValidationStack()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(campaign.cmpid);
            if (mpVersion != null)
            {
                int version = mpVersion.version;
                _campaignVersion = version;

            }
            LoadDates();
        }

        public async Task LoadData(int chid)
        {

            /*await LoadExpected(chid);
            await LoadRealized(chid);
            LoadDays();*/
        }

        private void LoadDates()
        {
            ClearStackPanel();

            foreach (var day in _dates)
            {
                var dateExpected = _dateExpectedDict[day];
                var dateRealized = _dateRealizedDict[day];

                ValidationDay validationDay = new ValidationDay(day, dateExpected, dateRealized, dgExpectedMask, dgRealizedMask);
                validationDay.InvertedExpectedColumnVisibility += ValidationDay_InvertedExpectedColumnVisibility;
                validationDay.InvertedRealizedColumnVisibility += ValidationDay_InvertedRealizedColumnVisibility;               

                spValidationDays.Children.Add(validationDay);
            }
        }



        private void ValidationDay_InvertedExpectedColumnVisibility(object? sender, IndexEventArgs e)
        {
            InvertExpectedGridHeader(e.Index);
        }
        private void ValidationDay_InvertedRealizedColumnVisibility(object? sender, IndexEventArgs e)
        {
            InvertRealizedGridHeader(e.Index);
        }

        private void InvertExpectedGridHeader(int index)
        {
            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertExpectedColumnVisibility(index);
            }
        }

        private void InvertRealizedGridHeader(int index)
        {
            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertRealizedColumnVisibility(index);
            }
        }

        public void ClearStackPanel()
        {
            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertedExpectedColumnVisibility -= ValidationDay_InvertedExpectedColumnVisibility;
                validationDay.InvertedRealizedColumnVisibility -= ValidationDay_InvertedRealizedColumnVisibility;
            }
            spValidationDays.Children.Clear();

        }

        private void svGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = svGrid;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta*0.4);
                e.Handled = true;
            }
        }
    }
}
