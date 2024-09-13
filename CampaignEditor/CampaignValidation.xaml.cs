using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.CmpBrndDTO;
using Database.DTOs.MediaPlanRealizedDTO;
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
        private CompletedValidationController _completedValidationController;
        private DGConfigController _dgConfigController;
        private ClientRealizedCoefsController _clientRealizedCoefsController;
        private CampaignController _campaignController;


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
        private readonly PrintValidation _factoryPrintValidation;
        private IAbstractFactory<PairSpots> _factoryPairSpots;
        private IEnumerable<int> uniqueSpotNums;

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
            ICmpBrndRepository cmpBrndRepository,
            ICompletedValidationRepository completedValidationRepository,
            IAbstractFactory<PrintValidation> factoryPrintValidation,
            IDGConfigRepository dGConfigRepository,
            IClientRealizedCoefsRepository clientRealizedCoefsRepository,
            ICampaignRepository campaignRepository,
            IAbstractFactory<PairSpots> factoryPairSpots)
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
            _completedValidationController = new CompletedValidationController(completedValidationRepository);
            _dgConfigController = new DGConfigController(dGConfigRepository);
            _clientRealizedCoefsController = new ClientRealizedCoefsController(clientRealizedCoefsRepository);
            _campaignController = new CampaignController(campaignRepository);

            _factoryPrintValidation = factoryPrintValidation.Create();
            _factoryPairSpots = factoryPairSpots;

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
            _factoryPrintValidation.Initialize(campaign);

            if (!CheckPrerequisites())
            {
                return;
            }

            //await CheckNewData();

            _allMediaPlans = allMediaPlans;

            gridNotInitialized.Visibility = System.Windows.Visibility.Collapsed;
            gridValidation.Visibility = System.Windows.Visibility.Visible;
            //Stopwatch s0 = new Stopwatch();
            //s0.Start();
            FillChannels();
            //s0.Stop();

            //Stopwatch s1 = new Stopwatch();
            //s1.Start();
            await FillDates(campaign);
            //s1.Stop();

            //Stopwatch s2 = new Stopwatch();
            //s2.Start();
            await GetMediaPlanRealized(campaign);
            //s2.Stop();

            //Stopwatch s3 = new Stopwatch();
            //s3.Start();
            await FillSpotNameDict(campaign);
            //s3.Stop();

            //Stopwatch s4 = new Stopwatch();
            //s4.Start();
            InitializeDicts();
            //s4.Stop();

            //Stopwatch s5 = new Stopwatch();
            //s5.Start();
            await AlignExpectedRealized();
            //s5.Stop();

            BindValidationStack();

            hideExpected = _allMediaPlans.Count == 0;
            /*Stopwatch s6 = new Stopwatch();
            s6.Start();*/
            await validationStack.Initialize(campaign, hideExpected);
            //s6.Stop();
            validationStack.UpdatedMediaPlanRealized += ValidationStack_UpdatedMediaPlanRealized;
            validationStack.CheckNewDataDay += ValidationStack_CheckNewDataDay;

            BindPrintValidation();

            /*MessageBox.Show($"0: {s0.ElapsedMilliseconds}\n" +
                $"1: {s1.ElapsedMilliseconds}\n" +
                $"2: {s2.ElapsedMilliseconds}\n" +
                $"3: {s3.ElapsedMilliseconds}\n" +
                $"4: {s4.ElapsedMilliseconds}\n" +
                $"5: {s5.ElapsedMilliseconds}\n" +
                $"6: {s6.ElapsedMilliseconds}\n", "");*/
        }

        private async void ValidationStack_CheckNewDataDay(object? sender, CheckDateEventArgs e)
        {
            SetLoadingPage?.Invoke(this, null);
            var date = e.date;
            if (await CheckNewDateData(date))
            {
                await GetMediaPlanRealized(_forecastData.Campaign);
                await FillSpotNameDict(_campaign);
                InitializeDateRealizedDict(date);
                await AlignExpectedRealized(date);
                //await validationStack.Initialize(_forecastData.Campaign, hideExpected);
                //validationStack.LoadDate(date);
                validationStack.RefreshDate(date);
            }
            SetContentPage?.Invoke(this, null);
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
                InitializeDateExpectedDict(date);
            }

        }

        private void InitializeDateExpectedDict(DateOnly date)
        {
            if (!_dateExpectedDict.ContainsKey(date))
            {
                _dateExpectedDict[date] = new List<TermTuple?>();
            }
            else
            {
                _dateExpectedDict[date].Clear();
            }

            foreach (var channel in _channels)
            {
                _dateExpectedDict[date].AddRange(GetExpectedByDateAndChannel(date, channel));
            }

            /*var results = new ConcurrentBag<List<TermTuple?>>();

            // Parallel fetching of results
            Parallel.ForEach(_channels, channel =>
            {
                var result = GetExpectedByDateAndChannel(date, channel);
                results.Add(result);
            });

            // Combine all the results and add them to the dictionary
            _dateExpectedDict[date].AddRange(results.SelectMany(x => x));*/
        }

        private void InitializeRealizedDict()
        {
            _dateRealizedDict.Clear();

            foreach (var date in _dates)
            {
                InitializeDateRealizedDict(date);
            }

        }

        private void InitializeDateRealizedDict(DateOnly date)
        {
            if (!_dateRealizedDict.ContainsKey(date))
            {
                _dateRealizedDict[date] = new List<MediaPlanRealized?>();
            }
            else 
            {
                _dateRealizedDict[date].Clear();
            }

            foreach (var channel in _channels)
            {
                _dateRealizedDict[date].AddRange(GetRealizedByDateAndChannel(date, channel));
            }
            /*var results = new ConcurrentBag<List<MediaPlanRealized?>>();

            // Parallel fetching of results
            Parallel.ForEach(_channels, channel =>
            {
                var result = GetRealizedByDateAndChannel(date, channel);
                results.Add(result);
            });

            _dateRealizedDict[date].AddRange(results.SelectMany(x => x));*/

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
            6 - DIFFERENT COEFS
            7 - DIFFERENT DURE DURF
         */
        private async Task AlignExpectedRealized(DateOnly? alignDate = null)
        {
            if (alignDate != null)
            {
                _dateExpectedDict[alignDate.Value] = _dateExpectedDict[alignDate.Value].Where(tt => tt.Status != -1).ToList();
                _dateRealizedDict[alignDate.Value] = _dateRealizedDict[alignDate.Value].Where(mpR => mpR.Status != -1).ToList();
                await AlignExpectedRealizedByDate(_dateExpectedDict[alignDate.Value], _dateRealizedDict[alignDate.Value]);
            }
            else
            {
                foreach (var date in _dates)
                {
                    await AlignExpectedRealizedByDate(_dateExpectedDict[date], _dateRealizedDict[date]);
                }
            }


        }

        private void PairMediaPlans(MediaPlanRealized mpR, TermTuple tt = null)
        {
            if (mpR.status == -1)
                return;

            var mediaPlan = tt == null ? null : tt.MediaPlan;
            mpR.MediaPlan = mediaPlan;
            //SetStatus(mpR, tt);


            // Calculate and add values
            /*if (mpR.status == null || mpR.status == 5 || (mpR.price == null))
            {
                //await SetRealizedName(mpR);
                var mediaPlan = tt == null ? null : tt.MediaPlan;
                mpR.MediaPlan = mediaPlan;
                //CalculateCoefs(mpR);
                SetStatus(mpR, tt);
                await _mediaPlanRealizedController.UpdateMediaPlanRealized(mpR);
            }*/

        }

        private async Task SetStatus(MediaPlanRealized mpR, TermTuple tt = null)
        {
            if (tt != null)
                tt.Status = 1;

            if (mpR.status != null)
                return;

            decimal eps = 0.001M;

            if (Math.Abs(mpR.dure.Value - mpR.durf.Value) > 1)
            {
                mpR.status = 7;
            }
            else if (tt == null)
            {
                mpR.status = 2;
            }
            else if (tt.Progcoef - mpR.Progcoef > eps || tt.CoefA - mpR.CoefA > eps ||
                tt.CoefB - mpR.CoefB > eps && tt.Seascoef - mpR.Seascoef > eps || 
                tt.Seccoef - mpR.Seccoef > eps && tt.Dpcoef - mpR.Dpcoef > eps)
            {
                mpR.status = 6;
            }
            else
            {
                mpR.status = 1;
            }

            await _mediaPlanRealizedController.UpdateMediaPlanRealized(mpR);
        }       

        private void AddEmptyRealized(List<MediaPlanRealized?> realized, int index, int chid = -1)
        {
            var mediaPlanRealizedEmpty = new MediaPlanRealizedEmpty();
            mediaPlanRealizedEmpty.chid = chid;
            realized.Insert(index, mediaPlanRealizedEmpty);
        }

        private void AddEmptyExpected(List<TermTuple?> expected, int index, int chid = -1)
        {
            var termTuple = new TermTuple(new MediaPlan(), new MediaPlanTerm(), new SpotDTO(0, "", "", 0, true), new TermCoefs(), "");
            termTuple.MediaPlan.chid = chid;
            expected.Insert(index, termTuple);
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
                        PairMediaPlans(realized[k], expected[k]);
                        await SetStatus(realized[k], expected[k]);
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
                                    PairMediaPlans(realized[k], expected[k]);
                                    await SetStatus(realized[k], null);
                                    AddEmptyExpected(expected, k, expectedChid);
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
                                    AddEmptyRealized(realized, k, realized[k].chid.Value);
                                    m++;
                                    betterPairFound = true;
                                }
                            }
                        }

                        // No better pair, pair current
                        if (!betterPairFound)
                        {
                            PairMediaPlans(realized[k], expected[k]);
                            await SetStatus(realized[k], expected[k]);
                        }
                    }
                    // No match, add empty rows
                    else
                    {
                        // Add empty expected row
                        if (expectedTime > realizedTime)
                        {
                            PairMediaPlans(realized[k], null);
                            await SetStatus(realized[k], null);
                            AddEmptyExpected(expected, k, expectedChid);
                            n++;
                        }
                        // Add empty realized row
                        else
                        {
                            expected[k].Status = 2;
                            AddEmptyRealized(realized, k, realized[k].chid.Value);
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
                        //AddEmptyRealized(realized, k, realized[k].chid.Value);
                        AddEmptyRealized(realized, k, _forecastData.ChrdsidChidDict.First(dict => dict.Value == expectedChid).Key);
                        m++;
                    }
                    // Add empty expected row
                    else
                    {
                        PairMediaPlans(realized[k], null);
                        await SetStatus(realized[k], null);
                        AddEmptyExpected(expected, k, _forecastData.ChrdsidChidDict[realized[k].chid.Value]);
                        n++;
                    }
                }

                k++;
            }
            // All realized used, fill the rest of expected
            while (k < n)
            {
                expected[k].Status = -1;
                //AddEmptyRealized(realized, k, realized[m-1].chid.Value);
                int chrdsid = -1;
                if (k == 0)
                    chrdsid = _forecastData.ChrdsidChidDict.First(c => c.Value == expected[k].MediaPlan.chid).Key;
                else                
                    chrdsid = realized[k - 1].chid.Value;
                
                AddEmptyRealized(realized, k, chrdsid);
                k++;
            }
            // All expected used, fill the rest of realizations
            while (k < m)
            {

                PairMediaPlans(realized[k], null);
                await SetStatus(realized[k], null);
                // If nothing is initialized , no need to add empty expected one by one
                if (_allMediaPlans.Count != 0)
                    AddEmptyExpected(expected, k, _forecastData.ChrdsidChidDict[realized[k].chid.Value]);
                k++;
            }
          
        }

        // Approach where we go channel by channel, the problem is that probably we'll
        // need some structure like Dictionary<Date, OrderedDictionary<Channel, List<Value>>>
        /*private async Task AlignExpectedRealizedByDate(List<TermTuple?> expected, List<MediaPlanRealized?> realized)
        {

            foreach (var channel in _channels)
            {
                var chid = channel.chid;
                var chrdsid = _forecastData.ChrdsidChidDict.First(dict => dict.Value == chid).Key;
                var expectedChannel = expected.Where(tt => tt != null && tt.MediaPlan.chid == chid).OrderBy(tt => tt.MediaPlan.blocktime).ToList();
                var realizedChannel = realized.Where(mpR => mpR != null && mpR.chid == chrdsid).OrderBy(mpR => mpR.stime.Value).ToList();

                await AlignExpectedRealizedByDateAndChannel(expectedChannel, realizedChannel, chid, chrdsid);
            }

            

        }

        private async Task AlignExpectedRealizedByDateAndChannel(List<TermTuple?> expectedChannel, List<MediaPlanRealized?> realizedChannel, int chid, int chrdsid)
        {
            int n = expectedChannel.Count();
            int m = realizedChannel.Count();
            int minPeriod = 30;
            bool checkSameHour = true;

            int k = 0;
            while (k < n && k < m)
            {

                int expectedTime = TimeFormat.RepresentativeToMins(expectedChannel[k].MediaPlan.Blocktime);
                int realizedTime = (int)realizedChannel[k].stime / (int)60;

                // Match within minPeriod
                if (Math.Abs(expectedTime - realizedTime) <= minPeriod)
                {
                    await PairMediaPlans(realizedChannel[k], expectedChannel[k]);
                }
                // Check if it's in the same hour
                else if (checkSameHour && IsSameHour(expectedTime, realizedTime))
                {
                    bool betterPairFound = false;

                    if (expectedTime > realizedTime && k + 1 < m)
                    {
                        // Check next realized

                        var nextRealized = realizedChannel[k + 1];
                        int nextRealizedTime = (int)nextRealized.stime / (int)60;

                        // When next realization is better, pair with mediaPlan anyway 
                        // but set status to 2 and add empty expected
                        if (Math.Abs(expectedTime - nextRealizedTime) <= minPeriod)
                        {
                            await PairMediaPlans(realizedChannel[k], expectedChannel[k]);
                            realizedChannel[k].status = 2;
                            AddEmptyExpected(expectedChannel, k, chid);
                            n++;
                            betterPairFound = true;
                        }
                        

                    }
                    else if (k + 1 < n)
                    {
                        // Check next expected
   
                        var nextExpected = expectedChannel[k + 1];
                        int nextExpectedTime = TimeFormat.RepresentativeToMins(nextExpected.MediaPlan.Blocktime);

                        // When next expected is better, mark it as unrealized
                        if (Math.Abs(expectedTime - nextExpectedTime) <= minPeriod)
                        {
                            expectedChannel[k].Status = 2;
                            AddEmptyRealized(realizedChannel, k, chrdsid);
                            m++;
                            betterPairFound = true;
                        }
                        
                    }

                    // No better pair, pair current
                    if (!betterPairFound)
                    {
                        await PairMediaPlans(realizedChannel[k], expectedChannel[k]);
                    }
                }
                // No match, add empty rows
                else
                {
                    // Add empty expected row
                    if (expectedTime > realizedTime)
                    {
                        await PairMediaPlans(realizedChannel[k], null);
                        AddEmptyExpected(expectedChannel, k, chid);
                        n++;
                    }
                    // Add empty realized row
                    else
                    {
                        expectedChannel[k].Status = 2;
                        AddEmptyRealized(realizedChannel, k, chrdsid);
                        m++;
                    }
                }
                               
                k++;
            }
            // All realized used, fill the rest with empty fields
            while (k < n)
            {
                expectedChannel[k].Status = -1;
                AddEmptyRealized(realizedChannel, k, chrdsid);
                k++;
            }
            // All expected used, fill the rest with empty fields
            while (k < m)
            {
                await PairMediaPlans(realizedChannel[k], null);
                // If nothing is initialized , no need to add empty expected one by one
                if (_allMediaPlans.Count != 0)
                    AddEmptyExpected(expectedChannel, k, chid);
                k++;
            }
        }*/

        private async void ValidationStack_UpdatedMediaPlanRealized(object? sender, UpdateMediaPlanRealizedEventArgs e)
        {
            var mpRealized = e.MediaPlanRealized;
            if (e.CoefsUpdated)
                CoefsUpdated(mpRealized);

            await _mediaPlanRealizedController.UpdateMediaPlanRealized(mpRealized);
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

            
            //var mediaPlanRealizes = _mediaPlanRealized.Where(mpr => TimeFormat.YMDStringToDateOnly(mpr.date) == date).ToList();
            // _forecastData.ChrdsidChidDict.ContainsKey(mpr.chid.Value) this is 
            // consequence of not deleting from database, so it has data in base, but not loaded in dictionary
            // FIX THIS
            var mediaPlanRealizes = _mediaPlanRealized.Where(mpr => TimeFormat.YMDStringToDateOnly(mpr.date) == date &&
                            _forecastData.ChrdsidChidDict.ContainsKey(mpr.chid.Value) ? 
                            _forecastData.ChrdsidChidDict[mpr.chid.Value] == channel.chid : false).ToList();

            mediaPlanRealizes.ForEach(mpr => mpr.Channel = _forecastData.ChrdsChannelDict[mpr.chid.Value]);
            // Add spot name in realized
            mediaPlanRealizes.ForEach(mpr => mpr.spotname = mpr.spotnum.HasValue ? _spotnumNameDict[mpr.spotnum.Value] : "");

            mediaPlanRealizes = mediaPlanRealizes.OrderBy(mpr => mpr.stime.Value).ToList();
            return mediaPlanRealizes.ToList();
        }

        public void FillChannels()
        {           
            _channels = _forecastData.Channels;
            int i = 0;
            foreach (var channel in _forecastData.Channels)
            {
                _chidOrder[channel.chid] = i++;
            }
            lbChannels.SelectedItems.Clear();
            lbChannels.ItemsSource = null;
            lbChannels.ItemsSource = _channels;

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
            var mpRealized = (await _mediaPlanRealizedController.GetAllMediaPlansRealizedByCmpid(campaign.cmpid)).ToList();

            foreach (var mpr in mpRealized.Where(mpR => mpR.status == null))
            {
                var coefs = await _clientRealizedCoefsController.GetRealizedCoefs(_campaign.clid, mpr.emsnum.Value);
                if (coefs != null)
                {
                    mpr.Progcoef = coefs.progcoef;
                }
                CalculateCoefs(mpr);
                await _mediaPlanRealizedController.UpdateMediaPlanRealized(mpr);
            }

            _mediaPlanRealized.ReplaceRange(mpRealized);

            uniqueSpotNums = mpRealized.DistinctBy(mpr => mpr.spotnum).Where(mpr => mpr.spotnum.HasValue).Select(mpr => mpr.spotnum!.Value);
        }

        private async Task FillSpotNameDict(CampaignDTO campaign)
        {
            _spotnumNameDict.Clear();

            var tuples = await _mediaPlanRealizedController.GetAllSpotNumSpotNamePairs(campaign.cmpid);

            foreach (var tuple in tuples)
            {
                _spotnumNameDict[tuple.Item1] = tuple.Item2.Trim();
            }
        }

        private async Task CheckRealizedPrice()
        {
            foreach (var mediaPlanRealized in _mediaPlanRealized)
            {
                if (mediaPlanRealized.price == null)
                {
                    CalculateCoefs(mediaPlanRealized);
                }
            }
        }

        private void CalculateCoefs(MediaPlanRealized mediaPlanRealized)
        {
            if (!_forecastData.ChrdsidChidDict.ContainsKey(mediaPlanRealized.chid.Value))
                return;

            var chid = _forecastData.ChrdsidChidDict[mediaPlanRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];
 
            _mpConverter.CalculateRealizedCoefs(mediaPlanRealized, pricelist);
         
        }

        private void CoefsUpdated(MediaPlanRealized mediaPlanRealized)
        {
            var chid = _forecastData.ChrdsidChidDict[mediaPlanRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];

            _mpConverter.CoefsUpdated(mediaPlanRealized, pricelist);
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

        private void lbChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedChannels = new List<ChannelDTO>();
            foreach (ChannelDTO channel in lbChannels.SelectedItems)
            {
                selectedChannels.Add(channel);
            }
            validationStack.SelectedChannelsChanged(selectedChannels);
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
                MessageBox.Show("Error while searching for new realizations" + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

 
        }

        private async Task<bool> CheckNewDateData(DateOnly date)
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
                         cmpbrnd.brbrand, TimeFormat.DateOnlyToYMDString(date), TimeFormat.DateOnlyToYMDString(date));
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching for new realizations " + ex.Message, "Error",
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
                await FillSpotNameDict(_campaign);
                InitializeRealizedDict();
                await AlignExpectedRealized();
                await validationStack.Initialize(_forecastData.Campaign, hideExpected);
            }
            SetContentPage?.Invoke(this, null);
        }

        private void BindValidationStack()
        {
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
            validationStack._completedValidationController = _completedValidationController;
            validationStack._dgConfigController = _dgConfigController;

            validationStack.ProgcoefChangedMediaPlanRealized += ValidationStack_ProgcoefChangedMediaPlanRealized;

        }

        private async void ValidationStack_ProgcoefChangedMediaPlanRealized(object? sender, UpdateMediaPlanRealizedEventArgs e)
        {
            // add or update row in dataTable
            var mediaPlanRealized = e.MediaPlanRealized;
            if (mediaPlanRealized == null)
                return;
            var coefs = new ClientRealizedCoefs(_campaign.clid, mediaPlanRealized.emsnum.Value, mediaPlanRealized.Progcoef.Value);
            await _clientRealizedCoefsController.CreateOrUpdateRealizedCoefs(coefs);

            // update all unverified rows in current campaign
            foreach (var date in _dateRealizedDict.Keys)
            {
                foreach (var mpRealized in _dateRealizedDict[date].Where(mpr => mpr.Status != null &&
                                                                   !(bool)mpr.Accept &&
                                                                   mpr.emsnum == mediaPlanRealized.emsnum))
                {
                    mpRealized.Progcoef = coefs.progcoef;
                    _mpConverter.CalculateRealizedPrice(mpRealized);
                    mpRealized.Status = 5;
                    await _mediaPlanRealizedController.UpdateMediaPlanRealized(mpRealized);
                }
            }
            // update all unverified rows in database from that client, except for this campaign (it's already done)
            var activeClientCampaigns = (await _campaignController.GetCampaignsByClientId(_campaign.clid))
                                        .Where(cmp => cmp.active && cmp.cmpid != _campaign.cmpid);

            foreach (var campaign in activeClientCampaigns)
            {
                var mpRealizedList = await _mediaPlanRealizedController.GetAllMediaPlansRealizedByCmpidAndEmsnum(campaign.cmpid, mediaPlanRealized.emsnum.Value);
                foreach (var mpRealized in mpRealizedList)
                {
                    decimal oldProgcoef = mpRealized.progcoef;
                    mpRealized.progcoef = coefs.progcoef;
                    mpRealized.status = 5;
                    decimal quotient = coefs.progcoef / oldProgcoef;
                    mpRealized.price *= quotient;
                    await _mediaPlanRealizedController.UpdateMediaPlanRealized(new UpdateMediaPlanRealizedDTO(mpRealized));
                }
            }
        }

        private void BindPrintValidation()
        {
            _factoryPrintValidation.DgExpectedMask = validationStack.DgExpectedMask;
            _factoryPrintValidation.DgRealizedMask = validationStack.DgRealizedMask;
            _factoryPrintValidation.ValidationDays = validationStack.ValidationDays;
            _factoryPrintValidation._dateExpectedDict = validationStack._dateExpectedDict;
            _factoryPrintValidation._dateRealizedDict = validationStack._dateRealizedDict;
            _factoryPrintValidation._dates = validationStack._dates;
            _factoryPrintValidation.ValidationDaysDict = validationStack.ValidationDaysDict;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            _factoryPrintValidation.ShowDialog();
        }

        public void CloseValidation()
        {
            validationStack.ClearStackPanel();
            validationStack.UpdatedMediaPlanRealized -= ValidationStack_UpdatedMediaPlanRealized;
            validationStack.CheckNewDataDay -= ValidationStack_CheckNewDataDay;
            // Close print window
            _factoryPrintValidation.shouldClose = true;
            _factoryPrintValidation.Close();
        }

        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            await Initialize(_campaign, _allMediaPlans);
        }

        private async void btnPairSpots_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryPairSpots.Create();
            await factory.Initialize(_campaign, _forecastData.Spots, uniqueSpotNums);
            factory.ShowDialog();

        }
    }
}
