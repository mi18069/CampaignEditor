using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        public DGConfigController _dgConfigController;

        public MediaPlanController _mediaPlanController;
        public MediaPlanTermController _mediaPlanTermController;
        public MediaPlanRealizedController _mediaPlanRealizedController;
        public SpotController _spotController;
        public MediaPlanConverter _mpConverter;
        public MediaPlanForecastData _forecastData;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        public ObservableRangeCollection<MediaPlanRealized> _mediaPlanRealized;

        private bool hideExpected = false;
        private int numGridsOpen = 0;
        //22 columns
        private string dgExpectedMask = "1110111001111111000111";
        private string dgRealizedMask = "1111111100111111100111";
        private DGConfig dgConfig;


        public event EventHandler<UpdateMediaPlanRealizedEventArgs> UpdatedMediaPlanRealized;
        public event EventHandler<CheckDateEventArgs> CheckNewDataDay;
        public event EventHandler<UpdateMediaPlanRealizedEventArgs> ProgcoefChangedMediaPlanRealized;
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

            await SetColumnsVisibility();

            await LoadDates();
            // When reinitializing
            if (hideExpected)
            {
                this.hideExpected = hideExpected;
                HideExpectedStack();
            }
        }

        private async Task SetColumnsVisibility()
        {
            var dgConf = await _dgConfigController.GetDGConfig(MainWindow.user.usrid, _campaign.clid);

            if (dgConf == null)
            {
                try
                {
                    await _dgConfigController.CreateDGConfig(new DGConfig(MainWindow.user.usrid, _campaign.clid,
                        null, dgExpectedMask, dgRealizedMask));
                    dgConfig = await _dgConfigController.GetDGConfig(MainWindow.user.usrid, _campaign.clid);
                }
                catch
                {
                    MessageBox.Show("Error while creating dgConfig", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                dgConfig = dgConf;
            }
            
            if (dgConfig.dgexp == null)
            {
                dgConfig.dgexp = dgExpectedMask;
                await _dgConfigController.UpdateDGConfigExp(dgConfig.usrid, dgConfig.clid, dgConfig.dgexp);
            }
            if (dgConfig.dgreal == null)
            {
                dgConfig.dgreal = dgRealizedMask;
                await _dgConfigController.UpdateDGConfigReal(dgConfig.usrid, dgConfig.clid, dgConfig.dgreal);
            }

        }
       

        private async Task LoadDates()
        {
            ClearStackPanel();

            ValidationDaysDict.Clear();
            /*foreach (var date in _dates)
            {
                await LoadDate(date);
            }*/
            await GetCompValidation();
            foreach (var date in _dates)
            {
                LoadDate(date);
            }
            
        }

        public async Task<Dictionary<DateOnly, bool>> GetCompValidation()
        {
            var dict = new Dictionary<DateOnly, bool>();

            foreach (var date in _dates)
            {
                var compValidation = await _completedValidationController.GetCompValidation(_campaign.cmpid, TimeFormat.DateOnlyToYMDString(date));
                if (compValidation == null)
                {
                    await _completedValidationController.CreateCompValidation(new CompletedValidation(_campaign.cmpid, TimeFormat.DateOnlyToYMDString(date), false));
                    dict[date] = false;
                }
                else
                {
                    dict[date] = true;
                }
            }

            return dict;
        }

        public void LoadDate(DateOnly date)
        {
            var dateExpected = _dateExpectedDict[date];
            var dateRealized = _dateRealizedDict[date];
            bool isCompleted = false;

            ValidationDay validationDay = new ValidationDay(date, dateExpected, dateRealized, dgConfig.dgexp, dgConfig.dgreal, isCompleted);
            validationDay.InvertedExpectedColumnVisibility += ValidationDay_InvertedExpectedColumnVisibility;
            validationDay.InvertedRealizedColumnVisibility += ValidationDay_InvertedRealizedColumnVisibility;
            validationDay.UpdatedMediaPlanRealized += ValidationDay_UpdatedMediaPlanRealized;
            validationDay.ProgcoefChangedMediaPlanRealized += ValidationDay_ProgcoefChangedMediaPlanRealized;
            validationDay.CompletedValidationChanged += ValidationDay_CompletedValidationChanged;
            validationDay.CheckNewDataDay += ValidationDay_CheckNewDataDay;
            /*validationDay.GridOpened += ValidationDay_GridOpened;
            validationDay.GridClosed += ValidationDay_GridClosed;*/
            spValidationDays.Children.Add(validationDay);

            ValidationDaysDict[date] = validationDay;

        }

        private void ValidationDay_ProgcoefChangedMediaPlanRealized(object? sender, UpdateMediaPlanRealizedEventArgs e)
        {
            ProgcoefChangedMediaPlanRealized?.Invoke(this, e);
        }

        /*private void ValidationDay_GridClosed(object? sender, EventArgs e)
        {
            numGridsOpen -= 1;
            if (numGridsOpen == 0)
            {
                btnShowHideAll.Content = "Show All";
            }
        }

        private void ValidationDay_GridOpened(object? sender, EventArgs e)
        {
            numGridsOpen += 1;
            if(numGridsOpen > 0)
            {
                btnShowHideAll.Content = "Hide All";
            }
        }*/

        public void RefreshDate(DateOnly date)
        {
            var dateExpected = _dateExpectedDict[date];
            var dateRealized = _dateRealizedDict[date];

            ValidationDay validationDay = ValidationDaysDict[date];
            validationDay.RefreshDate(dateExpected, dateRealized);
        }

        private void ValidationDay_CheckNewDataDay(object? sender, CheckDateEventArgs e)
        {
            CheckNewDataDay?.Invoke(sender, e);
        }

        private async void ValidationDay_InvertedExpectedColumnVisibility(object? sender, IndexEventArgs e)
        {
            await InvertExpectedGridHeader(e.Index);
        }
        private async void ValidationDay_InvertedRealizedColumnVisibility(object? sender, IndexEventArgs e)
        {
            await InvertRealizedGridHeader(e.Index);
        }
        private void ValidationDay_UpdatedMediaPlanRealized(object? sender, UpdateMediaPlanRealizedEventArgs e)
        {
            UpdatedMediaPlanRealized?.Invoke(this, e);
        }

        private async void ValidationDay_CompletedValidationChanged(object? sender, CompletedValidationEventArgs e)
        {
            await _completedValidationController.UpdateCompValidation(new CompletedValidation(_campaign.cmpid, e.Date, e.IsCompleted));
        }

        private async Task InvertExpectedGridHeader(int index)
        {
            await InvertBit(dgConfig.dgexp, index, "exp");

            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertExpectedColumnVisibility(index);
            }
        }

        private async Task InvertRealizedGridHeader(int index)
        {

            await InvertBit(dgConfig.dgreal, index, "real");

            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertRealizedColumnVisibility(index);
            }
        }

        public async Task InvertBit(string bitString, int position, string type)
        {
            if (position < 0 || position >= bitString.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of range.");
            }

            char[] bitArray = bitString.ToCharArray();

            bitArray[position] = bitArray[position] == '0' ? '1' : '0';

            var newBitArray = new string(bitArray);
            if (string.Compare(type, "exp") == 0)
            {
                dgConfig.dgexp = newBitArray;
                await _dgConfigController.UpdateDGConfigExp(dgConfig.usrid, dgConfig.clid, dgConfig.dgexp);
            }
            else if (string.Compare(type, "real") == 0)
            {
                dgConfig.dgreal = newBitArray;
                await _dgConfigController.UpdateDGConfigReal(dgConfig.usrid, dgConfig.clid, dgConfig.dgreal);
            }
        }

        public void ClearStackPanel()
        {
            foreach (ValidationDay validationDay in spValidationDays.Children)
            {
                validationDay.InvertedExpectedColumnVisibility -= ValidationDay_InvertedExpectedColumnVisibility;
                validationDay.InvertedRealizedColumnVisibility -= ValidationDay_InvertedRealizedColumnVisibility;
                validationDay.UpdatedMediaPlanRealized -= ValidationDay_UpdatedMediaPlanRealized;
                validationDay.ProgcoefChangedMediaPlanRealized -= ValidationDay_ProgcoefChangedMediaPlanRealized;
                validationDay.CheckNewDataDay -= ValidationDay_CheckNewDataDay;
                /*validationDay.GridOpened -= ValidationDay_GridOpened;
                validationDay.GridClosed -= ValidationDay_GridClosed;*/
            }
            spValidationDays.Children.Clear();

        }

        private void HideExpectedStack()
        {
            //var validationDays = spValidationDays.Children.OfType<ValidationDay>();

            //Parallel.ForEach(validationDays, validationDay => validationDay.HideExpected());


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

        /*private void btnShowHideAll_Click(object sender, RoutedEventArgs e)
        {
            if (numGridsOpen > 0)
            {
                foreach (ValidationDay validationDay in spValidationDays.Children)
                {
                    validationDay.HideGrid();
                }
            }
            else
            {
                foreach (ValidationDay validationDay in spValidationDays.Children)
                {
                    validationDay.ShowGrid();
                }
            }
        }*/
    }
}

