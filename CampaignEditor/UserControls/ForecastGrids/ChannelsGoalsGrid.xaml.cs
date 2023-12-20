using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

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
        List<ChannelDTO> _selectedChannels = new List<ChannelDTO>();
        public ChannelsGoalsGrid()
        {
            InitializeComponent();         
        }

        public void Initialize(ObservableCollection<MediaPlan> mediaPlans, List<ChannelDTO> channels)
        {
            _mediaPlans = mediaPlans;
            _dictionary.Clear();

            foreach (ChannelDTO channel in channels)
            {
                _dictionary.Add(channel.chid, new ProgramGoals(channel));
            }

            CalculateGoals();
            SubscribeToMediaPlans();
        }

        public void SelectedChannelsChanged(IEnumerable<ChannelDTO> selectedChannels)
        {
            foreach (var channel in selectedChannels)
            {
                _selectedChannels.Insert(0, channel);
            }
            dgGrid.ItemsSource = _values.Where(pg => _selectedChannels.Select(ch => ch.chid).Contains(pg.Channel.chid));
        }

        private void CalculateGoals()
        {
            ResetDictionaryValues();
            foreach (MediaPlan mediaPlan in _mediaPlans)
            {
                int chid = mediaPlan.chid;
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp1 += mediaPlan.Insertations * mediaPlan.Amrp1;
                _dictionary[chid].Grp2 += mediaPlan.Insertations * mediaPlan.Amrp2;
                _dictionary[chid].Grp3 += mediaPlan.Insertations * mediaPlan.Amrp3;
                _dictionary[chid].Budget += mediaPlan.price;
            }

            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values.Where(pg => _selectedChannels.Select(ch => ch.chid).Contains(pg.Channel.chid));
        }

        private void RecalculateGoals(int chid)
        {
            ResetDictionaryValues(chid);
            var mediaPlans = _mediaPlans.Where(mp => mp.chid == chid);
            foreach (MediaPlan mediaPlan in mediaPlans)
            {
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp1 += mediaPlan.Insertations * mediaPlan.Amrp1;
                _dictionary[chid].Grp2 += mediaPlan.Insertations * mediaPlan.Amrp2;
                _dictionary[chid].Grp3 += mediaPlan.Insertations * mediaPlan.Amrp3;
                _dictionary[chid].Budget += mediaPlan.Price;           
            }
            _values.ReplaceRange(_dictionary.Values);
        }

        public void ResetDictionaryValues(int chid = -1)
        {
            if (chid == -1)
            {
                foreach (ProgramGoals programGoal in _dictionary.Values)
                {
                    programGoal.Grp1 = 0;
                    programGoal.Grp2 = 0;
                    programGoal.Grp3 = 0;
                    programGoal.Insertations = 0;
                    programGoal.Budget = 0;
                }
            }
            else
            {
                _dictionary[chid].Budget = 0;
                _dictionary[chid].Grp1 = 0;
                _dictionary[chid].Grp2 = 0;
                _dictionary[chid].Grp3 = 0;
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

        private void AddHeader(ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            // Set the column headers in Excel 
            List<ExcelRange> cells = new List<ExcelRange>();

            var cellChannel = worksheet.Cells[rowOff, colOff];
            cellChannel.Value = "Channel";
            cells.Add(cellChannel);

            var cellIns = worksheet.Cells[rowOff, colOff + 1];
            cellIns.Value = "INS";
            cells.Add(cellIns);

            var cellGrp1 = worksheet.Cells[rowOff, colOff + 2];
            cellGrp1.Value = "GRP 1";
            cells.Add(cellGrp1);

            var cellGrp2 = worksheet.Cells[rowOff, colOff + 3];
            cellGrp2.Value = "GRP 2";
            cells.Add(cellGrp2);

            var cellGrp3 = worksheet.Cells[rowOff, colOff + 4];
            cellGrp3.Value = "GRP 3";
            cells.Add(cellGrp3);

            var cellBud = worksheet.Cells[rowOff, colOff + 5];
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

        private void AddChannel(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var programGoals = _dictionary[channel.chid];

            var cellChannel = worksheet.Cells[rowOff, colOff];
            cellChannel.Value = channel.chname.Trim();

            var cellIns = worksheet.Cells[rowOff, colOff + 1];
            cellIns.Value = programGoals.Insertations;

            var cellGrp1 = worksheet.Cells[rowOff, colOff + 2];
            cellGrp1.Value = programGoals.Grp1;

            var cellGrp2 = worksheet.Cells[rowOff, colOff + 3];
            cellGrp2.Value = programGoals.Grp2;

            var cellGrp3 = worksheet.Cells[rowOff, colOff + 4];
            cellGrp3.Value = programGoals.Grp3;

            var cellBud = worksheet.Cells[rowOff, colOff + 5];
            cellBud.Value = programGoals.Budget;
        }
        public void PopulateWorksheet(IEnumerable<ChannelDTO> selectedChannels, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {

            if (selectedChannels.Count() == 0)
                return;

            AddHeader(worksheet, rowOff, colOff);

            int i = 1;
            foreach (var channel in selectedChannels)
            {
                AddChannel(channel, worksheet, rowOff + i, colOff);
                i++;
            }

            
        }


    }
}
