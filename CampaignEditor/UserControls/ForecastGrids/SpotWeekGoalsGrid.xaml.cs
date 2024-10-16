﻿using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Windows.Input;
using Database.Entities;
using System.Linq;
using System.Windows.Data;
using CampaignEditor.Helpers;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// This grid is very similar to SpotGoalsGrid, so I'll pass it some SpotGoalsGrid, and make
    /// this grid based on that, on that way, there's no need to calculate same things more than once
    /// </summary>
    public partial class SpotWeekGoalsGrid : UserControl
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
        DateOnly SeparationDate { get; set; }
        int firstWeekNum;
        int lastWeekNum;
        int separationWeekNum;

        /*List<SpotDTO> _spots = new List<SpotDTO>();
          List<ChannelDTO> _channels = new List<ChannelDTO>();*/

        public List<SpotDTO> _spots;
        public List<ChannelDTO> _channels;

        public List<ChannelDTO> _selectedChannels;
        public List<ChannelDTO> _visibleChannels;
        public ObservableRangeCollection<MediaPlanRealized> _mpRealized;
        public MediaPlanForecastData _forecastData;

        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        public ObservableRangeCollection<MediaPlanTuple> _visibleTuples = new ObservableRangeCollection<MediaPlanTuple>();
        private Dictionary<Char, int> _spotLengths = new Dictionary<char, int>();

        public Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>> _data;
        private Dictionary<ChannelDTO, DataGrid> _channelGrids = new Dictionary<ChannelDTO, DataGrid>();
        private Dictionary<ChannelDTO, System.Windows.Controls.Border> channelBorderDict = new Dictionary<ChannelDTO, System.Windows.Controls.Border>();

        private ChannelDTO dummyChannel = new ChannelDTO(-1, "Total", true, 0, "", 0, 0); // Dummy channel for Total Column

        public SpotGoalsGrid spotGoalsGrid { get; set; }

        public SpotWeekGoalsGrid()
        {
            InitializeComponent();
        }

        public void Initialize(CampaignDTO campaign, IEnumerable<ChannelDTO> channels, IEnumerable<SpotDTO> spots, int version)
        {
            _campaign = campaign;
            _version = version;

            _spots.Clear();
            _spotLengths.Clear();
            _channels.Clear();
            _channelGrids.Clear();
            channelBorderDict.Clear();
            ugChannels.Children.Clear();
            ugGoals.Children.Clear();
            ugWeeks.Children.Clear();
            ugSpots.Children.Clear();
            ugGrid.Children.Clear();


            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            firstWeekNum = GetWeekOfYear(startDate);
            lastWeekNum = GetWeekOfYear(endDate);

            _spots.AddRange(spots);
            foreach (var spot in spots)
            {
                try
                {
                    _spotLengths.Add(spot.spotcode[0], spot.spotlength);
                }
                catch
                {
                }
            }


            _channels.AddRange(channels);
            _channels.Add(dummyChannel);

            //TransformData();
            SubscribeDataToSpotGoalsGrid(spotGoalsGrid);
            CreateOutboundHeaders();
            SetWidth();
        }

        public void ContructGrid(DateTime startDate, DateTime endDate)
        {

            _channelGrids.Clear();
            channelBorderDict.Clear();
            ugChannels.Children.Clear();
            ugGoals.Children.Clear();
            ugWeeks.Children.Clear();
            ugSpots.Children.Clear();
            ugGrid.Children.Clear();


            this.startDate = startDate;
            this.endDate = endDate;

            firstWeekNum = GetWeekOfYear(startDate);
            lastWeekNum = GetWeekOfYear(endDate);

            SubscribeDataToSpotGoalsGrid(spotGoalsGrid);
            CreateOutboundHeaders();
            SetWidth();
        }

        public void AssignDataValues(Dictionary<ChannelDTO, Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>> data)
        {
            var transformedData = TransformData(data);
            _data = transformedData;

            spotGoalsGrid._data = _data;
        }

        private Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>> TransformData(
    Dictionary<ChannelDTO, Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>> data)
        {
            // Create the new dictionary to store the transformed data
            var transformedData = new Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>>();

            // Iterate through each channel
            foreach (var channelEntry in data)
            {
                var channel = channelEntry.Key;
                var dateData = channelEntry.Value;

                // Create a new dictionary for each channel with week number as the key
                var weekData = new Dictionary<int, Dictionary<SpotDTO, SpotGoals>>();

                // Iterate through each date in the original data
                foreach (var dateEntry in dateData)
                {
                    var date = dateEntry.Key;
                    var spotData = dateEntry.Value;

                    // Get the week number from the date
                    int weekNumber = GetWeekOfYear(date);

                    if (date == DateOnly.FromDateTime(endDate.AddDays(1)))
                        weekNumber = lastWeekNum + 1; // For total column

                    // Ensure that the week number dictionary exists for the current channel
                    if (!weekData.ContainsKey(weekNumber))
                    {
                        weekData[weekNumber] = new Dictionary<SpotDTO, SpotGoals>();
                    }

                    // Iterate through each spot in the spot data
                    foreach (var spotEntry in spotData)
                    {
                        var spot = spotEntry.Key;
                        var spotGoals = spotEntry.Value;

                        // Sum the SpotGoals for the current spot
                        if (weekData[weekNumber].ContainsKey(spot))
                        {
                            // Accumulate the SpotGoals if it already exists for this spot
                            weekData[weekNumber][spot] += spotGoals;
                        }
                        else
                        {
                            // Add a new SpotGoals entry if it doesn't exist
                            weekData[weekNumber][spot] = spotGoals;
                        }
                    }
                }

                // Add the weekData for the current channel to the transformed data
                transformedData[channel] = weekData;
            }

            return transformedData;
        }

        public void SetSeparationDate(DateOnly separationDate)
        {
            SeparationDate = separationDate;
            separationWeekNum = GetWeekOfYear(SeparationDate);

        }
        private void SubscribeDataToSpotGoalsGrid(SpotGoalsGrid sgGrid)
        {
            sgGrid.firstWeekNum = firstWeekNum;
            sgGrid.lastWeekNum = lastWeekNum;
            sgGrid.startDate = startDate;
            sgGrid.endDate = endDate;
            sgGrid._spots = _spots;
            sgGrid._channels = _channels;
            sgGrid._selectedChannels = _selectedChannels;
            sgGrid._visibleChannels = _visibleChannels;
            sgGrid._data = _data;
            sgGrid.dummyChannel = dummyChannel;
            //sgGrid.Initialize();
        }

        /*public void TransformData()
        {
            InitializeData();
            //RecalculateGoals();
        }

        // Making appropriate _data structure, with all zero values for SpotGoals
        private void InitializeData()
        {
            _data = new Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>>();
            int weeksNum = TimeFormat.GetWeeksBetween(startDate, endDate) + 1;// last +1 is for Total header

            int weeksInYear = TimeFormat.GetWeeksInYear(startDate.Year);
            foreach (var channel in _channels)
            {
                var weekGoalsDict = new Dictionary<int, Dictionary<SpotDTO, SpotGoals>>();

                for (int i = 0; i < weeksNum; i++)
                {
                    int currentWeek = firstWeekNum + i;

                    if (currentWeek > weeksInYear)
                    {
                        currentWeek = currentWeek % (weeksInYear + 1) + 1; // so there are no week 0
                    }

                    var spotSpotGoalsDict = new Dictionary<SpotDTO, SpotGoals>();

                    foreach (var spot in _spots)
                    {
                        var spotGoals = new SpotGoals();
                        spotSpotGoalsDict.Add(spot, spotGoals);
                    }

                    weekGoalsDict.Add(currentWeek, spotSpotGoalsDict);

                }

                _data.Add(channel, weekGoalsDict);
            }
        }

        #region Recalculation
        public void RecalculateGoals()
        {
            foreach (var channel in _data.Keys)
            {
                RecalculateGoals(channel);
                RecalculateTotalFooterSpotGoals(channel);
            }
            RecalculateTotalColumnSpotGoals();
        }
        public void RecalculateGoals(ChannelDTO channel)
        {
            foreach (int weekNum in _data[channel].Keys)
            {
                RecalculateGoals(channel, weekNum);
            }
        }
        public void RecalculateGoals(ChannelDTO channel, DateOnly date)
        {
            int weekNum = GetWeekOfYear(date);
            RecalculateGoals(channel, weekNum);
        }
        public void RecalculateGoals(ChannelDTO channel, int weekNum)
        {
            foreach (var spotSpotGoalsDict in _data[channel][weekNum].Keys)
            {
                if (weekNum == lastWeekNum + 1)
                {
                    // Last date is for total, so it shouldn't be calculated
                    continue;
                }
                else
                {
                    RecalculateGoals(channel, weekNum, spotSpotGoalsDict);
                }
            }

        }

        public void RecalculateGoals(ChannelDTO channel, DateOnly date, SpotDTO spot, bool updateTotal = false)
        {
            int weekNum = GetWeekOfYear(date);
            RecalculateGoals(channel, weekNum, spot, updateTotal);
        }*/
        /*public void RecalculateGoals(ChannelDTO channel, int weekNum, SpotDTO spot, bool updateTotal = false)
        {
            var spotGoals = _data[channel][weekNum][spot];


            SpotGoals goals = new SpotGoals();
            if (showData == Data.Expected)
                goals = CalculateSpotGoalsExpected(channel.chid, spot, weekNum);
            else if (showData == Data.Realized)
            {
                int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == channel.chid).Key;
                if (!chrdsid.HasValue)
                    return;
                goals = CalculateSpotGoalsRealized(chrdsid.Value, spot, weekNum);
            }
            else if (showData == Data.ExpectedAndRealized)
            {
                if (weekNum < separationWeekNum)
                {
                    int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == channel.chid).Key;
                    if (!chrdsid.HasValue)
                        return;
                    goals = CalculateSpotGoalsRealized(chrdsid.Value, spot, weekNum);
                }
                else if (weekNum > separationWeekNum)
                {
                    goals = CalculateSpotGoalsExpected(channel.chid, spot, weekNum);
                }
                else
                {
                    int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == channel.chid).Key;
                    if (!chrdsid.HasValue)
                        return;

                    var expectedGoals = CalculateSpotGoalsExpected(channel.chid, spot, weekNum, true);
                    var realizedGoals = CalculateSpotGoalsRealized(chrdsid.Value, spot, weekNum, true);
                    goals.Insertations = expectedGoals.Insertations + realizedGoals.Insertations;
                    goals.Grp = expectedGoals.Grp + realizedGoals.Grp;
                    goals.Budget = expectedGoals.Budget + realizedGoals.Budget;
                }
            }

            spotGoals.Insertations = goals.Insertations;
            spotGoals.Grp = goals.Grp;
            spotGoals.Budget = goals.Budget;

            if (updateTotal)
            {
                RecalculateTotalColumnSpotGoals(weekNum, spot);
                RecalculateTotalFooterSpotGoals(channel, spot);
                RecalculateTotalFooterSpotGoals(dummyChannel, spot);
            }
        }

        private SpotGoals CalculateSpotGoalsExpected(int chid, SpotDTO spot, int weekNum, bool checkSeparationDate = false)
        {
            List<int> weekIndexes = GetWeekIndexes(weekNum, checkSeparationDate);

            var channelMpTuples = _visibleTuples.Where(mpt => mpt.MediaPlan.chid == chid);

            SpotGoals spotGoals = new SpotGoals();

            foreach (var mpTuple in channelMpTuples)
            {
                foreach (int index in weekIndexes)
                {
                    var term = mpTuple.Terms[index];
                    if (term == null || term.Spotcode == null)
                        continue;
                    foreach (char spotcode in term.Spotcode.Trim())
                    {
                        if (spotcode == spot.spotcode[0])
                        {
                            var mediaPlan = mpTuple.MediaPlan;
                            spotGoals.Insertations += 1;
                            spotGoals.Grp += mediaPlan.Amrp1;
                            // Need to fix this
                            if (mediaPlan.Length == 0)
                                spotGoals.Budget += 0;
                            else
                                spotGoals.Budget += (mediaPlan.Price / mediaPlan.Length) * spot.spotlength;
                        }
                    }
                }
            }
            
            return spotGoals;
        }

        private SpotGoals CalculateSpotGoalsRealized(int chrdsid, SpotDTO spot, int weekNum, bool checkSeparationDate = false)
        {
            List<DateOnly> weekDates = GetWeekDates(weekNum, checkSeparationDate);
            SpotGoals spotGoals = new SpotGoals();
            var spotnums = _forecastData.SpotPairs.Where(sp => sp.spotcode[0] == spot.spotcode[0]).Select(sp => sp.spotnum);

            var mediaPlansRealized = _mpRealized.Where(
                mpr => mpr.chid == chrdsid &&
                spotnums.Any(sn => sn == mpr.spotnum) &&
                weekDates.Contains(TimeFormat.YMDStringToDateOnly(mpr.Date)) &&
                mpr.Status != null && mpr.Status != 5);

            foreach (var mediaPlanRealized in mediaPlansRealized)
            {
                spotGoals.Insertations += 1;
                spotGoals.Grp += mediaPlanRealized.Amrp1 ?? 0.0M;
                spotGoals.Budget += mediaPlanRealized.price ?? 0.0M;
            }

            return spotGoals;
        }*/

        /*private List<int> GetWeekIndexes(int weekNum, bool checkSeparationDate = false)
        {
            DateOnly firstDate = DateOnly.FromDateTime(startDate);
            DateOnly lastDate = DateOnly.FromDateTime(endDate);
            if (checkSeparationDate)
            {
                lastDate = SeparationDate;
            }

            List<int> indexes = new List<int>();
            int index = 0;
            for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
            {
                int dateWeekNum = GetWeekOfYear(date);
                if (dateWeekNum == weekNum)
                {
                    indexes.Add(index);
                }
                index++;
            }
            return indexes;
        }

        private List<DateOnly> GetWeekDates(int weekNum, bool checkSeparationDate = false)
        {
            DateOnly firstDate = DateOnly.FromDateTime(startDate);
            DateOnly lastDate = DateOnly.FromDateTime(endDate);
            if (checkSeparationDate)
            {
                firstDate = SeparationDate;
            }

            List<DateOnly> dates = new List<DateOnly>();
            int index = 0;
            for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
            {
                int dateWeekNum = GetWeekOfYear(date);
                if (dateWeekNum == weekNum)
                {
                    dates.Add(date);
                }
                index++;
            }
            return dates;
        }*/

        /*private void RecalculateTotalFooterSpotGoals(ChannelDTO channel)
        {

            foreach (var spot in _spots)
            {
                RecalculateTotalFooterSpotGoals(channel, spot);
            }
        }

        private void RecalculateTotalFooterSpotGoals(ChannelDTO channel, SpotDTO spot)
        {
            var weekNum = lastWeekNum + 1;
            int weeksInYear = TimeFormat.GetWeeksInYear(startDate.Year);
            if (weekNum > weeksInYear)
            {
                weekNum = weekNum % (weeksInYear + 1) + 1; // so there are no week 0
            }
            var totalSpotGoals = _data[channel][weekNum][spot];

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

        private void RecalculateTotalColumnSpotGoals(int weekNum, SpotDTO spot)
        {
            var totalSpotGoals = _data[dummyChannel][weekNum][spot];

            int ins = 0;
            decimal grp = 0;
            decimal budget = 0;

            var channelSpotGoals = _data.Where(dict => _selectedChannels.Contains(dict.Key)) //  all selected channels
                                        .SelectMany(dict => dict.Value) // use only values
                                        .Where(dateDict => dateDict.Key == weekNum) // for given date
                                        .SelectMany(dateDict => dateDict.Value
                                        .Where(spotSpotGoalsDict => spotSpotGoalsDict.Key.spotcode.Trim() == spot.spotcode.Trim())
                                        .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));

            var a = channelSpotGoals.Count();
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

        #endregion*/
        private void CreateOutboundHeaders()
        {
            ugWeeks.Children.Clear();
            ugChannels.Children.Clear();
            ugGoals.Children.Clear();
            ugSpots.Children.Clear();
            // Add headers
            // Weeks
            int weeksNum = TimeFormat.GetWeeksBetween(startDate, endDate) + 1; // last + 1 is for Total footer 
            ugWeeks.Rows = weeksNum;
            int weeksInYear = TimeFormat.GetWeeksInYear(startDate.Year);

            for (int i = 0; i < weeksNum; i++) 
            {
                int currentWeek = firstWeekNum + i;

                if (currentWeek > weeksInYear)
                {
                    currentWeek = currentWeek % (weeksInYear + 1) + 1; // so there are no week 0
                }


                System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                border.BorderBrush = System.Windows.Media.Brushes.Black;
                border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                /*if (currentWeek == firstWeekNum)
                {
                    border.BorderThickness = new Thickness(1, 3, 1, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 1, 1, 1);
                }*/
                border.BorderThickness = new Thickness(1, 1, 1, 1);

                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.FontWeight = FontWeights.Bold;
                textBlock.Text = GetWeekLabel(currentWeek);

                // Changing last Week to Total
                if (currentWeek == lastWeekNum + 1)
                {
                    textBlock.Text = "Total";
                }

                border.Child = textBlock;
               
                ugWeeks.Children.Add(border);
            }


            // Spots
            for (int i = 0; i < weeksNum; i++) // +1 because of Total row
            {

                for (int j = 0; j < _spots.Count; j++)
                {
                    System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                    border.BorderBrush = System.Windows.Media.Brushes.Black;
                    border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                    /*if (j == _spots.Count - 1)
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 2);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 1);
                    }*/
                    border.BorderThickness = new Thickness(1, 1, 1, 1);
                    TextBlock textBlock = new TextBlock();

                    textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.FontWeight = FontWeights.Bold;

                    string label = GetSpotLabel(_spots[j]);
                    textBlock.Text = label;

                    border.Child = textBlock;
                    border.MouseLeftButtonDown += SpotBorder_MouseLeftButtonDown;

                    ugSpots.Children.Add(border);
                }

            }

            ugChannels.Columns = (_channels.Count); 
            ugGoals.Columns = ugChannels.Columns * 3;
            ugGrid.Columns = ugChannels.Columns;

            foreach (var channel in _channels)
            {
                AddChannelDataColumn(channel);
            }
            UpdateUgChannelOrder(_channels);

            foreach (var channel in _channels)
            {
                HideChannel(channel);
            }

            for (int i = 0; i < _channels.Count * 3; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Collapsed;
            }

            /*foreach (var channel in _selectedChannels)
            {
                ShowChannel(channel);
            }
            if (_selectedChannels.Count > 0)
            {
                ShowChannel(dummyChannel);
            }*/

        }

        private string GetWeekLabel(int weekNum)
        {
            string label = $"Week {weekNum}";
            return label;
        }
        private string GetSpotLabel(SpotDTO spot)
        {
            string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim() + $" ({spot.spotlength})";
            return label;
        }

        private int GetWeekOfYear(DateTime date)
        {
            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.Calendar calendar = cultureInfo.Calendar;

            System.Globalization.DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;
            dtfi.FirstDayOfWeek = DayOfWeek.Monday;

            return calendar.GetWeekOfYear(date, dtfi.CalendarWeekRule, dtfi.FirstDayOfWeek);
        }
        private int GetWeekOfYear(DateOnly date)
        {
            DateTime dateTime = date.ToDateTime(TimeOnly.Parse("00:01 AM"));
            return GetWeekOfYear(dateTime);
        }

        #region ChannelDataColumn
        private void AddChannelDataColumn(ChannelDTO channel)
        {
            AddChannelHeader(channel);
            AddChannelDataGridColumn(channel);
        }

        private void AddChannelHeader(ChannelDTO channel)
        {
            System.Windows.Controls.Border border = new System.Windows.Controls.Border();
            border.BorderBrush = System.Windows.Media.Brushes.Black;
            border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
            border.BorderThickness = new Thickness(1);

            TextBlock textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = channel.chname.Trim();

            border.Child = textBlock;
            //ugChannels.Children.Add(border);
            channelBorderDict[channel] = border;
            AddChannelGoalsHeader();
        }

        private void AddChannelGoalsHeader()
        {
            for (int i = 0; i < 3; i++)
            {
                System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                border.BorderBrush = System.Windows.Media.Brushes.Black;
                border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                border.BorderThickness = new Thickness(1);

                TextBlock textBlock = new TextBlock();
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                switch (i % 3)
                {
                    case 0:
                        textBlock.Text = "INS";
                        break;
                    case 1:
                        textBlock.Text = "GRP";
                        break;
                    case 2:
                        textBlock.Text = "BUD";
                        break;
                }
                border.Child = textBlock;
                ugGoals.Children.Add(border);
            }
        }

        private void AddChannelDataGridColumn(ChannelDTO channel)
        {

            DataGrid dataGrid = new DataGrid();
            dataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
            dataGrid.AutoGenerateColumns = false;
            dataGrid.IsManipulationEnabled = false;
            dataGrid.IsReadOnly = true;
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;

            dataGrid.AlternatingRowBackground = System.Windows.Media.Brushes.LightGoldenrodYellow;

            ApplyCellStyle(dataGrid);

            /*var columnData = _data[channel].SelectMany(dateDict => dateDict.Value
                                                               .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));*/


            var insColumn = new DataGridTextColumn
            {
                Binding = new Binding("Insertations"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star) // Set column width to star (*)

            };

            var grpColumn = new DataGridTextColumn
            {
                Binding = new Binding("Grp") { StringFormat = "N2" },
                Width = new DataGridLength(1, DataGridLengthUnitType.Star) // Set column width to star (*)
            };

            var budColumn = new DataGridTextColumn
            {
                Binding = new Binding("Budget") { StringFormat = "N2" },
                Width = new DataGridLength(1, DataGridLengthUnitType.Star) // Set column width to star (*)
            };


            // Add columns to the DataGrid
            dataGrid.Columns.Add(insColumn);
            dataGrid.Columns.Add(grpColumn);
            dataGrid.Columns.Add(budColumn);

            //ugGrid.Children.Add(dataGrid);
            //dataGrid.ItemsSource = columnData;

            _channelGrids.Add(channel, dataGrid);
        }

        public void BindDataValues()
        {
            foreach (var channel in _channels)
            {
                BindDataValues(channel);
            }

            spotGoalsGrid.BindDataValues();

        }

        private void BindDataValues(ChannelDTO channel)
        {
            var columnData = _data[channel].SelectMany(dateDict => dateDict.Value
                                                            .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));
            var dataGrid = _channelGrids[channel];
            dataGrid.ItemsSource = columnData;
            

        }

        #endregion

        #region Selection changed

        /*public void VisibleTuplesChanged(IEnumerable<MediaPlanTuple> visibleMpTuples)
        {
            _visibleTuples.ReplaceRange(visibleMpTuples);
            RecalculateGoals();

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

                        spotGoalsGrid.HideChannel(channel);
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

                        spotGoalsGrid.ShowChannel(channel);

                    }
                }

            }

            if (_selectedChannels.Count >= 2)
            {
                RecalculateTotalColumnSpotGoals();
                ShowChannel(dummyChannel);

                spotGoalsGrid.ShowChannel(dummyChannel);
            }
            else
            {
                RecalculateTotalColumnSpotGoals();
                HideChannel(dummyChannel);

                spotGoalsGrid.HideChannel(dummyChannel);
            }

            spotGoalsGrid.UpdateUgGoals();
            // for ugGoals, just show how many children needs to be shown
            for (int i = 0; i < _visibleChannels.Count * 3; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Visible;
            }
            for (int i = _visibleChannels.Count * 3; i < _channels.Count * 3; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Collapsed;
            }
        }*/

        public void ShowChannel(ChannelDTO channel)
        {
            // Showing Channel and Goals headers
            /*for (int i = 0; i < _channels.Count; i++)
            {
                if (_channels[i].chid == channel.chid)
                {
                    ugChannels.Children[i].Visibility = Visibility.Visible;
                    for (int j = 0; j < 3; j++)
                    {
                        ugGoals.Children[i * 3 + j].Visibility = Visibility.Visible;
                    }
                }
            }*/

            var border = channelBorderDict[channel];
            border.Visibility = Visibility.Visible;

            //Showing channelGrid
            var dataGrid = _channelGrids[channel];
            dataGrid.Visibility = Visibility.Visible;
        }

        public void HideChannel(ChannelDTO channel)
        {
            // Hiding Channel and Goals headers
            for (int i = 0; i < _channels.Count; i++)
            {
                if (_channels[i].chid == channel.chid)
                {
                    ugChannels.Children[i].Visibility = Visibility.Collapsed;
                    for (int j = 0; j < 3; j++)
                    {
                        ugGoals.Children[i * 3 + j].Visibility = Visibility.Collapsed;
                    }
                }
            }

            var border = channelBorderDict[channel];
            border.Visibility = Visibility.Collapsed;

            //Hiding channelGrid
            var dataGrid = _channelGrids[channel];
            dataGrid.Visibility = Visibility.Collapsed;
        }

        /*private void UpdateListsOrder(IEnumerable<ChannelDTO> channels, bool addDummyChannel = false)
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

            spotGoalsGrid._channels = _channels;
            spotGoalsGrid._visibleChannels = _visibleChannels;
        }*/

        public void UpdateUgChannelOrder(IEnumerable<ChannelDTO> channels, bool addDummyChannel = false)
        {
            ugChannels.Children.Clear();
            ugGrid.Children.Clear();
            //UpdateListsOrder(channels, addDummyChannel);

            foreach (var channel in channels)
            {
                ugChannels.Children.Add(channelBorderDict[channel]);
                ugGrid.Children.Add(_channelGrids[channel]);
            }
            if (addDummyChannel)
            {
                ugChannels.Children.Add(channelBorderDict[dummyChannel]);
                ugGrid.Children.Add(_channelGrids[dummyChannel]);
            }
        }

        #endregion

        /*public void ChangeDataForShowing(string dataName)
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
        }*/

        #region Datagrid Functionality

        private void SetWidth()
        {
            int channelsNum = ugChannels.Children.Count;

            int channelWidth = 170;
            int headerWidth = channelsNum * channelWidth;


            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
            ugGrid.Width = headerWidth;
        }

        private int FindBorderIndex(System.Windows.Controls.Border clickedBorder, UniformGrid uniformGrid)
        {
            int index = 0;

            // Iterate through the Children collection of the UniformGrid
            foreach (UIElement child in uniformGrid.Children)
            {
                // Check if the clicked element matches the current child
                if (child == clickedBorder)
                {
                    return index;
                }

                index++;
            }
            return -1;
        }

        private void SpotBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int index = -1;
            if (sender is System.Windows.Controls.Border clickedBorder)
            {
                index = FindBorderIndex(clickedBorder, ugSpots);
            }
            if (index == -1)
                return;

            var dataGrid = _channelGrids[dummyChannel]; // dummy channel always exists
            dataGrid.SelectedIndex = index;
        }

        // Event handler for the custom event in subGrids
        bool first = true;

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (first)
            {
                first = false;
                var dataGrid = sender as DataGrid;
                var selectedIndex = dataGrid.SelectedIndex;
                SelectAllGridRows(selectedIndex);
                first = true;
            }
        }

        private void SelectAllGridRows(int selectedIndex, bool unselectAll = false)
        {
            foreach (var dataGrid in _channelGrids.Values)
            {
                if (unselectAll)
                    dataGrid.UnselectAll();
                dataGrid.SelectedIndex = selectedIndex;
            }
        }

        private void ApplyCellStyle(DataGrid dataGrid)
        {
            var cellStyle = new System.Windows.Style(typeof(DataGridCell));

            Trigger trigger = new Trigger
            {
                Property = DataGridCell.IsSelectedProperty,
                Value = true
            };

            Setter backgroundSetter = new Setter(DataGridCell.BackgroundProperty, System.Windows.Media.Brushes.LightBlue);
            Setter foregroundSetter = new Setter(DataGridCell.ForegroundProperty, System.Windows.Media.Brushes.Black);

            trigger.Setters.Add(backgroundSetter);
            trigger.Setters.Add(foregroundSetter);

            cellStyle.Triggers.Add(trigger);

            dataGrid.Resources.Add(typeof(DataGridCell), cellStyle);
        }

        private void ugGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = svGrid;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

            #region Change dataGrid focus

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (e.Key == Key.Right && GetFocusedColumnIndex(dataGrid) == 2)
            {
                // Focus the next DataGrid to the right
                FocusNextDataGrid(dataGrid, true);
                e.Handled = true;
            }
            else if (e.Key == Key.Left && GetFocusedColumnIndex(dataGrid) == 0)
            {
                // Focus the previous DataGrid to the left
                FocusNextDataGrid(dataGrid, false);
                e.Handled = true;
            }
        }

        private int GetFocusedColumnIndex(DataGrid dataGrid)
        {
            if (dataGrid.CurrentCell != null)
            {
                DataGridColumn focusedColumn = dataGrid.CurrentCell.Column;

                if (focusedColumn != null)
                {
                    // Get the index of the focused column
                    int columnIndex = dataGrid.Columns.IndexOf(focusedColumn);
                    return columnIndex;
                }
            }

            return -1; // No focused cell or column found
        }

        private void FocusNextDataGrid(DataGrid currentDataGrid, bool focusRight)
        {
            DataGrid previousDataGrid = null;
            bool foundCurrent = false;

            foreach (var channelGrid in _channelGrids)
            {
                var channel = channelGrid.Key;
                if (_visibleChannels.Contains(channel))
                {
                    var dataGrid = channelGrid.Value;
                    if (foundCurrent)
                    {
                        // Focus the next or previous cell based on the arrow key direction
                        if (focusRight)
                        {
                            dataGrid.Focus();
                            MoveFocusToFirstCell(dataGrid);
                        }
                        else
                        {
                            dataGrid.Focus();
                            MoveFocusToLastCell(dataGrid);
                        }
                        return;
                    }

                    if (dataGrid == currentDataGrid)
                    {
                        if (focusRight)
                        {
                            foundCurrent = true;
                            continue;
                        }
                        else
                        {
                            if (previousDataGrid != null)
                            {
                                previousDataGrid.Focus();
                                MoveFocusToLastCell(previousDataGrid);
                            }
                        }
                    }

                    previousDataGrid = dataGrid;
                }
            }
        }

        private void MoveFocusToFirstCell(DataGrid dataGrid)
        {
            if (dataGrid != null && dataGrid.SelectedItem != null)
            {
                // Move to the most left cell in the same row
                dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.SelectedItem, dataGrid.Columns[0]);
                dataGrid.ScrollIntoView(dataGrid.SelectedItem, dataGrid.Columns[0]);
            }
        }

        private void MoveFocusToLastCell(DataGrid dataGrid)
        {
            if (dataGrid != null && dataGrid.SelectedItem != null)
            {
                // Move to the most right cell in the same row
                dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.SelectedItem, dataGrid.Columns[dataGrid.Columns.Count - 1]);
                dataGrid.ScrollIntoView(dataGrid.SelectedItem, dataGrid.Columns[dataGrid.Columns.Count - 1]);
            }
        }

            #endregion

        #endregion

        #region Export to Excel
        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            if (_visibleChannels.Count == 0)
                return;


            AddLeftHeadersInWorksheet(worksheet, rowOff + 2, colOff);

            int colOffset = 2;
            foreach (var channel in _visibleChannels)
            {
                AddChannelInWorksheet(channel, worksheet, rowOff, colOff + colOffset, showAllDecimals);
                colOffset += 3;
            }

        }

        private void AddLeftHeadersInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            AddWeeksHeaderInWorksheet(worksheet, rowOff, colOff);
            AddSpotsHeaderInWorksheet(worksheet, rowOff, colOff + 1);
        }
        private void AddChannelInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            AddChannelHeadersInWorksheet(channel, worksheet, rowOff, colOff);
            AddChannelDataInWorksheet(channel, worksheet, rowOff + 2, colOff, showAllDecimals);
        }
        private void AddChannelHeadersInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            AddChannelHeaderInWorksheet(channel, worksheet, rowOff, colOff);
            AddGoalsHeaderInWorksheet(worksheet, rowOff + 1, colOff);
        }

        private void AddWeeksHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var drawingThickness = ExcelBorderStyle.Thick;
            var drawingColor = System.Drawing.Color.Black;

            // Merging cells
            int offset = _spots.Count;
            int rowOffset = rowOff;
            for (int i = 0; i < _data[dummyChannel].Count; i++) // for every week
            {
                // Get the range of cells to merge
                var range = worksheet.Cells[rowOffset, colOff, rowOffset + offset - 1, colOff];
                // Merge the cells
                range.Merge = true;

                range.Style.Border.Bottom.Style = drawingThickness;
                range.Style.Border.Bottom.Color.SetColor(drawingColor);

                rowOffset += offset;
            }

            rowOffset = rowOff;
            // Set the cell values and colors in Excel
            for (int i = 0; i < _data[dummyChannel].Count; i++)
            {
                int weekNum = firstWeekNum + i;
                string weekString = GetWeekLabel(weekNum);
                if (weekNum == lastWeekNum + 1)
                {
                    weekString = "Total";
                }
                var cell = worksheet.Cells[rowOffset, colOff];
                cell.Value = weekString;

                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                rowOffset += offset;
            }
        }

        private void AddSpotsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var drawingThickness = ExcelBorderStyle.Thin;
            var drawingColor = System.Drawing.Color.Black;

            int spotsCount = _spots.Count;

            for (int i = 0; i < _data[dummyChannel].Keys.Count(); i++)
            {
                for (int j = 0; j < spotsCount; j++)
                {
                    string label = GetSpotLabel(_spots[j]);

                    var cell = worksheet.Cells[rowOff + spotsCount * i + j, colOff];
                    cell.Value = label;

                    // Set the cell color
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                    cell.Style.Border.Bottom.Style = drawingThickness;
                    cell.Style.Border.Bottom.Color.SetColor(drawingColor);

                }
            }
        }


        private void AddChannelHeaderInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            // Merging cells
            int offset = 2;

            // Get the range of cells to merge
            var range = worksheet.Cells[rowOff, colOff, rowOff, colOff + offset];
            // Merge the cells
            range.Merge = true;

            var cell = worksheet.Cells[rowOff, colOff];
            cell.Value = channel.chname.Trim();

            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));

        }

        private void AddGoalsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {

            // Set the column headers in Excel 
            List<ExcelRange> cells = new List<ExcelRange>();
            var cellIns = worksheet.Cells[rowOff, colOff];
            cellIns.Value = "INS";
            cells.Add(cellIns);

            var cellGrp = worksheet.Cells[rowOff, colOff + 1];
            cellGrp.Value = "GRP";
            cells.Add(cellGrp);

            var cellBud = worksheet.Cells[rowOff, colOff + 2];
            cellBud.Value = "BUD";
            cells.Add(cellBud);

            foreach (var cell in cells)
            {
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            }
        }

        private void AddChannelDataInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            var dataGrid = _channelGrids[channel];

            //Unselect all rows
            dataGrid.SelectedItem = null;

            int rowOffset = rowOff;
            foreach (SpotGoals spotGoals in dataGrid.Items)
            {
                worksheet.Cells[rowOffset, colOff].Value = spotGoals.Insertations;
                if (showAllDecimals)
                {
                    worksheet.Cells[rowOffset, colOff + 1].Value = spotGoals.Grp;
                    worksheet.Cells[rowOffset, colOff + 2].Value = spotGoals.Budget;
                }
                else
                {
                    worksheet.Cells[rowOffset, colOff + 1].Value = Math.Round(spotGoals.Grp);
                    worksheet.Cells[rowOffset, colOff + 2].Value = Math.Round(spotGoals.Budget, 2).ToString("#,##0.00");
                }
                

                rowOffset += 1;
            }
        }

        #endregion       

    }
}
