using CampaignEditor.Controllers;
using CampaignEditor.Entities;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.CmpBrndDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Page in which we'll graphically see times of reserved and realized terms
    /// </summary>
    public partial class CampaignValidation : Page
    {

        private ChannelCmpController _channelCmpController;
        private ChannelController _channelController;
        private MediaPlanVersionController _mediaPlanVersionController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private MediaPlanRealizedController _mediaPlanRealizedController;
        private SpotController _spotController;
        private DatabaseFunctionsController _databaseFunctionsController;
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;

        private List<ChannelDTO> _channels = new List<ChannelDTO>();
        private List<DateOnly> _dates = new List<DateOnly>();
        // for sorting inside dictionaries
        private Dictionary<int, int> _chidOrder = new Dictionary<int, int>();

        public MediaPlanForecastData _forecastData;
        public MediaPlanConverter _mpConverter;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        private ObservableRangeCollection<MediaPlanRealized> _mediaPlanRealized = new ObservableRangeCollection<MediaPlanRealized>();
        private Dictionary<int, string> _spotnumNameDict = new Dictionary<int, string>();

        public Dictionary<DateOnly, List<TermTuple?>> _dateExpectedDict = new Dictionary<DateOnly, List<TermTuple?>>();
        public Dictionary<DateOnly, List<MediaPlanRealized?>> _dateRealizedDict = new Dictionary<DateOnly, List<MediaPlanRealized?>>();

        public event EventHandler SetLoadingPage;
        public event EventHandler SetContentPage;
        public CampaignValidation(
            IChannelCmpRepository channelCmpRepository,
            IChannelRepository channelRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanRealizedRepository mediaPlanRealizedRepository,
            ISpotRepository spotRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IBrandRepository brandRepository,
            ICmpBrndRepository cmpBrndRepository)
        {
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanRealizedController = new MediaPlanRealizedController(mediaPlanRealizedRepository);
            _spotController = new SpotController(spotRepository);
            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);
            _brandController = new BrandController(brandRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            //await CheckNewData();

            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(campaign.cmpid);
            if (mpVersion != null)
            {
                gridNotInitialized.Visibility = System.Windows.Visibility.Collapsed;
                gridValidation.Visibility = System.Windows.Visibility.Visible;

                await FillChannels(campaign);
                await FillDates(campaign);
                await GetMediaPlanRealized(campaign);
                InitializeDicts();
                await AlignExpectedRealized();

                validationStack._channelCmpController = _channelCmpController;
                validationStack._channelController = _channelController;
                validationStack._mediaPlanVersionController = _mediaPlanVersionController;
                validationStack._mediaPlanController = _mediaPlanController;
                validationStack._mediaPlanTermController = _mediaPlanTermController;
                validationStack._spotController = _spotController;
                validationStack._channels = _channels;
                validationStack._dates = _dates;
                validationStack._dateExpectedDict = _dateExpectedDict;
                validationStack._dateRealizedDict = _dateRealizedDict;
                validationStack._mpConverter = _mpConverter;
                validationStack._forecastData = _forecastData;
                validationStack._allMediaPlans = _allMediaPlans;
                validationStack._mediaPlanRealized = _mediaPlanRealized;
                validationStack._mediaPlanRealizedController = _mediaPlanRealizedController;
                
                await validationStack.Initialize(campaign);
            }
            else
            {
                gridValidation.Visibility = System.Windows.Visibility.Collapsed;
                gridNotInitialized.Visibility = System.Windows.Visibility.Visible;               
            }
        }

        private void InitializeDicts()
        {
            InitializeExpectedDict();
            InitializeRealizedDict();
        }
        private void InitializeExpectedDict()
        {
            _dateExpectedDict.Clear();

            foreach (var date in _dates)
            {
                foreach (var channel in _channels)
                {
                    // Initialize the expected list if it does not exist
                    if (!_dateExpectedDict.ContainsKey(date))
                    {
                        _dateExpectedDict[date] = new List<TermTuple?>();
                    }
                    _dateExpectedDict[date].AddRange(GetExpectedByDateAndChannel(date, channel));
                   
                }
            }
        }

        private void InitializeRealizedDict()
        {
            _dateRealizedDict.Clear();

            foreach (var date in _dates)
            {
                foreach (var channel in _channels)
                {                  
                    // Initialize the realized list if it does not exist
                    if (!_dateRealizedDict.ContainsKey(date))
                    {
                        _dateRealizedDict[date] = new List<MediaPlanRealized?>();
                    }
                    _dateRealizedDict[date].AddRange(GetRealizedByDateAndChannel(date, channel));
                }
            }
        }

        /* STATUSES 
            NULL - NEW
            -1 - EMPTY ITEM
            0 - UNDEFINED
            1 - OK
            2 - NOT OK
            3 - SAME HOUR
            4 - +- 1 HOUR
            5 - CHANGED VALUES SINCE LAST UPDATING  
            6 - DIFFERENT PRICE
            7 - DIFFERENT DURE DURF
         */
        private async Task AlignExpectedRealized()
        {
            foreach (var date in _dates)
            {
                await AlignExpectedRealizedByDate(_dateExpectedDict[date], _dateRealizedDict[date]);
            }
        }

        private async Task AlignExpectedRealizedByDate(List<TermTuple?> expected, List<MediaPlanRealized?> realized)
        {
            int n = expected.Count();
            int m = realized.Count();
            int minPeriod = 30;
            bool checkSameHour = true;

            int k = 0;
            while(k < n && k < m)
            {
                int expectedTime = TimeFormat.RepresentativeToMins(expected[k].MediaPlan.Blocktime);
                int realizedTime = (int)realized[k].stime/(int)60;
                if (expected[k].MediaPlan.chid == _forecastData.ChrdsidChidDict[realized[k].chid.Value])
                {
                    // Match
                    if (Math.Abs(expectedTime - realizedTime) <= minPeriod)
                    {
                        realized[k].MediaPlan = expected[k].MediaPlan;
                        // First time paired
                        if (realized[k].status == null || realized[k].status == 5)
                        {
                            //Calculating all coefs
                            /*await CalculateCoefs(realized[k], expected[k].MediaPlan);
                            realized[k].status = 1;*/
                            //await _mediaPlanRealizedController.UpdateMediaPlanRealized(realized[k]);
                        }
                        await CalculateCoefs(realized[k], expected[k].MediaPlan);
                        if (Math.Abs(realized[k].price.Value - expected[k].Price.Value) > 0.1M)
                        {
                            realized[k].status = 6;
                        }
                        else
                        {
                            if (Math.Abs(realized[k].dure.Value - realized[k].durf.Value) > 1)
                            {
                                realized[k].status = 7;
                            }
                            else
                            {
                                realized[k].status = 1;
                            }
                        }
                        expected[k].Status = 1;
                    }
                    else if (checkSameHour && IsSameHour(expected[k].MediaPlan.Blocktime, realized[k].stimestr))
                    {
                        // Check if next is in the minRange
                        if (k + 1 < m && realized[k+1].chid == realized[k].chid)
                        {
                            int realizedNextTime = (int)realized[k+1].stime / (int)60;
                            // If next is better match, mark this as not ok, and the next will be match
                            if (Math.Abs(expectedTime - realizedNextTime) <= minPeriod)
                            {
                                // Fill current row with empty expected
                                realized[k].MediaPlan = expected[k].MediaPlan;
                                await CalculateCoefs(realized[k], expected[k].MediaPlan);
                                realized[k].status = 2;
                                expected.Insert(k, new TermTuple(new MediaPlan(), new MediaPlanTerm(), new SpotDTO(0, "", "", 0, true), new TermCoefs(), ""));
                                n++;   
                            }
                            else
                            {
                                realized[k].MediaPlan = expected[k].MediaPlan;
                                await CalculateCoefs(realized[k], expected[k].MediaPlan);
                                realized[k].status = 3;
                                expected[k].Status = 1;
                            }
                        }
                        else
                        {
                            // First time paired
                            if (realized[k].status == null || realized[k].status == 5)
                            {
                                //Calculating all coefs
                                /*await CalculateCoefs(realized[k], expected[k].MediaPlan);
                                realized[k].status = 1;*/
                                //await _mediaPlanRealizedController.UpdateMediaPlanRealized(realized[k]);
                            }
                            realized[k].MediaPlan = expected[k].MediaPlan;
                            await CalculateCoefs(realized[k], expected[k].MediaPlan);
                            realized[k].status = 3;
                            expected[k].Status = 1;

                        }

                    }

                    // Spot planned but not realized
                    else if (expectedTime < realizedTime)
                    {
                        // When last values are unpaired, and realization is last for channel
                        if ((k+1 < n && k+1 < m && realized[k].chid != realized[k+1].chid && 
                            expected[k].MediaPlan.chid != expected[k+1].MediaPlan.chid))
                        {
                            var closestMediaPlan = FindClosestMediaPlan(realized[k]);
                            realized[k].MediaPlan = closestMediaPlan;
                            await CalculateCoefs(realized[k], closestMediaPlan);

                            realized[k].status = 2;
                        }
                        else
                        {
                            expected[k].Status = 2;
                            realized.Insert(k, new MediaPlanRealizedEmpty());
                            m++;
                        }
                        /*expected[k].Status = 2;
                        realized.Insert(k, new MediaPlanRealizedEmpty());
                        m++;*/
                    }
                    // Realization that is not planned
                    else
                    {
   
                        var closestMediaPlan = FindClosestMediaPlan(realized[k]);                      
                        realized[k].MediaPlan = closestMediaPlan;
                        await CalculateCoefs(realized[k], closestMediaPlan);
                                                                                                
                        realized[k].status = 2;

                        if (realized[k].status == null)
                        {
                            realized[k].status = 2;
                            //await _mediaPlanRealizedController.UpdateMediaPlanRealized(realized[j]);
                        }
                        expected.Insert(k, new TermTuple(new MediaPlan(), new MediaPlanTerm(), new SpotDTO(0, "", "", 0, true), new TermCoefs(), ""));
                        n++;
                    }
                }
                // Fill with empty fields when one channel in finished and other is not
                else if (_chidOrder[expected[k].MediaPlan.chid] < _chidOrder[_forecastData.ChrdsidChidDict[realized[k].chid.Value]])
                {
                    /*var minChid = expected[k].MediaPlan.chid;
                    int k1 = expected.Where(ex => ex != null && ex.Status != -1).Count(ex => ex.MediaPlan.chid == minChid);
                    int k2 = realized.Where(re => re != null && re.status != -1).Count(re => _forecastData.ChrdsidChidDict[re.chid.Value] == minChid);*/
                    expected[k].Status = 2;
                    realized.Insert(k, new MediaPlanRealizedEmpty());
                    m++;
                }
                else
                {
                    var closestMediaPlan = FindClosestMediaPlan(realized[k]);
                    realized[k].MediaPlan = closestMediaPlan;
                    await CalculateCoefs(realized[k], closestMediaPlan);
                    realized[k].status = 2;
                    if (realized[k].status == null)
                    {
                        realized[k].status = 2;
                        //await _mediaPlanRealizedController.UpdateMediaPlanRealized(realized[j]);
                    }
                    expected.Insert(k, new TermTuple(new MediaPlan(), new MediaPlanTerm(), new SpotDTO(0, "", "", 0, true), new TermCoefs(), ""));
                    n++;
                }
                k++;
            }       
            while (k < n)
            {
                expected[k].Status = 0;
                realized.Insert(k, new MediaPlanRealizedEmpty());
                k++;               
            }
            while (k < m)
            {
                if (realized[k].status == null)
                {
                    realized[k].status = 2;
                    //await _mediaPlanRealizedController.UpdateMediaPlanRealized(realized[j]);
                }
                else if (realized[k].status == 1)
                    realized[k].status = 0;
                expected.Insert(k, new TermTuple(new MediaPlan(), new MediaPlanTerm(), new SpotDTO(0, "", "", 0, true), new TermCoefs(), ""));
                k++;
            }

        }

        private MediaPlan FindClosestMediaPlan(MediaPlanRealized mpRealized)
        {
            var closestMediaPlan = _allMediaPlans.FirstOrDefault(mp => (mp.MediaPlan.chid == _forecastData.ChrdsidChidDict[mpRealized.chid.Value] &&
                            String.Compare(mp.MediaPlan.stime, TimeFormat.TimeStrToRepresentative(mpRealized.stimestr)) <= 0 &&
                            String.Compare(mp.MediaPlan.etime, TimeFormat.TimeStrToRepresentative(mpRealized.stimestr)) >= 0 &&
                            mp.MediaPlan.days.Contains(TimeFormat.GetDayOfWeekInt(mpRealized.date).ToString())));
            if (closestMediaPlan != null)
            {
                return closestMediaPlan.MediaPlan;
            }
            else
            {
                closestMediaPlan = _allMediaPlans
                    .Where(mp =>
                        mp.MediaPlan.chid == _forecastData.ChrdsidChidDict[mpRealized.chid.Value] &&
                        mp.MediaPlan.days.Contains(TimeFormat.GetDayOfWeekInt(mpRealized.date).ToString()))
                    .OrderBy(mp => Math.Abs(TimeFormat.RepresentativeToMins(mp.MediaPlan.blocktime) - (int)mpRealized.stime / 60))
                    .FirstOrDefault();

                if (closestMediaPlan != null)
                {
                    return closestMediaPlan.MediaPlan;
                }
            }
            return new MediaPlan();
        }

        private bool IsSameHour(string timestr1, string timestr2)
        {
            return String.Compare(timestr1.Substring(0, 2), timestr2.Substring(0, 2)) == 0;
        }

        private List<TermTuple?> GetExpectedByDateAndChannel(DateOnly date, ChannelDTO channel)
        {
            List<TermTuple> termTuples = new List<TermTuple>();

            foreach (var mediaPlanTuple in _allMediaPlans.Where(mpt => mpt.MediaPlan.chid == channel.chid))
            {
                var mediaPlanTerms = mediaPlanTuple.Terms.Where(t => t != null && t.Date == date && t.Spotcode != null && 
                                                    t.Spotcode.Count() > 0);
                foreach (var mediaPlanTerm in mediaPlanTerms)
                {
                    foreach (char spotcode in mediaPlanTerm!.Spotcode!.Trim())
                    {
                        var spot = _forecastData.SpotcodeSpotDict[spotcode];
                        // It's better to have info from allMediaPlans
                        var mediaPlan = mediaPlanTuple.MediaPlan;
                        var termCoefs = new TermCoefs(mediaPlan.Amrpsale, mediaPlan.Cpp);
                        var price = _mpConverter.GetProgramSpotPrice(mediaPlan, mediaPlanTerm, spot, termCoefs);
                        TermTuple termTuple = new TermTuple(mediaPlan, mediaPlanTerm, spot, termCoefs, channel.chname.Trim());
                        termTuples.Add(termTuple);
                    }

                }
            }

            termTuples = termTuples.OrderBy(tt => tt.MediaPlan.blocktime).ToList();
            return termTuples;
        }

        private List<MediaPlanRealized?> GetRealizedByDateAndChannel(DateOnly date, ChannelDTO channel)
        {

            var mediaPlanRealizes = _mediaPlanRealized.Where(mpr => _forecastData.ChrdsidChidDict[mpr.chid.Value] == channel.chid && 
                                                TimeFormat.YMDStringToDateOnly(mpr.date) == date);
            mediaPlanRealizes.ToList().ForEach(mpr => mpr.Channel = _forecastData.ChrdsChannelDict[mpr.chid.Value]);
            mediaPlanRealizes = mediaPlanRealizes.OrderBy(mpr => mpr.stime.Value).ToList();
            return mediaPlanRealizes.ToList();
        }

        private async Task FillChannels(CampaignDTO campaign)
        {           
            _channels = _forecastData.Channels;
            int i = 0;
            foreach (var channel in _forecastData.Channels)
            {
                _chidOrder[channel.chid] = i++;
            }
            FillChannelsComboBox(_channels);
        }

        private void FillChannelsComboBox(IEnumerable<ChannelDTO> channels)
        {
            foreach (ChannelDTO channel in channels)
            {
                ComboBoxItem cbiChannel = new ComboBoxItem();
                cbiChannel.DataContext = channel;
                cbiChannel.Content = channel.chname;

                cbChannels.Items.Add(cbiChannel);
            }
        }

        private async Task FillDates(CampaignDTO campaign)
        {
            var startDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
            var endDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

            if (startDate > endDate)
            {
                return;
            }

            DateTime date = startDate;
            while (date <= endDate)
            {
                _dates.Add(DateOnly.FromDateTime(date));
                date = date.AddDays(1);
            }
        }

        private async Task GetMediaPlanRealized(CampaignDTO campaign)
        {
            var mpRealized = await _mediaPlanRealizedController.GetAllMediaPlansRealizedByCmpid(campaign.cmpid);
            _mediaPlanRealized.ReplaceRange(mpRealized);
        }

        private async Task CheckRealizedPrice()
        {
            foreach (var mediaPlanRealized in _mediaPlanRealized)
            {
                if (mediaPlanRealized.price == null || mediaPlanRealized.status == 5)
                {
                    await CalculateCoefs(mediaPlanRealized);
                }
            }
        }

        private async Task CalculateCoefs(MediaPlanRealized mediaPlanRealized, MediaPlan mediaPlan)
        {
            var chid = _forecastData.ChrdsidChidDict[mediaPlanRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];
            mediaPlanRealized.chcoef = mediaPlan.chcoef;
            mediaPlanRealized.progcoef = mediaPlan.progcoef;
            mediaPlanRealized.coefA = mediaPlan.coefA;
            mediaPlanRealized.coefB = mediaPlan.coefB;

            // For calculating
            _mpConverter.CalculateRealizedDPCoef(mediaPlanRealized);
            _mpConverter.CalculateRealizedCoefs(mediaPlanRealized, pricelist);

            // Getting name of spot from nazreklame 
            int spotnum = mediaPlanRealized.spotnum.Value;
            if (_spotnumNameDict.ContainsKey(spotnum))
            {
                mediaPlanRealized.spotname = _spotnumNameDict[spotnum];
            }
            else
            {
                var progName = await _mediaPlanRealizedController.GetDedicatedSpotName(spotnum);
                if (progName != null)
                {
                    _spotnumNameDict[spotnum] = progName.Trim();
                    mediaPlanRealized.spotname = progName.Trim();
                }
            }



        }

        private async Task CalculateCoefs(MediaPlanRealized mediaPlanRealized)
        {
            var chid = _forecastData.ChrdsidChidDict[mediaPlanRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];
            _mpConverter.CalculateRealizedDPCoef(mediaPlanRealized);
            _mpConverter.CalculateRealizedCoefs(mediaPlanRealized, pricelist);


            int spotnum = mediaPlanRealized.spotnum.Value;
            if (_spotnumNameDict.ContainsKey(spotnum))
            {
                mediaPlanRealized.spotname = _spotnumNameDict[spotnum];
            }
            else
            {
                var progName = await _mediaPlanRealizedController.GetDedicatedSpotName(spotnum);
                if (progName != null)
                {
                    _spotnumNameDict[spotnum] = progName.Trim();
                    mediaPlanRealized.spotname = progName.Trim();
                }
            }
            
            
            
        }

        private async void cbChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChannels.SelectedItem is ComboBoxItem selectedChannelItem)
            {
                if (selectedChannelItem.DataContext is ChannelDTO selectedChannel)
                {
                    await validationStack.LoadData(selectedChannel.chid);

                }
            }
        }

        private async Task<bool> CheckNewData()
        {
            var campaign = _forecastData.Campaign;
            var cmpBrnds = (await _cmpBrndController.GetCmpBrndsByCmpId(campaign.cmpid)).ToList();
            if (cmpBrnds == null || cmpBrnds.Count == 0)
            {
                MessageBox.Show("No brand is selected for this campaign", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }            
            try
            {
                foreach (CmpBrndDTO cmpbrnd in cmpBrnds)
                {
                    await _databaseFunctionsController.StartRealizationFunction(campaign.cmpid,
                         cmpbrnd.brbrand, campaign.cmpsdate, campaign.cmpedate);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching for new realizations", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

 
        }

        private async void btnCheckRealized_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetLoadingPage?.Invoke(this, null);
            if (await CheckNewData())
            {
                await GetMediaPlanRealized(_forecastData.Campaign);
                InitializeRealizedDict();
                await AlignExpectedRealized();
                await validationStack.Initialize(_forecastData.Campaign);
            }
            SetContentPage?.Invoke(this, null);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            bool allDecimals = (bool)chbAllDecimals.IsChecked;
            validationStack.Print(allDecimals);
        }

        public void CloseValidation()
        {
            validationStack.ClearStackPanel();
        }
    }
}
