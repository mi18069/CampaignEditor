﻿using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// Grid which have multiple headers and is used in showing goals of campaign 
    /// </summary>
    public partial class SpotGoalsGrid : UserControl
    {

        // for duration of campaign
        public int firstWeekNum;
        public int lastWeekNum;
        public DateTime startDate;
        public DateTime endDate;

        public List<SpotDTO> _spots { get; set; }
        public List<ChannelDTO> _channels { get; set; }
        public List<ChannelDTO> _selectedChannels { get; set; }
        public List<ChannelDTO> _visibleChannels;
        public Dictionary<ChannelDTO, Dictionary<int, Dictionary<SpotDTO, SpotGoals>>> _data { get; set; }
        public ChannelDTO dummyChannel { get; set; } // Dummy channel for Total Column

        private Dictionary<int, Dictionary<ChannelDTO, DataGrid>> _channelWeekGrids = new Dictionary<int, Dictionary<ChannelDTO, DataGrid>>();
        private Dictionary<int, Dictionary<ChannelDTO, System.Windows.Controls.Border>> channelBorderDict = new Dictionary<int, Dictionary<ChannelDTO, System.Windows.Controls.Border>>();


        public SpotGoalsGrid()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            ugGrid.Children.Clear();
            _channelWeekGrids.Clear();
            channelBorderDict.Clear();

            CreateOutboundHeaders();
            SetWidth();
        }

        public void ConstructGrid(DateTime startDate, DateTime endDate)
        {
            ugGrid.Children.Clear();
            _channelWeekGrids.Clear();
            channelBorderDict.Clear();

            this.startDate = startDate;
            this.endDate = endDate;

            CreateOutboundHeaders();
            SetWidth();
        }

        private void CreateOutboundHeaders()
        {
            ugWeeks.Children.Clear();
            ugChannels.Children.Clear();
            ugGoals.Children.Clear();
            ugSpots.Children.Clear();
            // Add headers
            // Weeks
            int weeksNum = TimeFormat.GetWeeksBetween(startDate, endDate) + 1; // last + 1 is for Total footer 
            ugWeeks.Columns = weeksNum;
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
            foreach (var spot in _spots)
            {
                
                System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                border.BorderBrush = System.Windows.Media.Brushes.Black;
                border.Background = System.Windows.Media.Brushes.LightGoldenrodYellow;
                border.BorderThickness = new Thickness(1, 1, 1, 1);               
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.FontWeight = FontWeights.Bold;

                string label = GetSpotLabel(spot);
                textBlock.Text = label;

                border.Child = textBlock;
                border.MouseLeftButtonDown += SpotBorder_MouseLeftButtonDown;

                ugSpots.Children.Add(border);
                

            }


            ugChannels.Columns = _channels.Count * ugWeeks.Columns;
            ugGoals.Columns = ugChannels.Columns * 3;
            ugGrid.Columns = ugChannels.Columns;

            for (int i = 0; i < weeksNum; i++)
            {
                int currentWeek = firstWeekNum + i;

                if (currentWeek > weeksInYear)
                {
                    currentWeek = currentWeek % (weeksInYear + 1) + 1; // so there are no week 0
                }

                var dictBorder = new Dictionary<ChannelDTO, System.Windows.Controls.Border>();
                var dictGrid = new Dictionary<ChannelDTO, DataGrid>();
                foreach (var channel in _channels)
                {
                    AddChannelDataColumn(channel, currentWeek, dictBorder, dictGrid);
                }

                channelBorderDict.Add(currentWeek, dictBorder);
                _channelWeekGrids.Add(currentWeek, dictGrid);
            }

            UpdateUgChannelOrder(_channels);


            foreach (var channel in _channels)
            {
                HideChannel(channel);
            }

            for (int i = 0; i < ugGoals.Children.Count; i++)
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

            return calendar.GetWeekOfYear(date, dtfi.CalendarWeekRule, dtfi.FirstDayOfWeek) - 1;
        }
        private int GetWeekOfYear(DateOnly date)
        {
            DateTime dateTime = date.ToDateTime(TimeOnly.Parse("00:01 AM"));
            return GetWeekOfYear(dateTime);
        }

        #region ChannelDataColumn
        private void AddChannelDataColumn(ChannelDTO channel, int weekNum, Dictionary<ChannelDTO, System.Windows.Controls.Border> dictBorder, Dictionary<ChannelDTO, DataGrid> dictGrid)
        {
            AddChannelHeader(channel, dictBorder);
            AddChannelDataGridColumn(channel, weekNum, dictGrid);
        }

        private void AddChannelHeader(ChannelDTO channel, Dictionary<ChannelDTO, System.Windows.Controls.Border> dictBorder)
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
            dictBorder[channel] = border;

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

        private void AddChannelDataGridColumn(ChannelDTO channel, int weekNum, Dictionary<ChannelDTO, DataGrid> dict)
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
           
            /*var columnData = _data[channel].Where(dateDict => dateDict.Key == weekNum)
                                           .SelectMany(dateDict => dateDict.Value
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

            dict.Add(channel, dataGrid);
        }

        public void BindDataValues()
        {
            for (int weekNum = firstWeekNum; weekNum <= lastWeekNum + 1; weekNum++)
            {
                var dataGridWeek = _channelWeekGrids[weekNum];

                foreach (var channel in _channels)
                {
                    var columnData = _data[channel].Where(dateDict => dateDict.Key == weekNum).SelectMany(dateDict => dateDict.Value.Select(spotSpotGoalsDict => spotSpotGoalsDict.Value));

                    var dataGrid = dataGridWeek[channel];
                    dataGrid.ItemsSource = columnData;
                }
            }


        }



        #endregion

        #region Selection changed

        public void UpdateUgGoals()
        {
            // for ugGoals, just show how many children needs to be shown
            for (int i = 0; i < _visibleChannels.Count * 3 * ugWeeks.Children.Count; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Visible;
            }
            for (int i = _visibleChannels.Count * 3 * ugWeeks.Children.Count; i < _channels.Count * 3 * ugWeeks.Children.Count; i++)
            {
                ugGoals.Children[i].Visibility = Visibility.Collapsed;
            }
        }
        public void ShowChannel(ChannelDTO channel)
        {
            // Showing Channel and Goals headers
            
            /*for (int i = 0; i < ugWeeks.Children.Count; i++)
            {
                for (int j = 0; j < _channels.Count; j++)
                {
                    if (_channels[j].chid == channel.chid)
                    {
                        int channelIndex = i * _channels.Count + j;
                        ugChannels.Children[channelIndex].Visibility = Visibility.Visible;
                        for (int k = 0; k < 3; k++)
                        {
                            ugGoals.Children[channelIndex * 3 + k].Visibility = Visibility.Visible;
                        }
                    }
                }

            }*/

            foreach (var dict in channelBorderDict.Values)
            {
                foreach (var dictChannel in dict.Keys)
                {
                    if (dictChannel.chid == channel.chid)
                    {
                        dict[dictChannel].Visibility = Visibility.Visible;
                    }
                }
            }

            //Showing channelGrid

            foreach (var dict in _channelWeekGrids.Values)
            {
                foreach (var dictChannel in dict.Keys)
                {
                    if (dictChannel.chid == channel.chid)
                    {
                        dict[dictChannel].Visibility = Visibility.Visible;
                    }
                }
            }

            ResizeUgWeeks();
        }

        public void HideChannel(ChannelDTO channel)
        {
            // Hiding Channel and Goals headers

            /*for (int i = 0; i < ugWeeks.Children.Count; i++)
            {
                for (int j = 0; j < _channels.Count; j++)
                {
                    if (_channels[j].chid == channel.chid)
                    {
                        int channelIndex = i * _channels.Count + j;
                        ugChannels.Children[channelIndex].Visibility = Visibility.Collapsed;
                        for (int k = 0; k < 3; k++)
                        {
                            ugGoals.Children[channelIndex * 3 + k].Visibility = Visibility.Collapsed;
                        }
                    }
                }

            }*/

            foreach (var dict in channelBorderDict.Values)
            {
                foreach (var dictChannel in dict.Keys)
                {
                    if (dictChannel.chid == channel.chid)
                    {
                        dict[dictChannel].Visibility = Visibility.Collapsed;
                    }
                }
            }

            //Hiding channelGrid

            foreach (var dict in _channelWeekGrids.Values)
            {
                foreach (var dictChannel in dict.Keys)
                {
                    if (dictChannel.chid == channel.chid)
                    {
                        dict[dictChannel].Visibility = Visibility.Collapsed;
                    }
                }
            }
            ResizeUgWeeks();
        }

        /*private void UpdateListsOrder(IEnumerable<ChannelDTO> channels)
        {
            _channels = channels.ToList();
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
        }*/

        public void UpdateUgChannelOrder(IEnumerable<ChannelDTO> channels, bool addDummyChannel = false)
        {
            ugChannels.Children.Clear();
            ugGrid.Children.Clear();
            //UpdateListsOrder(channels);

            int weeksInYear = TimeFormat.GetWeeksInYear(startDate.Year);
            for (int i = 0; i < ugWeeks.Children.Count; i++)
            {
                int currentWeek = firstWeekNum + i;

                if (currentWeek > weeksInYear)
                {
                    currentWeek = currentWeek % (weeksInYear + 1) + 1; // so there are no week 0
                }
                foreach (var channel in channels)
                {
                    ugChannels.Children.Add(channelBorderDict[currentWeek][channel]);
                    ugGrid.Children.Add(_channelWeekGrids[currentWeek][channel]);
                }

                if (addDummyChannel)
                {
                    ugChannels.Children.Add(channelBorderDict[currentWeek][dummyChannel]);
                    ugGrid.Children.Add(_channelWeekGrids[currentWeek][dummyChannel]);
                }
            }
            
        }

        #endregion



        #region Datagrid Functionality

        private void SetWidth()
        {
            int channelsNum = ugChannels.Children.Count;

            int channelWidth = 170;
            int headerWidth = channelsNum * channelWidth;


            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
            ugGrid.Width = headerWidth;
            ResizeUgWeeks();
        }

        private void ResizeUgWeeks()
        {
            int visibleChannels = _visibleChannels.Count();

            int channelWidth = 170;
            int channelsNum = visibleChannels * ugWeeks.Children.Count;

            if (channelsNum == 0)
            {
                ugWeeks.Width = channelWidth * ugWeeks.Children.Count;
            }
            else
            {
                int headerWidth = channelsNum * channelWidth;
                ugWeeks.Width = headerWidth;
            }

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

            var dict = _channelWeekGrids[lastWeekNum + 1]; // total always exists
            var dataGrid = dict[dummyChannel]; // dummy channel always exist
            dataGrid.SelectedIndex = index;
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
            var dataGrids = _channelWeekGrids.Values.SelectMany(dict => dict.Values);
            foreach (var dataGrid in dataGrids)
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

            foreach (var channelGrid in _channelWeekGrids.Values)
            {
                var channels = channelGrid.Keys;
                foreach (var channel in channels)
                {
                    if (_visibleChannels.Contains(channel))
                    {
                        var dataGrid = channelGrid[channel];
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


            AddLeftHeadersInWorksheet(worksheet, rowOff + 3, colOff);

            int colOffset = 1;
            int weekCount = TimeFormat.GetWeeksBetween(startDate, endDate) + 1;
            int weeksInYear = TimeFormat.GetWeeksInYear(startDate.Year);

            for (int i = 0; i < weekCount; i++)
            {
                int currentWeek = firstWeekNum + i;

                if (currentWeek > weeksInYear)
                {
                    currentWeek = currentWeek % (weeksInYear + 1) + 1; // so there are no week 0
                }

                AddWeeksHeaderInWorksheet(currentWeek, worksheet, rowOff, colOff + colOffset, showAllDecimals);
                colOffset += 3 * _visibleChannels.Count;
            }

        }

        private void AddLeftHeadersInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            AddSpotsHeaderInWorksheet(worksheet, rowOff, colOff);
        }
        private void AddChannelInWorksheet(ChannelDTO channel, int weekNum, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            AddChannelHeadersInWorksheet(channel, worksheet, rowOff, colOff);
            AddChannelDataInWorksheet(channel, weekNum, worksheet, rowOff + 2, colOff, showAllDecimals);
        }
        private void AddChannelHeadersInWorksheet(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            AddChannelHeaderInWorksheet(channel, worksheet, rowOff, colOff);
            AddGoalsHeaderInWorksheet(worksheet, rowOff + 1, colOff);
        }

        private void AddWeeksHeaderInWorksheet(int weekNum, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            var drawingThickness = ExcelBorderStyle.Thick;
            var drawingColor = System.Drawing.Color.Black;

            // Merging week cells
            int offset = _visibleChannels.Count * 3 - 1;

            // Get the range of cells to merge
            var range = worksheet.Cells[rowOff, colOff, rowOff, colOff + offset];
            // Merge the cells
            range.Merge = true;

            range.Style.Border.Bottom.Style = drawingThickness;
            range.Style.Border.Bottom.Color.SetColor(drawingColor);


            // Set the cell value and colors in Excel
            string weekString = GetWeekLabel(weekNum);
            if (weekNum == lastWeekNum + 1)
            {
                weekString = "Total";
            }

            range.Value = weekString;

            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int colOffset = 0;
            foreach (var channel in _visibleChannels)
            {
                AddChannelInWorksheet(channel, weekNum, worksheet, rowOff + 1, colOff + colOffset, showAllDecimals);
                colOffset += 3;
            }
        }

        private void AddSpotsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var drawingThickness = ExcelBorderStyle.Thin;
            var drawingColor = System.Drawing.Color.Black;

            for (int i = 0; i < _spots.Count; i++)
            {
                string label = GetSpotLabel(_spots[i]);

                var cell = worksheet.Cells[rowOff + i, colOff];
                cell.Value = label;

                // Set the cell color
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                cell.Style.Border.Bottom.Style = drawingThickness;
                cell.Style.Border.Bottom.Color.SetColor(drawingColor);

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

        private void AddChannelDataInWorksheet(ChannelDTO channel, int weekNum, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            var dataGrid = _channelWeekGrids[weekNum][channel];

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
