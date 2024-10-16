using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace CampaignEditor.UserControls.ForecastGrids
{
    public class GridsDataManipulation
    {
        enum Data
        {
            Expected,
            Realized,
            ExpectedAndRealized
        }

        Data showData = Data.Expected;

        CampaignDTO _campaign;
        int _version = 1;


        // for duration of campaign
        DateTime startDate;
        DateTime endDate;
        public DateOnly separationDate;
        public DateOnly SeparationDate { get; set; } // for showing expectedAndRealized

        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();

        private List<ChannelDTO> _selectedChannels = new List<ChannelDTO>();
        private List<ChannelDTO> _visibleChannels = new List<ChannelDTO>();

        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        public ObservableRangeCollection<MediaPlanTuple> _visibleTuples = new ObservableRangeCollection<MediaPlanTuple>();
        public ObservableRangeCollection<MediaPlanRealized> _mpRealized;
        private MediaPlanForecastData _forecastData;

        private Dictionary<ChannelDTO, Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>> _data;
        private ChannelDTO dummyChannel = new ChannelDTO(-1, "Total", true, 0, "", 0, 0); // Dummy channel for Total Column

        public SpotDaysGoalsGrid sdgGrid;
        public SpotWeekGoalsGrid swgGrid;
        public ChannelsGoalsGrid cgGrid;
        public SpotGoalsGrid sgGrid;

        public void Initialize(CampaignDTO campaign, MediaPlanForecastData forecastData, int version)
        {
            _campaign = campaign;
            _version = version;
            _forecastData = forecastData;

            _spots.Clear();
            _channels.Clear();


            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);


            _spots.AddRange(_forecastData.Spots);

            _channels.AddRange(_forecastData.Channels);
            _channels.Add(dummyChannel);

            sdgGrid._channels = _channels;
            swgGrid._channels = _channels;

            sdgGrid._spots = _spots;
            swgGrid._spots = _spots;

            sdgGrid._visibleTuples = _visibleTuples;
            swgGrid._visibleTuples = _visibleTuples;

            sdgGrid._visibleChannels = _visibleChannels;
            swgGrid._visibleChannels = _visibleChannels;

            sdgGrid._selectedChannels = _selectedChannels;
            swgGrid._selectedChannels = _selectedChannels;

            cgGrid._selectedChannels = _selectedChannels;
            TransformData();

            ConstructGrids();
            AssignDataValues();
        }

        public void TransformData()
        {
            InitializeData();
        }

        // Making appropriate _data structure, with all zero values for SpotGoals
        private void InitializeData()
        {
            _data = new Dictionary<ChannelDTO, Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>>();
            int daysNum = (int)(endDate - startDate).Days + 1 + 1; // Last +1 is because we want to have Total footer

            foreach (var channel in _channels)
            {
                var dateGoalsDict = new Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>();

                for (int i = 0; i < daysNum; i++)
                {
                    DateTime currentDate = startDate.AddDays(i);
                    var spotSpotGoalsDict = new Dictionary<SpotDTO, SpotGoals>();

                    foreach (var spot in _spots)
                    {
                        var spotGoals = new SpotGoals();
                        spotSpotGoalsDict.Add(spot, spotGoals);
                    }

                    dateGoalsDict.Add(DateOnly.FromDateTime(currentDate), spotSpotGoalsDict);

                }

                _data.Add(channel, dateGoalsDict);
            }
        }

        private void ConstructGrids()
        {
            sdgGrid.ConstructGrid(startDate, endDate);
            swgGrid.ContructGrid(startDate, endDate);
            sgGrid.ConstructGrid(startDate, endDate);
            cgGrid.ConstructGrid(startDate, endDate);
        }

        private void AssignDataValues()
        {
            sdgGrid.AssignDataValues(_data);
            swgGrid.AssignDataValues(_data);
            cgGrid.AssignDataValues(_data);

            sdgGrid.BindDataValues();
            swgGrid.BindDataValues();
            cgGrid.BindDataValues();
        }

        public void UpdateChannelOrder(List<ChannelDTO> channels)
        {
            sdgGrid.UpdateUgChannelOrder(channels, true);
            swgGrid.UpdateUgChannelOrder(channels, true);
            sgGrid.UpdateUgChannelOrder(channels, true);
        }

        public void AddRealizations(ObservableRangeCollection<MediaPlanRealized> mpRealized)
        {
            _mpRealized = mpRealized;
        }

        public void SelectedChannelsChanged(IEnumerable<ChannelDTO> selectedChannels)
        {
            // Channel is unselected
            if (selectedChannels.Count() < _selectedChannels.Count())
            {

                for (int i = 0; i < _selectedChannels.Count(); i++)
                {
                    var channel = _selectedChannels[i];
                    if (!selectedChannels.Contains(channel))
                    {
                        HideChannel(channel);
                        _selectedChannels.Remove(channel);
                        _visibleChannels.Remove(channel);

                        if (_selectedChannels.Count < 2 && _visibleChannels.Contains(dummyChannel))
                            _visibleChannels.Remove(dummyChannel);

                        i--;
                    }
                }

            }
            // Channel is selected
            else if (selectedChannels.Count() > _selectedChannels.Count())
            {
                foreach (var channel in selectedChannels)
                {
                    if (!_selectedChannels.Contains(channel))
                    {
                        ShowChannel(channel);
                        _selectedChannels.Add(channel);

                        int index = _visibleChannels.Count;
                        if (_visibleChannels.Contains(dummyChannel))
                        {
                            index -= 1;
                        }
                        _visibleChannels.Insert(index, channel);

                        if (_selectedChannels.Count >= 2 && !_visibleChannels.Contains(dummyChannel))
                            _visibleChannels.Add(dummyChannel);
                    }
                }

            }

            RecalculateTotalColumnSpotGoals();
            AssignDataValues();
            cgGrid.SelectedChannelsChanged(_selectedChannels);

            if (_selectedChannels.Count >= 2)
            {
                ShowChannel(dummyChannel);
            }
            else
            {
                HideChannel(dummyChannel);
            }

            // for ugGoals, just show how many children needs to be shown
            /*for (int i = 0; i < _visibleChannels.Count * 3; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Visible;
            }
            for (int i = _visibleChannels.Count * 3; i < _channels.Count * 3; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Collapsed;

            }*/
        }

        private void ShowChannel(ChannelDTO channel)
        {
            // Showing Channel and Goals headers


            sdgGrid.ShowChannel(channel);
            swgGrid.ShowChannel(channel);
            sgGrid.ShowChannel(channel);
        }

        private void HideChannel(ChannelDTO channel)
        {


            sdgGrid.HideChannel(channel);
            swgGrid.HideChannel(channel);
            sgGrid.HideChannel(channel);

        }

        #region Recalculation
        public void RecalculateGoals()
        {
            foreach (var channel in _data.Keys)
            {
                RecalculateGoals(channel);
                RecalculateTotalFooterSpotGoals(channel);
            }
            //RecalculateTotalColumnSpotGoals();

            //AssignDataValues();
        }
        public void RecalculateGoals(ChannelDTO channel)
        {
            foreach (var date in _data[channel].Keys)
            {
                RecalculateGoals(channel, date);
            }
        }

        public void RecalculateGoals(ChannelDTO channel, DateOnly date)
        {
            foreach (var spotSpotGoalsDict in _data[channel][date].Keys)
            {
                if (date == DateOnly.FromDateTime(endDate.AddDays(1)))
                {
                    // Last date is for total, so it shouldn't be calculated
                    continue;
                }
                else
                {
                    RecalculateGoals(channel, date, spotSpotGoalsDict);
                }
            }

        }

        public void RecalculateGoals(ChannelDTO channel, DateOnly date, SpotDTO spot, bool updateTotal = true)
        {
            var spotGoals = _data[channel][date][spot];

            SpotGoals goals = new SpotGoals();
            if (showData == Data.Expected)
                goals = CalculateSpotGoalsExpected(channel.chid, spot, date);
            else if (showData == Data.Realized)
            {
                int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == channel.chid).Key;
                if (!chrdsid.HasValue)
                    return;
                goals = CalculateSpotGoalsRealized(chrdsid.Value, spot, date);
            }
            else if (showData == Data.ExpectedAndRealized)
            {
                if (date <= SeparationDate)
                {
                    int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == channel.chid).Key;
                    if (!chrdsid.HasValue)
                        return;

                    goals = CalculateSpotGoalsRealized(chrdsid.Value, spot, date);
                }
                else
                {

                    goals = CalculateSpotGoalsExpected(channel.chid, spot, date);

                }

            }


            spotGoals.Insertations = goals.Insertations;
            spotGoals.Grp = goals.Grp;
            spotGoals.Budget = goals.Budget;

            if (updateTotal)
            {
                RecalculateTotalColumnSpotGoals(date, spot);
                RecalculateTotalFooterSpotGoals(channel, spot);
                RecalculateTotalFooterSpotGoals(dummyChannel, spot);
            }

            AssignDataValues();
        }

        private SpotGoals CalculateSpotGoalsExpected(int chid, SpotDTO spot, DateOnly date)
        {
            var dateTime = date.ToDateTime(TimeOnly.Parse("00:01 AM"));
            int dateIndex = (int)(dateTime - startDate).Days;

            var channelMpTuples = _visibleTuples.Where(mpt => mpt.MediaPlan.chid == chid);
            SpotGoals spotGoals = new SpotGoals();

            foreach (var mpTuple in channelMpTuples)
            {
                var term = mpTuple.Terms[dateIndex];
                if (term == null || term.Spotcode == null)
                    continue;
                foreach (char spotcode in term.Spotcode.Trim())
                {
                    if (spotcode == spot.spotcode[0])
                    {
                        var mediaPlan = mpTuple.MediaPlan;
                        spotGoals.Insertations += 1;
                        spotGoals.Grp += mediaPlan.Amrp1;
                        spotGoals.Grp2 += mediaPlan.Amrp2;
                        spotGoals.Grp3 += mediaPlan.Amrp3;
                        // Need to fix this
                        if (mediaPlan.Length == 0)
                            spotGoals.Budget += 0;
                        else
                            spotGoals.Budget += (mediaPlan.Price / mediaPlan.Length) * spot.spotlength;
                    }
                }
            }

            return spotGoals;
        }

        private SpotGoals CalculateSpotGoalsRealized(int chrdsid, SpotDTO spot, DateOnly date)
        {
            SpotGoals spotGoals = new SpotGoals();

            var spotnums = _forecastData.SpotPairs.Where(sp => sp.spotcode[0] == spot.spotcode[0]).Select(sp => sp.spotnum);

            string dateString = TimeFormat.DateOnlyToYMDString(date);
            var mediaPlansRealized = _mpRealized.Where(
                mpr => mpr.chid == chrdsid &&
                spotnums.Any(sn => sn == mpr.spotnum) &&
                String.Compare(mpr.Date, dateString) == 0 &&
                mpr.Status != null && mpr.Status != 5);

            foreach (var mpRealized in mediaPlansRealized)
            {
                spotGoals.Insertations += 1;
                spotGoals.Grp += mpRealized.Amrp1 ?? 0.0M;
                spotGoals.Grp2 += mpRealized.Amrp2 ?? 0.0M;
                spotGoals.Grp3 += mpRealized.Amrp3 ?? 0.0M;
                spotGoals.Budget += mpRealized.price ?? 0.0M;
            }

            return spotGoals;
        }

        private void RecalculateTotalFooterSpotGoals(ChannelDTO channel)
        {

            foreach (var spot in _spots)
            {
                RecalculateTotalFooterSpotGoals(channel, spot);
            }
        }

        private void RecalculateTotalFooterSpotGoals(ChannelDTO channel, SpotDTO spot)
        {
            var date = DateOnly.FromDateTime(endDate.AddDays(1));
            var totalSpotGoals = _data[channel][date][spot];

            var channelSpotGoals = _data[channel].SelectMany(dateDict => dateDict.Value
                                                .Where(spotSpotGoalsDict => spotSpotGoalsDict.Key.spotcode.Trim() == spot.spotcode.Trim())
                                                .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));

            int ins = 0;
            decimal grp = 0;
            decimal budget = 0;
            foreach (var spotGoal in channelSpotGoals)
            {
                ins += spotGoal.Insertations;
                grp += spotGoal.Grp;
                budget += spotGoal.Budget;
            }

            totalSpotGoals.Insertations = ins - totalSpotGoals.Insertations;
            totalSpotGoals.Grp = grp - totalSpotGoals.Grp;
            totalSpotGoals.Budget = budget - totalSpotGoals.Budget;

        }

        private void RecalculateTotalColumnSpotGoals()
        {
            foreach (var date in _data[dummyChannel].Keys)
            {
                foreach (var spot in _spots)
                {
                    RecalculateTotalColumnSpotGoals(date, spot);
                }
            }

        }

        private void RecalculateTotalColumnSpotGoals(DateOnly date, SpotDTO spot)
        {
            var totalSpotGoals = _data[dummyChannel][date][spot];

            int ins = 0;
            decimal grp = 0;
            decimal budget = 0;

            var channelSpotGoals = _data.Where(dict => _selectedChannels.Contains(dict.Key)) //  all selected channels
                                        .SelectMany(dict => dict.Value) // use only values
                                        .Where(dateDict => dateDict.Key == date) // for given date
                                        .SelectMany(dateDict => dateDict.Value
                                        .Where(spotSpotGoalsDict => spotSpotGoalsDict.Key.spotcode.Trim() == spot.spotcode.Trim())
                                        .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));

            foreach (var spotGoal in channelSpotGoals)
            {
                ins += spotGoal.Insertations;
                grp += spotGoal.Grp;
                budget += spotGoal.Budget;
            }


            totalSpotGoals.Insertations = ins;
            totalSpotGoals.Grp = grp;
            totalSpotGoals.Budget = budget;
        }

        #endregion       

        #region Selection changed

        public void VisibleTuplesChanged(IEnumerable<MediaPlanTuple> visibleMpTuples)
        {
            _visibleTuples.ReplaceRange(visibleMpTuples);
            RecalculateGoals();
        }        
       

        private void UpdateListsOrder(IEnumerable<ChannelDTO> channels, bool addDummyChannel = false)
        {
            _channels = channels.ToList();
            if (addDummyChannel)
                _channels.Add(dummyChannel);
            // visible channels aren't sorted, but channels are
            List<ChannelDTO> visibleChannels = new List<ChannelDTO>();
            foreach (var channel in _channels)
            {
                foreach (var visibleChannel in _visibleChannels)
                {
                    if (channel.chid == visibleChannel.chid)
                    {
                        visibleChannels.Add(channel);
                        break;
                    }
                }
            }
            _visibleChannels = visibleChannels.ToList();
        }

        

        #endregion

        public void ChangeDataForShowing(string dataName)
        {
            Data data;
            switch (dataName)
            {
                case "expected": data = Data.Expected; break;
                case "realized": data = Data.Realized; break;
                case "expectedrealized": data = Data.ExpectedAndRealized; break;
                default: data = Data.Expected; break;
            }

            if (data == showData)
                return;

            showData = data;

            RecalculateGoals();
            AssignDataValues();
        }

    }
}
