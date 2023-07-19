﻿using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
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

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ChannelsGoalsGrid.xaml
    /// </summary>
    public partial class ChannelsGoalsGrid : UserControl
    {
        Dictionary<int, ProgramGoals> _dictionary = new Dictionary<int, ProgramGoals>();
        ObservableRangeCollection<ProgramGoals> _values = new ObservableRangeCollection<ProgramGoals>();
        ObservableCollection<MediaPlan> _mediaPlans;

        public ChannelsGoalsGrid()
        {
            InitializeComponent();         
        }

        public void Initialize(ObservableCollection<MediaPlan> mediaPlans, List<ChannelDTO> channels)
        {
            _mediaPlans = mediaPlans;
            foreach (ChannelDTO channel in channels)
            {
                _dictionary.Add(channel.chid, new ProgramGoals(channel));
            }

            CalculateGoals();
            SubscribeToMediaPlans();
        }

        private void CalculateGoals()
        {
            ResetDictionaryValues();
            foreach (MediaPlan mediaPlan in _mediaPlans)
            {
                int chid = mediaPlan.chid;
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp += mediaPlan.Insertations * (mediaPlan.Amrp1 + mediaPlan.Amrp2 + mediaPlan.Amrp3);
                _dictionary[chid].Budget += mediaPlan.price;
            }

            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;
        }

        private void RecalculateGoals(int chid)
        {
            ResetDictionaryValues(chid);
            var mediaPlans = _mediaPlans.Where(mp => mp.chid == chid);
            foreach (MediaPlan mediaPlan in mediaPlans)
            {
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp += mediaPlan.Insertations * (mediaPlan.Amrp1 + mediaPlan.Amrp2 + mediaPlan.Amrp3);
                _dictionary[chid].Budget += mediaPlan.price;           
            }
            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;
        }

        private void ResetDictionaryValues(int chid = -1)
        {
            if (chid == -1)
            {
                foreach (ProgramGoals programGoal in _dictionary.Values)
                {
                    programGoal.Grp = 0;
                    programGoal.Insertations = 0;
                    programGoal.Budget = 0;
                }
            }
            else
            {
                _dictionary[chid].Budget = 0;
                _dictionary[chid].Grp = 0;
                _dictionary[chid].Insertations = 0;
            }
        }

        public void SubscribeToMediaPlans()
        {
            foreach (MediaPlan mediaPlan in _mediaPlans)
            { 
                mediaPlan.PropertyChanged += MediaPlan_PropertyChanged;
            }
        }

        private void MediaPlan_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle the changes in the MediaPlanTerm attributes here
            var mp = sender as MediaPlan;
            if (mp != null)
            {
                RecalculateGoals(mp.chid);
            }

        }

        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            var dataGrid = dgGrid;

            //Unselect all rows
            dataGrid.SelectedItem = null;

            // Get the visible columns from the DataGrid
            var columns = dataGrid.Columns;

            // Set the column headers in Excel
            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Value = column.Header;
                worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            }
            //Set width of first column
            worksheet.Column(0 + 1 + colOff).Width = 30;

            // Set the cell values and colors in Excel
            for (int rowIndex = 0; rowIndex < dataGrid.Items.Count; rowIndex++)
            {
                var dataItem = (ProgramGoals)dataGrid.Items[rowIndex];

                worksheet.Cells[rowIndex + 2 + rowOff, 0 + 1 + colOff].Value = dataItem.Channel.chname.Trim();
                worksheet.Cells[rowIndex + 2 + rowOff, 1 + 1 + colOff].Value = dataItem.Insertations;
                worksheet.Cells[rowIndex + 2 + rowOff, 2 + 1 + colOff].Value = dataItem.Grp;
                worksheet.Cells[rowIndex + 2 + rowOff, 3 + 1 + colOff].Value = dataItem.Budget;

                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    var excelCell = worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff];

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
    }
}
