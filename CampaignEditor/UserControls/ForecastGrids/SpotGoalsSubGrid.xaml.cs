﻿using CampaignEditor.Helpers;
using Database.DTOs.SpotDTO;
using Database.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// This is part of dataGrid which contains only datas for one channel and week
    /// </summary>
    public partial class SpotGoalsSubGrid : UserControl
    {

        private Dictionary<Char, SpotGoals> _dictionary = new Dictionary<char, SpotGoals>();
        private ObservableRangeCollection<SpotGoals> _values = new ObservableRangeCollection<SpotGoals>();
        private ObservableCollection<MediaPlanTuple> _mpTuples;
        private Dictionary<Char, int> _spotLengths = new Dictionary<char, int>();

        public int row = -1;
        public Dictionary<Char, SpotGoals> Dict
        {
            get { return _dictionary; }
        }

        public ObservableCollection<SpotGoals> Values
        {
            get { return _values; }
        }

        public SpotGoalsSubGrid(IEnumerable<SpotDTO> spots, ObservableCollection<MediaPlanTuple> mpTuples)
        {
            InitializeComponent();
            _mpTuples = mpTuples;
            _dictionary.Clear();
            _spotLengths.Clear();
            dgGrid.Items.Clear();
            
            foreach (var spot in spots)
            {
                _dictionary.Add(spot.spotcode[0], new SpotGoals());
                _spotLengths.Add(spot.spotcode[0], spot.spotlength);
            }
            _dictionary.OrderBy(kv => kv.Key);
            CalculateGoals();
            SubscribeToMediaPlanTerms();
            dgGrid.ItemsSource = _dictionary.Values;
        }
        public SpotGoalsSubGrid(SpotGoalsSubGrid sg)
        {
            InitializeComponent();

            this._mpTuples = sg._mpTuples; 
            _dictionary = sg._dictionary;
            _values = sg._values;
            _spotLengths = sg._spotLengths;

            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;
            SubscribeToMediaPlanTerms();
        }

        private void CalculateGoals()
        {
            ResetDictionaryValues();
            foreach (var mpTuple in _mpTuples)
            {
                var mediaPlan = mpTuple.MediaPlan;
                foreach (var mpTerm in mpTuple.Terms)
                {
                    var spotcodes = mpTerm.Spotcode;

                    if (spotcodes != null && spotcodes.Length > 0)
                    {
                        foreach (char spotcode in spotcodes)
                        {
                            if (spotcode != ' ')
                            {
                                try
                                {
                                    _dictionary[spotcode].Insertations += 1;
                                    _dictionary[spotcode].Budget += (mediaPlan.Price / mediaPlan.Length) * _spotLengths[spotcode];
                                    _dictionary[spotcode].Grp += mediaPlan.Amrp1;
                                }
                                catch
                                {

                                }
                            }

                        }
                    }
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


        public void SubscribeToMediaPlanTerms()
        {
            foreach (MediaPlanTuple mediaPlanTuple in _mpTuples)
            {
                foreach (MediaPlanTerm mpTerm in mediaPlanTuple.Terms)
                {
                    mpTerm.PropertyChanged += MediaPlanTerm_PropertyChanged;
                }
            }
        }

        private void MediaPlanTerm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle the changes in the MediaPlanTerm attributes here
            CalculateGoals();

        }

        #region Export to Excel
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

        #endregion

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
