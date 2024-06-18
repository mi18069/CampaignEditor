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

        private CampaignDTO _campaign;
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

        bool hideExpected = false;
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

        private bool CheckPrerequisites()
        {
            if (_forecastData.Channels.Count == 0)
            {
                gridValidation.Visibility = System.Windows.Visibility.Collapsed;
                gridNotInitialized.Visibility = System.Windows.Visibility.Visible;
                return false;
            }
            return true;
        }

        public async Task Initialize(CampaignDTO campaign, ObservableRangeCollection<MediaPlanTuple> allMediaPlans)
        {
            _campaign = campaign;

            if (!CheckPrerequisites())
            {
                return;
            }

            //await CheckNewData();

            _allMediaPlans = allMediaPlans;

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

            hideExpected = _allMediaPlans.Count == 0;
            await validationStack.Initialize(campaign, hideExpected);
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

        private async Task PairMediaPlans(MediaPlanRealized mpR, TermTuple tt = null)
        {
            if (mpR.status == null || mpR.status == 5 || (tt != null && mpR.status == 1))
            {
                //Calculating all coefs

                //await _mediaPlanRealizedController.UpdateMediaPlanRealized(realized[k]);

            }

            await SetRealizedName(mpR);
            var mediaPlan = tt == null ? null : tt.MediaPlan;
            mpR.MediaPlan = mediaPlan;
            CalculateCoefs(mpR);

        }

        private void SetStatus(MediaPlanRealized mpR, TermTuple tt)
        {
            if (Math.Abs(mpR.dure.Value - mpR.durf.Value) > 1)
            {
                mpR.status = 7;
            }
            else if(tt.Price.HasValue && mpR.price.HasValue && Math.Abs(tt.Price.Value - mpR.price.Value) > 0.1M)
            {
                mpR.status = 6;
            }
            else
            {
                mpR.status = 1;
            }

            tt.Status = 1;
        }

        private void AddEmptyRealized(List<MediaPlanRealized?> realized, int index)
        {
            realized.Insert(index, new MediaPlanRealizedEmpty());
        }

        private void AddEmptyExpected(List<TermTuple?> expected, int index)
        {
            expected.Insert(index, new TermTuple(new MediaPlan(), new MediaPlanTerm(), new SpotDTO(0, "", "", 0, true), new TermCoefs(), ""));
        }

        private async Task AlignExpectedRealizedByDate(List<TermTuple?> expected, List<MediaPlanRealized?> realized)
        {
            int n = expected.Count();
            int m = realized.Count();
            int minPeriod = 30;
            bool checkSameHour = true;

            int k = 0;
            while (k < n && k < m)
            {

                int expectedTime = TimeFormat.RepresentativeToMins(expected[k].MediaPlan.Blocktime);
                int realizedTime = (int)realized[k].stime / (int)60;

                int expectedChid = expected[k].MediaPlan.chid;
                int realizedChid = _forecastData.ChrdsidChidDict[realized[k].chid.Value];

                if (expectedChid == 0)
                {
                    k++;
                    continue;
                }
                if (expectedChid == realizedChid)
                {
                    // Match within minPeriod
                    if (Math.Abs(expectedTime - realizedTime) <= minPeriod)
                    {
                        await PairMediaPlans(realized[k], expected[k]);
                        SetStatus(realized[k], expected[k]);
                    }
                    // Check if it's in the same hour
                    else if (checkSameHour && IsSameHour(expectedTime, realizedTime))
                    {
                        bool betterPairFound = false;

                        if (expectedTime > realizedTime)
                        {
                            // Check next realized
                            if (k + 1 < m)
                            {
                                var nextRealized = realized[k + 1];
                                int nextRealizedTime = (int)realized[k + 1].stime / (int)60;

                                // When next realization is better, pair with mediaPlan anyway 
                                // but set status to 2 and add empty expected
                                if (nextRealized.chid == realized[k].chid &&
                                    Math.Abs(expectedTime - nextRealizedTime) <= minPeriod)
                                {
                                    await PairMediaPlans(realized[k], expected[k]);
                                    realized[k].status = 2;
                                    AddEmptyExpected(expected, k);
                                    n++;
                                    betterPairFound = true;
                                }
                            }

                        }
                        else
                        {
                            // Check next expected
                            if (k + 1 < n)
                            {
                                var nextExpected = expected[k + 1];
                                int nextExpectedTime = TimeFormat.RepresentativeToMins(nextExpected.MediaPlan.Blocktime);

                                // When next expected is better, mark it as unrealized
                                if (nextExpected.MediaPlan.chid == expected[k].MediaPlan.chid &&
                                    Math.Abs(expectedTime - nextExpectedTime) <= minPeriod)
                                {
                                    expected[k].Status = 2;
                                    AddEmptyRealized(realized, k);
                                    m++;
                                    betterPairFound = true;
                                }
                            }
                        }

                        // No better pair, pair current
                        if (!betterPairFound)
                        {
                            await PairMediaPlans(realized[k], expected[k]);
                            SetStatus(realized[k], expected[k]);
                        }
                    }
                    // No match, add empty rows
                    else
                    {
                        // Add empty expected row
                        if (expectedTime > realizedTime)
                        {
                            await PairMediaPlans(realized[k], null);
                            realized[k].status = 2;
                            AddEmptyExpected(expected, k);
                            n++;
                        }
                        // Add empty realized row
                        else
                        {
                            expected[k].Status = 2;
                            AddEmptyRealized(realized, k);
                            m++;
                        }
                    }
                }
                // Different chids
                else
                {
                    // Add empty realized row
                    if (_chidOrder[expectedChid] < _chidOrder[realizedChid])
                    {
                        expected[k].Status = 2;
                        AddEmptyRealized(realized, k);
                        m++;
                    }
                    // Add empty expected row
                    else
                    {
                        await PairMediaPlans(realized[k], null);
                        realized[k].status = 2;
                        AddEmptyExpected(expected, k);
                        n++;
                    }
                }

                k++;
            }
            // All realized used, fill the rest of expected
            while (k < n)
            {
                expected[k].Status = -1;
                AddEmptyRealized(realized, k);
                k++;
            }
            // All expected used, fill the rest of realizations
            while (k < m)
            {
                await PairMediaPlans(realized[k], null);
                realized[k].status = 2;
                // If nothing is initialized , no need to add empty expected one by one
                if (_allMediaPlans.Count != 0)
                    AddEmptyExpected(expected, k);
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
        private bool IsSameHour(int mins1, int mins2)
        {
            return mins1/60 == mins2/60; // dividing int/int to get hour then compare them
        }

        private List<TermTuple?> GetExpectedByDateAndChannel(DateOnly date, ChannelDTO channel)
        {
            List<TermTuple?> termTuples = new List<TermTuple?>();

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
                    CalculateCoefs(mediaPlanRealized);
                }
            }
        }

        private void CalculateCoefs(MediaPlanRealized mediaPlanRealized)
        {
            var chid = _forecastData.ChrdsidChidDict[mediaPlanRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];
 
            _mpConverter.CalculateRealizedCoefs(mediaPlanRealized, pricelist);
         
        }

        private async Task SetRealizedName(MediaPlanRealized mediaPlanRealized)
        {
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
                await validationStack.Initialize(_forecastData.Campaign, hideExpected);
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

        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            await Initialize(_campaign, _allMediaPlans);
        }
    }
}
