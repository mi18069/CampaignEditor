using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// Interaction logic for ChannelsGoalsGrid.xaml
    /// </summary>
    public partial class ChannelsGoalsGrid : UserControl
    {
        /*enum Data
        {
            Expected,
            Realized,
            ExpectedAndRealized
        }

        Data showData = Data.Expected;*/

        Dictionary<int, ProgramGoals> _dictionary = new Dictionary<int, ProgramGoals>();
        ObservableRangeCollection<ProgramGoals> _values = new ObservableRangeCollection<ProgramGoals>();
        /*ObservableCollection<MediaPlan> _mediaPlans;
        ObservableRangeCollection<MediaPlan> _visibleMediaPlans = new ObservableRangeCollection<MediaPlan>();*/
        public List<ChannelDTO> _selectedChannels;
        //public ObservableRangeCollection<MediaPlanRealized> _mpRealized;

        public MediaPlanForecastData _forecastData;
        public DateTime startDate;
        public DateTime endDate;
        public DateOnly SeparationDate { get; set; }
        public ChannelsGoalsGrid()
        {
            InitializeComponent();         
        }

        public void ConstructGrid(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            dgGrid.ItemsSource = _values;

        }

        /*public void Initialize(ObservableCollection<MediaPlan> mediaPlans, List<ChannelDTO> channels)
        {
            _mediaPlans = mediaPlans;
            _dictionary.Clear();
            _values.Clear();
            dgGrid.ItemsSource = _values;

            foreach (ChannelDTO channel in channels)
            {
                _dictionary.Add(channel.chid, new ProgramGoals(channel));
            }

            //CalculateGoals();
            //SubscribeToMediaPlans();

            if (_selectedChannels.Count > 0)
            {
                SelectedChannelsChanged(new List<ChannelDTO>(_selectedChannels));
            }

        }*/

        public void SelectedChannelsChanged(IEnumerable<ChannelDTO> selectedChannels)
        {
            /*_selectedChannels.Clear();
            foreach (var channel in selectedChannels)
            {
                _selectedChannels.Add(channel);
            }*/
            UpdateOrder(selectedChannels.ToList());
        }

        public void UpdateOrder(List<ChannelDTO> selectedChannels)
        {
            List<ProgramGoals> selectedInOrder = new List<ProgramGoals>();
            foreach (var channel in selectedChannels)
            {
                selectedInOrder.Add(_dictionary.First(kv => kv.Key == channel.chid).Value);
            }
  
            _values.ReplaceRange(selectedInOrder);
        }


        public void AssignDataValues(Dictionary<ChannelDTO, Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>> data)
        {
            TransformData(data);
        }

        private void TransformData(Dictionary<ChannelDTO, Dictionary<DateOnly, Dictionary<SpotDTO, SpotGoals>>> data)
        {

            foreach (var channelEntry in data)
            {
                var channel = channelEntry.Key;
                var dateDict = channelEntry.Value;
                int channelId = channel.chid;


                // Sum up SpotGoals for all dates and spots for the current channel
                var totalSpotGoalsBySpot = dateDict[DateOnly.FromDateTime(endDate.AddDays(1))];
                SpotGoals totalSpotGoals = new SpotGoals();
                foreach(var spotGoal in totalSpotGoalsBySpot)
                {
                    totalSpotGoals += spotGoal.Value;
                }

                // Add the accumulated SpotGoals to the result dictionary
                var programGoals = new ProgramGoals(channel);
                programGoals.Budget = totalSpotGoals.Budget;
                programGoals.Insertations = totalSpotGoals.Insertations;
                programGoals.Grp1 = totalSpotGoals.Grp;
                programGoals.Grp2 = totalSpotGoals.Grp2;
                programGoals.Grp3 = totalSpotGoals.Grp3;
                _dictionary[channelId] = programGoals;
            }
        }

        public void BindDataValues()
        {
            _values.ReplaceRange(_dictionary.Values.Where(pg => _selectedChannels
                                            .Select(ch => ch.chid).Contains(pg.Channel.chid)));
        }

       

        public void VisibleTuplesChanged(IEnumerable<MediaPlanTuple> visibleMpTuples)
        {
            /*var visibleMediaPlans = visibleMpTuples.Select(mpTuple => mpTuple.MediaPlan);
            _visibleMediaPlans.ReplaceRange(visibleMediaPlans);
            CalculateGoals();*/
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
                if (!_dictionary.ContainsKey(chid))
                    return;
                _dictionary[chid].Budget = 0;
                _dictionary[chid].Grp1 = 0;
                _dictionary[chid].Grp2 = 0;
                _dictionary[chid].Grp3 = 0;
                _dictionary[chid].Insertations = 0;
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

        private void AddChannel(ChannelDTO channel, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {
            var programGoals = _dictionary[channel.chid];

            var cellChannel = worksheet.Cells[rowOff, colOff];
            cellChannel.Value = channel.chname.Trim();

            var cellIns = worksheet.Cells[rowOff, colOff + 1];
            cellIns.Value = programGoals.Insertations;
            if (showAllDecimals)
            {
                var cellGrp1 = worksheet.Cells[rowOff, colOff + 2];
                cellGrp1.Value = programGoals.Grp1;

                var cellGrp2 = worksheet.Cells[rowOff, colOff + 3];
                cellGrp2.Value = programGoals.Grp2;

                var cellGrp3 = worksheet.Cells[rowOff, colOff + 4];
                cellGrp3.Value = programGoals.Grp3;

                var cellBud = worksheet.Cells[rowOff, colOff + 5];
                cellBud.Value = programGoals.Budget;
            }
            else
            {
                var cellGrp1 = worksheet.Cells[rowOff, colOff + 2];
                cellGrp1.Value = Math.Round(programGoals.Grp1);

                var cellGrp2 = worksheet.Cells[rowOff, colOff + 3];
                cellGrp2.Value = Math.Round(programGoals.Grp2);

                var cellGrp3 = worksheet.Cells[rowOff, colOff + 4];
                cellGrp3.Value = Math.Round(programGoals.Grp3);

                var cellBud = worksheet.Cells[rowOff, colOff + 5];
                cellBud.Value = Math.Round(programGoals.Budget, 2).ToString("#,##0.00");
            }
            
        }
        public void PopulateWorksheet(IEnumerable<ChannelDTO> selectedChannels, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1, bool showAllDecimals = false)
        {

            if (selectedChannels.Count() == 0)
                return;

            AddHeader(worksheet, rowOff, colOff);

            int i = 1;
            foreach (var channel in selectedChannels)
            {
                AddChannel(channel, worksheet, rowOff + i, colOff, showAllDecimals);
                i++;
            }

            
        }


    }
}
