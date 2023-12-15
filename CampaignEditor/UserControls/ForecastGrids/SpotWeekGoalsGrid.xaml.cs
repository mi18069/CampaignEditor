using CampaignEditor.DTOs.CampaignDTO;
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
using CampaignEditor.Controllers;
using System.Threading.Tasks;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This grid is very similar to SpotGoalsGrid, so I'll pass it some SpotGoalsGrid, and make
    /// this grid based on that, on that way, there's no need to calculate same things more than once
    /// </summary>
    public partial class SpotWeekGoalsGrid : UserControl
    {

        CampaignDTO _campaign;
        int _version = 1;


        // for duration of campaign
        DateTime startDate;
        DateTime endDate;
        int firstWeekNum;
        int lastWeekNum;

        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();
        public MediaPlanController _mediaPlanController { get; set; }
        public MediaPlanTermController _mediaPlanTermController { get; set; }
        public SpotController _spotController { get; set; }
        public ChannelController _channelController { get; set; }
        private List<ChannelDTO> _selectedChannels = new List<ChannelDTO>();
        private List<ChannelDTO> _visibleChannels = new List<ChannelDTO>();

        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        private Dictionary<Char, int> _spotLengths = new Dictionary<char, int>();

        private Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>> _data;
        private Dictionary<ChannelDTO, DataGrid> _channelGrids = new Dictionary<ChannelDTO, DataGrid>();

        private ChannelDTO dummyChannel = new ChannelDTO(-1, "Total", true, 0, "", 0, 0); // Dummy channel for Total Column

        public SpotWeekGoalsGrid()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign, int version)
        {
            _campaign = campaign;
            _version = version;

            _spots.Clear();
            _spotLengths.Clear();
            _channels.Clear();
            ugChannels.Children.Clear();
            ugGoals.Children.Clear();
            ugWeeks.Children.Clear();
            ugSpots.Children.Clear();
            ugGrid.Children.Clear();


            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            firstWeekNum = GetWeekOfYear(startDate);
            lastWeekNum = GetWeekOfYear(endDate);

            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            _spots = spots.ToList();
            _spots = _spots.OrderBy(s => s.spotcode).ToList();
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

            var channelIds = await _mediaPlanController.GetAllChannelsByCmpid(_campaign.cmpid, _version);
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (var chid in channelIds)
            {
                ChannelDTO channel = await _channelController.GetChannelById(chid);
                channels.Add(channel);
            }
            _channels = channels.OrderBy(c => c.chname).ToList();
            _channels.Add(dummyChannel);

            TransformData();
            CreateOutboundHeaders();
            SetWidth();
        }

        public void TransformData()
        {
            InitializeData();
            RecalculateGoals();
        }

        // Making appropriate _data structure, with all zero values for SpotGoals
        private void InitializeData()
        {
            _data = new Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>>();
            int weeksNum = lastWeekNum - firstWeekNum + 1 + 1; // last +1 is for Total header

            foreach (var channel in _channels)
            {
                var weekGoalsDict = new Dictionary<int, Dictionary<SpotDTO, SpotGoals>>();

                for (int i = 0; i < weeksNum; i++)
                {
                    int currentWeek = firstWeekNum + i;
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
        }
        public void RecalculateGoals(ChannelDTO channel, int weekNum, SpotDTO spot, bool updateTotal = false)
        {
            var spotGoals = _data[channel][weekNum][spot];

            List<int> weekIndexes = GetWeekIndexes(weekNum);

            var channelMpTuples = _allMediaPlans.Where(mpt => mpt.MediaPlan.chid == channel.chid);

            int ins = 0;
            double grp = 0;
            double budget = 0;
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
                            ins += 1;
                            grp += mediaPlan.Amrp1;
                            // Need to fix this
                            budget += (mediaPlan.Price / mediaPlan.Length) * spot.spotlength;
                        }
                    }
                }
                
            }
            spotGoals.Insertations = ins;
            spotGoals.Grp = grp;
            spotGoals.Budget = budget;

            if (updateTotal)
            {
                RecalculateTotalColumnSpotGoals(weekNum, spot);
                RecalculateTotalFooterSpotGoals(channel, spot);
                RecalculateTotalFooterSpotGoals(dummyChannel, spot);
            }
        }

        private List<int> GetWeekIndexes(int weekNum)
        {
            DateOnly firstDate = DateOnly.FromDateTime(startDate);
            DateOnly lastDate = DateOnly.FromDateTime(endDate);

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

        private void RecalculateTotalFooterSpotGoals(ChannelDTO channel)
        {

            foreach (var spot in _spots)
            {
                RecalculateTotalFooterSpotGoals(channel, spot);
            }
        }

        private void RecalculateTotalFooterSpotGoals(ChannelDTO channel, SpotDTO spot)
        {
            var weekNum = lastWeekNum + 1;
            var totalSpotGoals = _data[channel][weekNum][spot];

            var channelSpotGoals = _data[channel].SelectMany(dateDict => dateDict.Value
                                                .Where(spotSpotGoalsDict => spotSpotGoalsDict.Key.spotcode.Trim() == spot.spotcode.Trim())
                                                .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));

            int ins = 0;
            double grp = 0;
            double budget = 0;
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
            double grp = 0;
            double budget = 0;

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

        #endregion
        private void CreateOutboundHeaders()
        {

            // Add headers
            // Weeks
            ugWeeks.Rows = lastWeekNum - firstWeekNum + 1 + 1; // last + 1 is for Total footer 
            for (int i = firstWeekNum; i <= lastWeekNum + 1; i++) // + 1 is for Total footer 
            {

                System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                border.BorderBrush = System.Windows.Media.Brushes.Black;
                border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                if (i == firstWeekNum)
                {
                    border.BorderThickness = new Thickness(1, 3, 1, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 1, 1, 1);
                }
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.FontWeight = FontWeights.Bold;
                textBlock.Text = GetWeekLabel(i);

                // Changing last Week to Total
                if (i == lastWeekNum + 1)
                {
                    textBlock.Text = "Total";
                }

                border.Child = textBlock;

                ugWeeks.Children.Add(border);
            }


            // Spots
            for (int i = firstWeekNum; i < lastWeekNum + 1 + 1; i++) // +1 because of Total row
            {
                for (int j = 0; j < _spots.Count; j++)
                {
                    System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                    border.BorderBrush = System.Windows.Media.Brushes.Black;
                    border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                    if (j == _spots.Count - 1)
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 2);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 1);
                    }
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
            foreach (var channel in _channels)
            {
                HideChannel(channel);
            }

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

            return calendar.GetWeekOfYear(date, dtfi.CalendarWeekRule, dtfi.FirstDayOfWeek) - 1;
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
            ugChannels.Children.Add(border);

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

            var columnData = _data[channel].SelectMany(dateDict => dateDict.Value
                                                               .Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));


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

            ugGrid.Children.Add(dataGrid);
            dataGrid.ItemsSource = columnData;

            _channelGrids.Add(channel, dataGrid);
        }

        #endregion

        #region Selection changed
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
                        _visibleChannels.Add(channel);

                        if (_selectedChannels.Count >= 2 && !_visibleChannels.Contains(dummyChannel))
                            _visibleChannels.Add(dummyChannel);
                    }
                }

            }

            if (_selectedChannels.Count >= 2)
            {
                RecalculateTotalColumnSpotGoals();
                ShowChannel(dummyChannel);
            }
            else
            {
                RecalculateTotalColumnSpotGoals();
                HideChannel(dummyChannel);
            }
        }

        private void ShowChannel(ChannelDTO channel)
        {
            // Showing Channel and Goals headers
            for (int i = 0; i < _channels.Count; i++)
            {
                if (_channels[i].chid == channel.chid)
                {
                    ugChannels.Children[i].Visibility = Visibility.Visible;
                    for (int j = 0; j < 3; j++)
                    {
                        ugGoals.Children[i * 3 + j].Visibility = Visibility.Visible;
                    }
                }
            }

            //Showing channelGrid
            var dataGrid = _channelGrids[channel];
            dataGrid.Visibility = Visibility.Visible;
        }

        private void HideChannel(ChannelDTO channel)
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

            //Hiding channelGrid
            var dataGrid = _channelGrids[channel];
            dataGrid.Visibility = Visibility.Collapsed;
        }

        #endregion



        #region Datagrid Functionality

        private void SetWidth()
        {
            int channelsNum = ugChannels.Children.Count;

            int channelWidth = 150;
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
                            previousDataGrid.Focus();
                            MoveFocusToLastCell(previousDataGrid);
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
        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var visibleChannels = _visibleChannels;

            if (visibleChannels.Count == 0)
                return;

            AddLeftHeadersInWorksheet(worksheet, rowOff + 2, colOff);

            int colOffset = 2;
            foreach (var channel in visibleChannels)
            {
                AddChannelInWorksheet(channel, worksheet, rowOff, colOff + colOffset);
                colOffset += 3;
            }

        }

        private void AddLeftHeadersInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            AddWeeksHeaderInWorksheet(worksheet, rowOff, colOff);
            AddSpotsHeaderInWorksheet(worksheet, rowOff, colOff + 1);
        }
        private void AddChannelInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            AddChannelHeadersInWorksheet(channel, worksheet, rowOff, colOff);
            AddChannelDataInWorksheet(channel, worksheet, rowOff + 2, colOff);
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

            for (int i = 1; i <= _data[dummyChannel].Keys.Count(); i++)
            {
                for (int j = 0; j < _spots.Count; j++)
                {
                    string label = GetSpotLabel(_spots[j]);

                    var cell = worksheet.Cells[rowOff * i + j, colOff];
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

        private void AddChannelDataInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var dataGrid = _channelGrids[channel];

            //Unselect all rows
            dataGrid.SelectedItem = null;

            int rowOffset = rowOff;
            foreach (SpotGoals spotGoals in dataGrid.Items)
            {
                worksheet.Cells[rowOffset, colOff].Value = spotGoals.Insertations;
                worksheet.Cells[rowOffset, colOff + 1].Value = Math.Round(spotGoals.Grp, 2);
                worksheet.Cells[rowOffset, colOff + 2].Value = Math.Round(spotGoals.Budget, 2);

                rowOffset += 1;
            }
        }

        #endregion

        /*private void CreateOutboundHeaders()
        {
            int firstWeekNum = GetWeekOfYear(startDate);
            int lastWeekNum = GetWeekOfYear(endDate);

            // Add headers
            // Weeks
            ugWeeks.Rows = lastWeekNum - firstWeekNum + 1 + 1; // last + 1 is for Header Total
            for (int i = firstWeekNum; i <= lastWeekNum; i++)
            {

                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                border.Background = Brushes.LightGoldenrodYellow;
                if (i == firstWeekNum)
                {
                    border.BorderThickness = new Thickness(1, 3, 1, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 1, 1, 1);
                }
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.FontWeight = FontWeights.Bold;
                textBlock.Text = "Week " + i.ToString();

                border.Child = textBlock;

                ugWeeks.Children.Add(border);
            }

            AddWeeksHeaderTotal();

            // Spots
            for (int i = firstWeekNum; i < lastWeekNum + 1 + 1; i++) // +1 because of Total row 
            {
                for (int j=0; j<_spots.Count; j++)
                {
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.Background = Brushes.LightGoldenrodYellow;
                    if (j == _spots.Count - 1)
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 2);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 1);
                    }
                    TextBlock textBlock = new TextBlock();

                    textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.FontWeight = FontWeights.Bold;

                    SpotDTO spot = _spots[j];
                    string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim() + $" ({spot.spotlength})";
                    textBlock.Text = label;

                    border.Child = textBlock;

                    ugSpots.Children.Add(border);
                }

            }

           

            //Channels
            ugChannels.Columns = (_channels.Count + 1);

            for (int i = 0; i < _channels.Count + 1; i++)
            {

                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                if (i == _channels.Count)
                {
                    border.Background = Brushes.Yellow;
                }
                else
                {
                    border.Background = Brushes.LightGoldenrodYellow;
                }

                if (i == 0)
                {
                    border.BorderThickness = new Thickness(3, 1, 3, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 1, 3, 1);
                }
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;

                if (i == _channels.Count)
                {
                    textBlock.Text = "Total";
                }
                else
                {
                    textBlock.Text = _channels[i].chname.Trim();
                }

                border.Child = textBlock;

                ugChannels.Children.Add(border);

            }



            //Goals
            ugGoals.Columns = ugChannels.Columns * 3;
            for (int j = 0; j < ugChannels.Columns; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.Background = Brushes.LightGoldenrodYellow;
                    if (i == 0 && j == 0)
                    {
                        border.BorderThickness = new Thickness(3, 1, 3, 1);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 3, 1);
                    }
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

        }
        

        private void AddGrid(UniformGrid grid)
        {
            // Set num of Columns
            int nCol = ugChannels.Children.Count;
            ugGrid.Columns = nCol;

            // Iterate through the child elements in the UniformGrid
            int i = 0;
            foreach (var child in grid.Children)
            {
                if (child is SpotGoalsSubGrid sg)
                {
                    var nsg = new SpotGoalsSubGrid(sg);
                    nsg.row = i / nCol;
                    nsg.SelectedRowChanged += SubGrid_SelectedRowChanged;
                    ugGrid.Children.Add(nsg);
                }
                else if (child is SpotGoalsTotalSubGrid tsg)
                {
                    var ntsg = new SpotGoalsTotalSubGrid(tsg);
                    ntsg.row = i / nCol;
                    ntsg.SelectedRowChanged += SubGrid_SelectedRowChanged;
                    ugGrid.Children.Add(ntsg);
                }
                else
                {
                    continue;
                }

                i++;
            }
        }

        private void AddWeeksHeaderTotal()
        {
            Border border = new Border();
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.Yellow;

            border.BorderThickness = new Thickness(1, 2, 1, 1);

            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Text = "Total";

            border.Child = textBlock;

            ugWeeks.Children.Add(border);
        }

        private int GetWeekOfYear(DateTime date)
        {
            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.Calendar calendar = cultureInfo.Calendar;

            System.Globalization.DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;
            dtfi.FirstDayOfWeek = DayOfWeek.Monday;

            return calendar.GetWeekOfYear(date, dtfi.CalendarWeekRule, dtfi.FirstDayOfWeek) - 1;
        }

        private void SetWidth()
        {
            int channelsNum = ugChannels.Children.Count;

            int channelWidth = 150;
            int headerWidth = channelsNum * channelWidth;


            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
            ugGrid.Width = headerWidth;
        }

        // Event handler for the custom event in subGrids
        bool first = true;
        private void SubGrid_SelectedRowChanged(object sender, int rowIndex)
        {
            if (first)
            {
                first = false;
                var sg = sender as SpotGoalsSubGrid;
                if (sg != null && rowIndex != -1)
                {
                    int sgRow = sg.row;
                    SelectAllSubgridRows(rowIndex, sgRow);
                }
                else if (rowIndex != -1)
                {
                    var tsg = sender as SpotGoalsTotalSubGrid;
                    if (tsg != null)
                    {
                        int sgRow = tsg.row;
                        SelectAllSubgridRows(rowIndex, sgRow);
                    }
                }
                first = true;
            }

        }

        private void SelectAllSubgridRows(int rowIndex, int sgRow)
        {
            if (rowIndex != null)
            {
                foreach (var subGrid in ugGrid.Children)
                {
                    SpotGoalsSubGrid sg = subGrid as SpotGoalsSubGrid;
                    if (sg == null)
                    {
                        SpotGoalsTotalSubGrid tsg = subGrid as SpotGoalsTotalSubGrid;
                        if (tsg != null)
                        {
                            tsg.Unselect();
                            if (tsg.row == sgRow)
                                tsg.SelectByRowIndex(rowIndex);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        sg.Unselect();
                        if (sg.row == sgRow)
                            sg.SelectByRowIndex(rowIndex);
                    }
                }
            }
        }

        #region Export to Excel
        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            AddHeadersInWorksheet(worksheet, rowOff, colOff);

            var rowOffset = rowOff + 2; // because of headers
            var colOffset = colOff + 2; // because of headers

            int colOffChn = 0;
            foreach (var subGrid in ugGrid.Children)
            {

                var sg = subGrid as SpotGoalsSubGrid;
                if (sg != null)
                {
                    sg.PopulateWorksheet(worksheet, rowOffset, colOffset + colOffChn * 3);
                }
                else
                {
                    var tsg = subGrid as SpotGoalsTotalSubGrid;
                    tsg.PopulateWorksheet(worksheet, rowOffset, colOffset + colOffChn * 3); // we have 3 spotGoals
                }
                colOffChn += 1;
                if (colOffChn == _channels.Count + 1)
                {
                    colOffChn = 0;
                    rowOffset += _spots.Count;
                }
            }

        }

        private void AddHeadersInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            AddWeeksHeaderInWorksheet(worksheet, rowOff + 2, colOff);
            AddSpotsHeaderInWorksheet(worksheet, rowOff + 2, colOff + 1);
            AddChannelsHeaderInWorksheet(worksheet, rowOff, colOff + 2);
            AddGoalsHeaderInWorksheet(worksheet, rowOff + 1, colOff + 2);
        }

        private void AddWeeksHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            var drawingThickness = ExcelBorderStyle.Thick;
            var drawingColor = System.Drawing.Color.Black;

            // Merging cells
            int offset = _spots.Count;
            for (int i = 0, rowOffset = rowOff; i < ugWeeks.Rows; i++, rowOffset += offset)
            {
                // Get the range of cells to merge
                var range = worksheet.Cells[1 + rowOffset, 1 + colOff, offset + rowOffset, 1 + colOff];
                // Merge the cells
                range.Merge = true;

                range.Style.Border.Bottom.Style = drawingThickness;
                range.Style.Border.Bottom.Color.SetColor(drawingColor);
            }


            // Set the cell values and colors in Excel
            for (int rowIndex = 0; rowIndex < ugWeeks.Children.Count; rowIndex++)
            {
                string cellValue = string.Empty;

                var weekBorder = ugWeeks.Children[rowIndex] as Border;
                if (weekBorder != null)
                {
                    var weekTextBlock = weekBorder.Child as TextBlock;
                    if (weekTextBlock != null)
                    {
                        cellValue = weekTextBlock.Text;
                    }
                }

                worksheet.Cells[rowIndex * offset + 1 + rowOff, colOff + 1].Value = cellValue;

                // Set the cell color
                var cell = worksheet.Cells[rowIndex * offset + 1 + rowOff , colOff + 1];
                if (cell != null)
                {
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    double cellWidth = 10;

                    // Set the size of the Excel cell
                    worksheet.Column(colOff + 1).Width = cellWidth;
                    //worksheet.Row(rowIndex + 1 + rowOff).OutlineLevel = 2;

                }

            }
        }

        private void AddSpotsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            // Set the cell values and colors in Excel
            for (int rowIndex = 0; rowIndex < ugSpots.Children.Count; rowIndex++)
            {
                string cellValue = string.Empty;

                var spotBorder = ugSpots.Children[rowIndex] as Border;
                if (spotBorder != null) 
                {
                    var spotTextBlock = spotBorder.Child as TextBlock;
                    if (spotTextBlock != null)
                    {
                        cellValue = spotTextBlock.Text;
                    }
                }
                worksheet.Cells[rowIndex + 1 + rowOff, colOff + 1].Value = cellValue;

                var drawingThickness = ExcelBorderStyle.Thin;
                var drawingColor = System.Drawing.Color.Black;

                // Set the cell color
                var cell = worksheet.Cells[rowIndex + 1 + rowOff, colOff + 1];
                if (cell != null)
                {
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                    cell.Style.Border.Bottom.Style = drawingThickness;
                    cell.Style.Border.Bottom.Color.SetColor(drawingColor);

                    double cellWidth = 30;

                    // Set the size of the Excel cell
                    worksheet.Column(colOff + 1).Width = cellWidth;
                    //worksheet.Row(rowIndex + 1 + rowOff).OutlineLevel = 2;

                }


            }
        }
        

        private void AddChannelsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            // Merging cells
            int offset = (int)(ugGoals.Columns / ugChannels.Columns);
            for (int i = 0, colOffset = colOff; i < ugChannels.Columns; i++, colOffset += offset)
            {
                // Get the range of cells to merge
                var range = worksheet.Cells[1 + rowOff, 1 + colOffset, 1 + rowOff, colOffset + offset];
                // Merge the cells
                range.Merge = true;
            }

            // Set the column headers in Excel
            for (int columnIndex = 0; columnIndex < ugChannels.Columns; columnIndex++)
            {
                var border = ugChannels.Children[columnIndex] as Border;
                var content = string.Empty;
                if (border != null)
                {
                    var textBlock = border.Child as TextBlock;
                    if (textBlock != null)
                    {
                        content = textBlock.Text;
                    }
                }

                var cell = worksheet.Cells[1 + rowOff, columnIndex * offset + 1 + colOff];
                cell.Value = content;

                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            }
        }

        private void AddGoalsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            // Set the column headers in Excel
            for (int columnIndex = 0; columnIndex < ugGoals.Columns; columnIndex++)
            {
                var border = ugGoals.Children[columnIndex] as Border;
                var content = string.Empty;
                if (border != null)
                {
                    var textBlock = border.Child as TextBlock;
                    if (textBlock != null)
                    {
                        content = textBlock.Text;
                    }
                }

                var cell = worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff];
                cell.Value = content;

                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            }
        }

        #endregion

        private void ugGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = svGrid;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }*/

    }
}
