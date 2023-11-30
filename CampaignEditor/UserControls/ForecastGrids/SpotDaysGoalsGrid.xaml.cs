using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// This grid is very similar to SpotWeekGoalsGrid, it just have days on the side, not weeks
    /// </summary>
    public partial class SpotDaysGoalsGrid : UserControl
    {

        CampaignDTO _campaign;
        int _version = 1;


        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();
        public MediaPlanController _mediaPlanController { get; set; }
        public MediaPlanTermController _mediaPlanTermController { get; set; }
        public SpotController _spotController { get; set; }
        public ChannelController _channelController { get; set; }

        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        private Dictionary<Char, int> _spotLengths = new Dictionary<char, int>();


        public SpotDaysGoalsGrid()
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
            ugDays.Children.Clear();
            ugSpots.Children.Clear();
            ugGrid.Children.Clear();


            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

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

            CreateOutboundHeaders();
            SetWidth();

        }

        private void CreateOutboundHeaders()
        {
            int firstDayNum = GetDayOfYear(startDate);
            int lastDayNum = GetDayOfYear(endDate);

            // Add headers
            // Days
            ugDays.Rows = lastDayNum - firstDayNum + 1 + 1; // last + 1 is for Header Total // add +1 to this +1
            for (int i = firstDayNum; i <= lastDayNum; i++)
            {

                System.Windows.Controls.Border border = new System.Windows.Controls.Border();
                border.BorderBrush = Brushes.Black;
                border.Background = Brushes.LightGoldenrodYellow;
                if (i == firstDayNum)
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
                textBlock.Text = startDate.AddDays(i - firstDayNum).ToShortDateString();

                border.Child = textBlock;

                ugDays.Children.Add(border);
            }

            AddDaysHeaderTotal();

            // Spots
            for (int i = firstDayNum; i < lastDayNum + 1 + 1; i++) // +1 because of Total row // add +1
            {
                for (int j = 0; j < _spots.Count; j++)
                {
                    System.Windows.Controls.Border border = new System.Windows.Controls.Border();
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
                    string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim();
                    textBlock.Text = label;

                    border.Child = textBlock;

                    ugSpots.Children.Add(border);
                }

            }



            //Channels
            ugChannels.Columns = (_channels.Count + 1);

            for (int i = 0; i < ugChannels.Columns; i++)
            {

                System.Windows.Controls.Border border = new System.Windows.Controls.Border();
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
                    System.Windows.Controls.Border border = new System.Windows.Controls.Border();
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

            // Grid Goals
            ugGrid.Columns = _channels.Count + 1; // 1 for total
            for (int i = 0; i < ugDays.Children.Count; i++)
            {
                DateTime date = startDate.AddDays(i);

                if (i == ugDays.Children.Count - 1)
                {
                    AddTotalSubGrids();
                    continue;
                }

                for (int j = 0; j < ugChannels.Columns; j++)
                {

                    // Filtering mediaPlans by channels 
                    ObservableCollection<MediaPlanTuple> channelMpTuples;

                    if (j == _channels.Count)
                    {
                        ObservableCollection<ObservableCollection<SpotGoals>> valuesList = new ObservableCollection<ObservableCollection<SpotGoals>>();
                        int n = ugGrid.Children.Count;
                        for (int k = 0; k < _channels.Count; k++)
                        {
                            SpotGoalsSubGrid sg = ugGrid.Children[n - 1 - k] as SpotGoalsSubGrid;
                            ObservableCollection<SpotGoals> dg = sg.Values;
                            valuesList.Add(dg);
                        }
                        var totalSubGrid = new SpotGoalsTotalSubGrid(valuesList);
                        totalSubGrid.row = i;
                        totalSubGrid.SelectedRowChanged += SubGrid_SelectedRowChanged;
                        ugGrid.Children.Add(totalSubGrid);
                        continue;
                    }
                    else
                    {
                        int channelId = _channels[j].chid;
                        channelMpTuples = new ObservableCollection<MediaPlanTuple>(_allMediaPlans.Where(tuple => tuple.MediaPlan.chid == channelId));
                    }

                    
                    

                    var mpTuples = new ObservableCollection<MediaPlanTuple>();
                    foreach (var mpTuple in channelMpTuples)
                    {
                        var allMpTerms = mpTuple.Terms;
                        ObservableCollection<MediaPlanTerm> mpTerms;

                        mpTerms = new ObservableCollection<MediaPlanTerm>(mpTuple.Terms.Where(t => t != null &&
                                  t.Date != null && t.Date == DateOnly.FromDateTime(date)));

                        
                        mpTuples.Add(new MediaPlanTuple(mpTuple.MediaPlan, mpTerms));
                    }
                    var subGrid = new SpotGoalsSubGrid(_spots, mpTuples);
                    subGrid.row = i;
                    subGrid.SelectedRowChanged += SubGrid_SelectedRowChanged;
                    ugGrid.Children.Add(subGrid);
                }
            }


        }

        private void AddTotalSubGrids()
        {
            for (int j = 0; j < ugChannels.Columns; j++)
            {

                if (j == _channels.Count)
                {
                    ObservableCollection<ObservableCollection<SpotGoals>> totalValuesList = new ObservableCollection<ObservableCollection<SpotGoals>>();
                    int m = ugGrid.Children.Count;
                    for (int k = 0; k < _channels.Count; k++)
                    {
                        SpotGoalsTotalSubGrid tsg = ugGrid.Children[m - 1 - k] as SpotGoalsTotalSubGrid;
                        ObservableCollection<SpotGoals> totalValues = tsg.Values;
                        totalValuesList.Add(totalValues);
                    }
                    var totalsSubGrid = new SpotGoalsTotalSubGrid(totalValuesList);
                    totalsSubGrid.row = ugDays.Children.Count - 1;
                    ugGrid.Children.Add(totalsSubGrid);
                }
                else
                {
                    ObservableCollection<ObservableCollection<SpotGoals>> valuesList = new ObservableCollection<ObservableCollection<SpotGoals>>();
                    int n = ugGrid.Children.Count;

                    for (int k = n - (_channels.Count + 1); k >= 0; k -= _channels.Count + 1)
                    {
                        SpotGoalsSubGrid sg = ugGrid.Children[k] as SpotGoalsSubGrid;
                        if (sg != null)
                        {
                            ObservableCollection<SpotGoals> values = sg.Values;
                            valuesList.Add(values);
                        }

                    }
                    var totalSubGrid = new SpotGoalsTotalSubGrid(valuesList);
                    totalSubGrid.row = ugDays.Children.Count - 1;
                    totalSubGrid.SelectedRowChanged += SubGrid_SelectedRowChanged;
                    ugGrid.Children.Add(totalSubGrid);
                }
            }
        }


        private void AddDaysHeaderTotal()
        {
            System.Windows.Controls.Border border = new System.Windows.Controls.Border();
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.Yellow;

            border.BorderThickness = new Thickness(1, 2, 1, 1);

            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Text = "Total";

            border.Child = textBlock;

            ugDays.Children.Add(border);
        }

        private int GetDayOfYear(DateTime date)
        {
            System.Globalization.Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            return calendar.GetDayOfYear(date);
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
            for (int i = 0, rowOffset = rowOff; i < ugDays.Rows; i++, rowOffset += offset)
            {
                // Get the range of cells to merge
                var range = worksheet.Cells[1 + rowOffset, 1 + colOff, offset + rowOffset, 1 + colOff];
                // Merge the cells
                range.Merge = true;

                range.Style.Border.Bottom.Style = drawingThickness;
                range.Style.Border.Bottom.Color.SetColor(drawingColor);
            }


            // Set the cell values and colors in Excel
            for (int rowIndex = 0; rowIndex < ugDays.Children.Count; rowIndex++)
            {
                string cellValue = string.Empty;

                var weekBorder = ugDays.Children[rowIndex] as System.Windows.Controls.Border;
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
                var cell = worksheet.Cells[rowIndex * offset + 1 + rowOff, colOff + 1];
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

                var spotBorder = ugSpots.Children[rowIndex] as System.Windows.Controls.Border;
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
                var border = ugChannels.Children[columnIndex] as System.Windows.Controls.Border;
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
                var border = ugGoals.Children[columnIndex] as System.Windows.Controls.Border;
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

        

    }
}
