﻿using CampaignEditor.Helpers;
using Database.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// Like spotGoalsSubGrid, but only with Total values
    /// </summary>
    public partial class SpotGoalsTotalSubGrid : UserControl
    {

        ObservableCollection<ObservableCollection<SpotGoals>> _valuesList;
        Dictionary<int, SpotGoals> _dictionary = new Dictionary<int, SpotGoals>();
        ObservableRangeCollection<SpotGoals> _values = new ObservableRangeCollection<SpotGoals>();

        public int row = -1;
        public ObservableCollection<SpotGoals> Values
        {
            get { return _values; }
        }
        public SpotGoalsTotalSubGrid(ObservableCollection<ObservableCollection<SpotGoals>> valuesList)
        {
            InitializeComponent();

            _valuesList = valuesList;
            int n = 0;
            if (_valuesList.Count > 0)
            {
                n = _valuesList[0].Count;
            }
            for (int i=0; i<n; i++)
            {
                _dictionary.Add(i, new SpotGoals());
            }
            SumValues();
            SubscribeToDataGrids();
        }

        public SpotGoalsTotalSubGrid(SpotGoalsTotalSubGrid tsg)
        {
            InitializeComponent();

            _valuesList = tsg._valuesList;
            _dictionary = tsg._dictionary;
            _values = tsg._values;

            dgGrid.Items.Clear();

            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;
            SubscribeToDataGrids();
        }

        private void SumValues() 
        {
            ResetDictionaryValues();
            foreach (ObservableCollection<SpotGoals> spotGoalsList in _valuesList)
            {
                int n = spotGoalsList.Count;
                for (int i=0; i<n; i++)
                {
                    SpotGoals spotGoals = spotGoalsList[i];
                    _dictionary[i].Insertations += spotGoals.Insertations;
                    _dictionary[i].Grp += spotGoals.Grp;
                    _dictionary[i].Budget += spotGoals.Budget;
                }
            }
            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;

        }

        private void ResetDictionaryValues()
        {
            foreach (SpotGoals spotGoal in _dictionary.Values)
            {
                spotGoal.Grp = 0;
                spotGoal.Insertations = 0;
                spotGoal.Budget = 0;
            }
        }

        public void SubscribeToDataGrids()
        {
           
            foreach (ObservableCollection<SpotGoals> spotGoalsList in _valuesList)
            {
                spotGoalsList.CollectionChanged += spotGoalsList_CollectionChanged;
            }
        }


        private void spotGoalsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SumValues();
        }

        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            var dataGrid = dgGrid;

            //Unselect all rows
            dataGrid.SelectedItem = null;

            // Get the visible columns from the DataGrid
            var columns = dataGrid.Columns.ToList();

            // Set the cell values and colors in Excel
            for (int rowIndex = 0; rowIndex < dataGrid.Items.Count; rowIndex++)
            {
                var dataItem = (SpotGoals)dataGrid.Items[rowIndex];
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    var column = columns[columnIndex];
                    var cellContent = column.GetCellContent(dataItem);
                    if (cellContent is TextBlock textBlock)
                    {
                        // Try to parse the text to a double
                        if (double.TryParse(textBlock.Text, out double numericValue))
                        {
                            // If successful, write the numeric value to the Excel cell
                            worksheet.Cells[rowIndex + 1 + rowOff, columnIndex + 1 + colOff].Value = numericValue;
                        }
                        else
                        {
                            // If not a valid number, write the text as it is
                            worksheet.Cells[rowIndex + 1 + rowOff, columnIndex + 1 + colOff].Value = textBlock.Text;
                        }

                    }
                    else
                    {
                        worksheet.Cells[rowIndex + 1 + rowOff, columnIndex + 1 + colOff].Value = "";
                    }

                    // Set the cell color
                    var cell = FindParentDataGridCell(cellContent as TextBlock) as DataGridCell;
                    if (cell != null)
                    {
                        var cellColor = cell.Background;
                        if (cellColor != null)
                        {
                            var excelColor = System.Drawing.ColorTranslator.FromHtml(cellColor.ToString());
                            var excelCell = worksheet.Cells[rowIndex + 1 + rowOff, columnIndex + 1 + colOff];
                            excelCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelCell.Style.Fill.BackgroundColor.SetColor(excelColor);

                            excelCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            excelCell.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            excelCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            excelCell.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                            excelCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            excelCell.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            excelCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            excelCell.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        }
                        double cellHeight = cell.ActualHeight;
                        double cellWidth = cell.ActualWidth / 7;

                        // Set the size of the Excel cell
                        worksheet.Row(rowIndex + 1 + rowOff).Height = cellHeight;
                        worksheet.Column(columnIndex + 1 + colOff).Width = cellWidth;
                        //worksheet.Row(rowIndex + 1 + rowOff).OutlineLevel = 2;

                    }

                }
            }


        }

        private DataGridCell FindParentDataGridCell(TextBlock textBlock)
        {
            if (textBlock == null)
                return null;

            DependencyObject parent = VisualTreeHelper.GetParent(textBlock);

            while (parent != null && !(parent is DataGridCell))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as DataGridCell;
        }

        public void SelectByRowIndex(int rowIndex)
        {
            dgGrid.SelectedIndex = rowIndex;
        }

        public void Unselect()
        {
            dgGrid.UnselectAll();
        }

        public event EventHandler<int> SelectedRowChanged;


        // Event handler for DataGrid's SelectedCellsChanged event
        private void dgGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // Get the index of the selected row
            int rowIndex = dgGrid.SelectedIndex;

            // Raise the custom event with the selected row index as the event argument
            SelectedRowChanged?.Invoke(this, rowIndex);
        }
   
    }
}
