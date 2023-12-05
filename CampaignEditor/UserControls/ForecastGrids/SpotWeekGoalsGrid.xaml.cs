using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Border = System.Windows.Controls.Border;
using OfficeOpenXml.FormulaParsing;
using System.Windows.Interop;
using System.Windows.Input;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This grid is very similar to SpotGoalsGrid, so I'll pass it some SpotGoalsGrid, and make
    /// this grid based on that, on that way, there's no need to calculate same things more than once
    /// </summary>
    public partial class SpotWeekGoalsGrid : UserControl
    {

        CampaignDTO _campaign;
        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();
        public SpotWeekGoalsGrid()
        {
            InitializeComponent();
        }

        public void Initialize(SpotGoalsGrid sgGrid)
        {
            _campaign = sgGrid.Campaign;

            startDate = sgGrid.StartDate;
            endDate = sgGrid.EndDate;

            _spots.Clear();
            _channels.Clear();
            ugWeeks.Children.Clear();
            ugGoals.Children.Clear();
            ugChannels.Children.Clear();
            ugSpots.Children.Clear();
            ugGrid.Children.Clear();

            var spots = sgGrid.Spots;
            foreach (SpotDTO spot in spots)
            {
                _spots.Add(spot);
            }

            var channels = sgGrid.Channels;
            foreach (ChannelDTO channel in channels)
            {
                _channels.Add(channel);

            }

            CreateOutboundHeaders();
            AddGrid(sgGrid.ugGrid);
            SetWidth();
        }

        private void CreateOutboundHeaders()
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
        }

    }
}
