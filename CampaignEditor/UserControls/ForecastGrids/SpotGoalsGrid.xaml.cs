using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Border = System.Windows.Controls.Border;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Grid which have multiple headers and is used in showing goals of campaign 
    /// </summary>
    public partial class SpotGoalsGrid : UserControl
    {

        public MediaPlanController _mediaPlanController { get; set; }
        public MediaPlanTermController _mediaPlanTermController { get; set; }
        public SpotController _spotController { get; set; }
        public ChannelController _channelController { get; set; }

        // for checking if certain character can be written in spot cells
        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();

        public ObservableCollection<MediaPlanTuple> _allMediaPlans;

        CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        // for accessing from SpotWeekGoalsGrid class
        public CampaignDTO Campaign { get { return _campaign; } }
        public DateTime StartDate { get { return startDate; } }
        public DateTime EndDate { get { return endDate; } }
        public List<SpotDTO> Spots { get { return _spots; } }
        public List<ChannelDTO> Channels { get { return _channels; } }


        public SpotGoalsGrid()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {

            _campaign = campaign;

            _spots.Clear();
            _channels.Clear();
            ugChannels.Children.Clear();
            ugGoals.Children.Clear();
            ugWeeks.Children.Clear();
            dgSpots.Items.Clear();
            ugGrid.Children.Clear();
            

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            _spots = spots.ToList();
            _spots = _spots.OrderBy(s => s.spotcode).ToList();

            var channelIds = await _mediaPlanController.GetAllChannelsByCmpid(_campaign.cmpid);
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
            int firstWeekNum = GetWeekOfYear(startDate);
            int lastWeekNum = GetWeekOfYear(endDate);

            // Add headers
            // Weeks
            ugWeeks.Columns = lastWeekNum - firstWeekNum + 1 + 1; // last + 1 is for Header Total
            for (int i = firstWeekNum; i <= lastWeekNum; i++)
            {

                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                border.Background = Brushes.LightGoldenrodYellow;
                if (i == firstWeekNum)
                {
                    border.BorderThickness = new Thickness(3, 3, 3, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 3, 3, 1);
                }
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = "Week " + i.ToString();

                border.Child = textBlock;

                ugWeeks.Children.Add(border);
            }

            AddWeeksHeaderTotal();

            //Channels
            ugChannels.Columns = (_channels.Count + 1) * ugWeeks.Columns;
            for (int j = 0; j < ugWeeks.Columns; j++)
            {
                for (int i = 0; i < _channels.Count + 1; i++)
                {

                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    if (i == _channels.Count || j == ugWeeks.Columns - 1)
                    {
                        border.Background = Brushes.Yellow;
                    }
                    else
                    {
                        border.Background = Brushes.LightGoldenrodYellow;
                    }

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

            // Grid Goals
            for (int i = 0; i < ugWeeks.Children.Count; i++)
            {
                int weekNum = firstWeekNum + i;

                if (i == ugWeeks.Children.Count - 1)
                {
                    AddTotalSubGrids();
                    continue;
                }

                for (int j = 0; j <= _channels.Count; j++)
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

                        if (weekNum <= lastWeekNum)
                        {
                            mpTerms = new ObservableCollection<MediaPlanTerm>(mpTuple.Terms.Where(t => t != null &&
                                        GetWeekOfYear(t.Date.ToDateTime(TimeOnly.Parse("00:00 AM"))) == weekNum));
                        }
                        else
                        {
                            mpTerms = new ObservableCollection<MediaPlanTerm>(mpTuple.Terms.Where(t => t != null));
                        }
                        mpTuples.Add(new MediaPlanTuple(mpTuple.MediaPlan, mpTerms));
                    }
                    var subGrid = new SpotGoalsSubGrid(_spots, mpTuples);
                    subGrid.SelectedRowChanged += SubGrid_SelectedRowChanged;
                    ugGrid.Children.Add(subGrid);
                }
            }

            // Spots
            foreach (SpotDTO spot in _spots)
            {
                string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim();
                dgSpots.Items.Add(new SpotLabel { Label = label });
            }


        }

        public class SpotLabel
        {
            public string Label { get; set; }
        }

        private void AddTotalSubGrids()
        {
            for (int j = 0; j <= _channels.Count; j++)
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
                    totalSubGrid.SelectedRowChanged += SubGrid_SelectedRowChanged;
                    ugGrid.Children.Add(totalSubGrid);
                }
            }
        }

        private void AddWeeksHeaderTotal()
        {
            Border border = new Border();
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.Yellow;

            border.BorderThickness = new Thickness(1, 3, 3, 1);

            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = "Total";

            border.Child = textBlock;

            ugWeeks.Children.Add(border);
        }

        private int GetWeekOfYear(DateTime date)
        {
            System.Globalization.Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(
                date,
                CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                DayOfWeek.Monday // Use Monday as the first day of the week
            );
        }

        private void SetWidth()
        {
            int weeksNum = ugWeeks.Children.Count;
            int channelsNum = ugChannels.Children.Count;

            double weekWidth = 140;
            double headerWidth = weeksNum * weekWidth * (_channels.Count + 1);

            ugWeeks.Width = headerWidth;
            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
            ugGrid.Width = headerWidth;
            ugGrid.Height = dgSpots.Height;
        }

        #region Export to Excel
        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            AddHeadersInWorksheet(worksheet, rowOff, colOff);

            var rowOffset = rowOff + 3; // because of headers
            var colOffset = colOff + 1; // because of spots

            foreach (var subGrid in ugGrid.Children)
            {

                var sg = subGrid as SpotGoalsSubGrid;
                if (sg != null)
                {
                    sg.PopulateWorksheet(worksheet, rowOffset, colOffset);
                }
                else
                {
                    var sg2 = subGrid as SpotGoalsTotalSubGrid;
                    sg2.PopulateWorksheet(worksheet, rowOffset, colOffset);
                }
                colOffset += 3;
            }

        }

        private void AddHeadersInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            AddSpotsHeaderInWorksheet(worksheet, rowOff + 3, colOff);
            AddWeeksHeaderInWorksheet(worksheet, rowOff, colOff+1);
            AddChannelsHeaderInWorksheet(worksheet, rowOff + 1, colOff+1);
            AddGoalsHeaderInWorksheet(worksheet, rowOff + 2, colOff+1);
        }
        private void AddSpotsHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            // Set the cell values and colors in Excel
            for (int rowIndex = 0; rowIndex < dgSpots.Items.Count; rowIndex++)
            {                
                var spotLabel = dgSpots.Items[rowIndex] as SpotLabel;
                string cellValue = spotLabel.Label;
                worksheet.Cells[rowIndex + 1 + rowOff, colOff + 1].Value = cellValue;

                // Set the cell color
                var cell = worksheet.Cells[rowIndex + 1 + rowOff, colOff + 1];
                var drawingThickness = ExcelBorderStyle.Thin;
                var drawingColor = System.Drawing.Color.Black;

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

        private void AddWeeksHeaderInWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            // Merging cells
            int offset = (int)(ugGoals.Columns / ugWeeks.Columns);
            for (int i=0, colOffset = colOff; i<ugWeeks.Columns; i++, colOffset += offset)
            {
                // Get the range of cells to merge
                var range = worksheet.Cells[1 + rowOff, 1 + colOffset, 1 + rowOff, colOffset + offset];
                // Merge the cells
                range.Merge = true;
            }

            // Set the column headers in Excel
            for (int columnIndex = 0; columnIndex < ugWeeks.Columns; columnIndex++)
            {
                var border = ugWeeks.Children[columnIndex] as Border;
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

        // Helper method to find a parent of a specific type
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
                return null;

            T parentItem = parent as T;
            return parentItem ?? FindParent<T>(parent);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Get the clicked DataGridCell
            TextBlock textBlock = sender as TextBlock;

            // Find the parent DataGridCell
            DataGridCell cell = FindParent<DataGridCell>(textBlock);

            // Find the parent DataGridRow
            DataGridRow row = FindParent<DataGridRow>(cell);
            if (row == null)
                return;

            // Get the column index
            //int columnIndex = cell.Column.DisplayIndex;

            // Get the row index of the clicked cell
            int rowIndex = dgSpots.Items.IndexOf(row.Item);
            SelectAllSubgridRows(rowIndex);
            
        }

        private void SelectAllSubgridRows(int rowIndex)
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
                            tsg.SelectByRowIndex(rowIndex);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        sg.SelectByRowIndex(rowIndex);
                    }
                }
            }
        }

        // Event handler for the custom event in subGrids
        private void SubGrid_SelectedRowChanged(object sender, int rowIndex)
        {
            SelectAllSubgridRows(rowIndex);
        }
    }
}
