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

        public Dictionary<DateOnly, List<TermTuple>> _dayTermDict = new Dictionary<DateOnly, List<TermTuple>>();
        public Dictionary<DateOnly, List<MediaPlanRealized>> _dayRealizedDict = new Dictionary<DateOnly, List<MediaPlanRealized>>();

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

        }

        public async Task LoadData(int chid)
        {

            await LoadExpected(chid);
            await LoadRealized(chid);
            LoadDays();
        }

        private void LoadDays()
        {
            spValidationDays.Children.Clear();

            foreach (var day in _dates)
            {
                var dayTerms = _dayTermDict[day];
                var dayRealizeds = _dayRealizedDict[day];

                ValidationDay validationDay = new ValidationDay(day, dayTerms, dayRealizeds);
                validationDay._mediaPlanRealizedController = _mediaPlanRealizedController;
                validationDay.SetUserControl();

                spValidationDays.Children.Add(validationDay);
            }
        }

        private async Task LoadExpected(int chid)
        {

            List<TermTuple> termTuples = new List<TermTuple>();

            //var mediaPlans = await _mediaPlanController.GetAllChannelCmpMediaPlans(chid, _campaign.cmpid, _campaignVersion);
            var mediaPlanTuples = _allMediaPlans.Where(mpt => mpt.MediaPlan.chid == chid);
            foreach (var mediaPlanTuple in mediaPlanTuples)
            {
                //var mediaPlanTerms = await _mediaPlanTermController.GetAllNotNullMediaPlanTermsByXmpid(mediaPlan.xmpid);
                var mediaPlanTerms = mediaPlanTuple.Terms.Where(t => t != null && t.Spotcode != null);
                foreach (var mediaPlanTerm in mediaPlanTerms)
                {
                    foreach (char spotcode in mediaPlanTerm!.Spotcode!.Trim())
                    {
                        var spot = _forecastData.SpotcodeSpotDict[spotcode];
                        // It's better to have info from allMediaPlans
                        var mediaPlan = mediaPlanTuple.MediaPlan;
                        var termCoefs = new TermCoefs();
                        var price = _mpConverter.GetProgramSpotPrice(mediaPlan, mediaPlanTerm, spot, termCoefs);
                        TermTuple termTuple = new TermTuple(mediaPlan, mediaPlanTerm, spot, termCoefs);
                        termTuples.Add(termTuple);
                    }

                }
            }

            _dayTermDict.Clear();

            termTuples = termTuples.OrderBy(tt => tt.MediaPlanTerm.Date).ThenBy(tt => tt.MediaPlan.stime).ToList();
            foreach (DateOnly date in _dates)
            {
                List<TermTuple> dayTuples = termTuples.Where(tt => tt.MediaPlanTerm.Date == date).ToList();
                _dayTermDict[date] = dayTuples;
            }

            //AddIntoExpected(_dayTuples);
        }

        private async Task LoadRealized(int chid)
        {
            if (_mediaPlanRealized == null)
            {
                return;
            }
            var chrdsid = _forecastData.ChrdsidChidDict.First(kv => kv.Value == chid).Key;
            var realized = _mediaPlanRealized.Where(mpr => mpr.chid == chrdsid);
            realized = realized.OrderBy(r => r.date).ThenBy(tt => tt.stime).ToList();

            _dayRealizedDict.Clear();

            foreach (DateOnly date in _dates)
            {
                List<MediaPlanRealized> dayTuples = realized.Where(tt => TimeFormat.YMDStringToDateOnly(tt.date) == date).ToList();
                _dayRealizedDict[date] = dayTuples;
            }
            //AddIntoRealized(_dayRealizedTuples);
        }

        private void AddIntoExpected(List<Tuple<DateOnly, List<TermTuple>>> dayTuples)
        {
            /*spExpected.Children.Clear();

            foreach (var tuple in dayTuples)
            {

                string date = tuple.Item1.ToString();
                DateLabelItem dli = new DateLabelItem(date);

                spExpected.Children.Add(dli);

                foreach (var termTuple in tuple.Item2)
                {
                    VTermItem vTermItem = new VTermItem();
                    vTermItem.Initialize(termTuple);

                    spExpected.Children.Add(vTermItem);
                }

            }*/
        }

        private void AddIntoRealized(List<Tuple<DateOnly, List<MediaPlanRealized>>> dayTuples)
        {
            /*spRealized.Children.Clear();

            foreach (var tuple in dayTuples)
            {

                string date = tuple.Item1.ToString();
                DateLabelItem dli = new DateLabelItem(date);
                spRealized.Children.Add(dli);

                foreach (var realized in tuple.Item2)
                {
                    VRealizedItem vRealizedItem = new VRealizedItem();
                    vRealizedItem._mpRController = _mediaPlanRealizedController;
                    vRealizedItem.Initialize(realized);

                    spRealized.Children.Add(vRealizedItem);
                }

            }*/
        }

        
    }
}
