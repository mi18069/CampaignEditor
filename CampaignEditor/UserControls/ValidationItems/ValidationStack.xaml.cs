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
        public CompletedValidationController _completedValidationController;

        public MediaPlanController _mediaPlanController;
        public MediaPlanTermController _mediaPlanTermController;
        public MediaPlanRealizedController _mediaPlanRealizedController;
        public SpotController _spotController;
        public MediaPlanConverter _mpConverter;
        public MediaPlanForecastData _forecastData;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        public ObservableRangeCollection<MediaPlanRealized> _mediaPlanRealized;

        private bool hideExpected = false;

        //22 columns
        private string dgExpectedMask = "1110111001111111000111";
        private string dgRealizedMask = "1111111100111111100111";

        /*private bool[] dgExpectedMask = new bool[22]
        { true, true, true, false, true, true, true, false, false, true, true, true,
            true, true, true, true, false, false, false, true, true, true};

        private bool[] dgRealizedMask = new bool[22]
        { true, true, true, true, true, true, true, true, false, false, true,
            true, true, true, true, true, true, false, false, true, true, true};*/

        public event EventHandler<UpdateMediaPlanRealizedEventArgs> UpdatedMediaPlanRealized;
        public string DgExpectedMask { get { return dgExpectedMask; } }
        public string DgRealizedMask { get { return dgRealizedMask; } }
        public UIElementCollection ValidationDays{ get {return spValidationDays.Children; } }

        public Dictionary<DateOnly, ValidationDay> ValidationDaysDict = new Dictionary<DateOnly, ValidationDay>();
        public ValidationStack()
        {
            InitializeComponent();

        }

        public async Task Initialize(CampaignDTO campaign, bool hideExpected = false)
        {
            _campaign = campaign;
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(campaign.cmpid);
            if (mpVersion != null)
            {
                int version = mpVersion.version;
                _campaignVersion = version;

            }

            await LoadDates();
            // When reinitializing
            if (hideExpected)
            {
                this.hideExpected = hideExpected;
                HideExpectedStack();
            }
        }

        public async Task LoadData(int chid)
        {

            /*await LoadExpected(chid);
            await LoadRealized(chid);
            LoadDays();*/
        }

        private async Task LoadDates()
        {
            ClearStackPanel();

            ValidationDaysDict.Clear();
            foreach (var day in _dates)
            {
                var dateExpected = _dateExpectedDict[day];
                var dateRealized = _dateRealizedDict[day];
                bool isCompleted = false;
                var compValidation = await _completedValidationController.GetCompValidation(_campaign.cmpid, TimeFormat.DateOnlyToYMDString(day));
                if (compValidation == null)
                {
                    await _completedValidationController.CreateCompValidation(new CompletedValidation(_campaign.cmpid, TimeFormat.DateOnlyToYMDString(day), false));
                    isCompleted = false;
                }
                else
                {
                    isCompleted = compValidation.completed;
                }
                ValidationDay validationDay = new ValidationDay(day, dateExpected, dateRealized, dgExpectedMask, dgRealizedMask, isCompleted);
                validationDay.InvertedExpectedColumnVisibility += ValidationDay_InvertedExpectedColumnVisibility;
                validationDay.InvertedRealizedColumnVisibility += ValidationDay_InvertedRealizedColumnVisibility;
                validationDay.UpdatedMediaPlanRealized += ValidationDay_UpdatedMediaPlanRealized;
                validationDay.CompletedValidationChanged += ValidationDay_CompletedValidationChanged;
                spValidationDays.Children.Add(validationDay);

                ValidationDaysDict[day] = validationDay;
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
        private void ValidationDay_UpdatedMediaPlanRealized(object? sender, UpdateMediaPlanRealizedEventArgs e)
        {
            UpdatedMediaPlanRealized?.Invoke(this, e);
        }

        private async void ValidationDay_CompletedValidationChanged(object? sender, CompletedValidationEventArgs e)
        {
            await _completedValidationController.UpdateCompValidation(new CompletedValidation(_campaign.cmpid, e.Date, e.IsCompleted));
        }

        private void InvertExpectedGridHeader(int index)
        {
            InvertBit(dgRealizedMask, index);

            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertExpectedColumnVisibility(index);
            }
        }

        private void InvertRealizedGridHeader(int index)
        {

            InvertBit(dgRealizedMask, index);

            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertRealizedColumnVisibility(index);
            }
        }

        public string InvertBit(string bitString, int position)
        {
            if (position < 0 || position >= bitString.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of range.");
            }

            char[] bitArray = bitString.ToCharArray();

            bitArray[position] = bitArray[position] == '0' ? '1' : '0';

            return new string(bitArray);
        }

        public void ClearStackPanel()
        {
            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertedExpectedColumnVisibility -= ValidationDay_InvertedExpectedColumnVisibility;
                validationDay.InvertedRealizedColumnVisibility -= ValidationDay_InvertedRealizedColumnVisibility;
                validationDay.UpdatedMediaPlanRealized -= ValidationDay_UpdatedMediaPlanRealized;
            }
            spValidationDays.Children.Clear();

        }

        private void HideExpectedStack()
        {
            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.HideExpected();
                
            }
        }

        public void SelectedChannelsChanged(IEnumerable<ChannelDTO> selectedChannels)
        {
            var chids = new List<int>();
            var chrdsids = new List<int>();

            chids = selectedChannels.Select(ch => ch.chid).ToList();
            chrdsids = selectedChannels.Select(ch => _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == ch.chid).Key).ToList();

            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.ChannelsChanged(chids, chrdsids);
            }
        }

        


    }
}

