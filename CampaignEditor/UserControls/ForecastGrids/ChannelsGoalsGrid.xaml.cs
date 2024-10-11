using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ChannelsGoalsGrid.xaml
    /// </summary>
    public partial class ChannelsGoalsGrid : UserControl
    {
        enum Data
        {
            Expected,
            Realized,
            ExpectedAndRealized
        }

        Data showData = Data.Expected;

        Dictionary<int, ProgramGoals> _dictionary = new Dictionary<int, ProgramGoals>();
        ObservableRangeCollection<ProgramGoals> _values = new ObservableRangeCollection<ProgramGoals>();
        ObservableCollection<MediaPlan> _mediaPlans;
        ObservableRangeCollection<MediaPlan> _visibleMediaPlans = new ObservableRangeCollection<MediaPlan>();
        List<ChannelDTO> _selectedChannels = new List<ChannelDTO>();
        public ObservableRangeCollection<MediaPlanRealized> _mpRealized;

        public MediaPlanForecastData _forecastData;
        public DateOnly startDate;
        public ChannelsGoalsGrid()
        {
            InitializeComponent();         
        }

        public void Initialize(ObservableCollection<MediaPlan> mediaPlans, List<ChannelDTO> channels)
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

        }

        public void SelectedChannelsChanged(IEnumerable<ChannelDTO> selectedChannels)
        {
            _selectedChannels.Clear();
            foreach (var channel in selectedChannels)
            {
                _selectedChannels.Add(channel);
            }
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

        private void CalculateGoals()
        {
            ResetDictionaryValues();
            var channelIds = _visibleMediaPlans.Select(mp => mp.chid).Distinct();
            if (showData == Data.Expected)
            {
                Parallel.ForEach(channelIds, chid =>
                {
                    var mediaPlans = _visibleMediaPlans.Where(mp => mp.chid == chid);
                    CalculateGoalsExpected(mediaPlans);
                });
            }
            else if (showData == Data.Realized)
            {
                Parallel.ForEach(channelIds, chid =>
                {
                    int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == chid).Key;
                    if (chrdsid.HasValue)
                    {
                        var mediaPlansRealized = _mpRealized.Where(mpr => mpr.chid == chrdsid && mpr.Status != null && mpr.Status != 5);
                        CalculateGoalsRealized(mediaPlansRealized);
                    }

                });
            }
            else if (showData == Data.ExpectedAndRealized)
            {
                /*DateOnly separationDate;
                var threeDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-3));
                if (threeDaysAgo < startDate)
                    separationDate = startDate;
                else
                    separationDate = threeDaysAgo;


                Parallel.ForEach(channelIds, chid =>
                {
                    var mediaPlans = _visibleMediaPlans.Where(mp => mp.chid == chid);
                    CalculateGoalsExpected(mediaPlans);
                });
                Parallel.ForEach(channelIds, chid =>
                {
                    int? chrdsid = _forecastData.ChrdsidChidDict.FirstOrDefault(dict => dict.Value == chid).Key;
                    if (chrdsid.HasValue)
                    {
                        var mediaPlansRealized = _mpRealized.Where(
                            mpr => mpr.chid == chrdsid && 
                            mpr.Status != null && 
                            mpr.Status != 5 &&
                            TimeFormat.YMDStringToDateOnly(mpr.Date) > separationDate);
                        CalculateGoalsRealized(mediaPlansRealized);
                    }

                });*/
            }

            _values.ReplaceRange(_dictionary.Values.Where(pg => _selectedChannels
                                            .Select(ch => ch.chid).Contains(pg.Channel.chid)));

        }

        private void CalculateGoalsExpected(IEnumerable<MediaPlan> mediaPlans)
        {
            int chid;
            if (mediaPlans.Count() > 0)
                chid = mediaPlans.ElementAt(0).chid;
            else
                return;

            foreach (MediaPlan mediaPlan in mediaPlans)
            {
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp1 += mediaPlan.Insertations * mediaPlan.Amrp1;
                _dictionary[chid].Grp2 += mediaPlan.Insertations * mediaPlan.Amrp2;
                _dictionary[chid].Grp3 += mediaPlan.Insertations * mediaPlan.Amrp3;
                _dictionary[chid].Budget += mediaPlan.price;
            }
        }

        private void CalculateGoalsRealized(IEnumerable<MediaPlanRealized> mediaPlansRealized)
        {
            int chid;
            if (mediaPlansRealized.Count() > 0)
            {
                int chrdsid = mediaPlansRealized.ElementAt(0).chid!.Value;
                if (_forecastData.ChrdsidChidDict.ContainsKey(chrdsid))
                    chid = _forecastData.ChrdsidChidDict[chrdsid];
                else
                    return;
            }
            else
                return;

            // CHECK WHAT TO DO IF WE HAVE NULL FOR AMRP? 
            foreach (MediaPlanRealized mpRealized in mediaPlansRealized)
            {
                _dictionary[chid].Insertations += 1;
                _dictionary[chid].Grp1 += mpRealized.Amrp1 ?? 0.0M;
                _dictionary[chid].Grp2 += mpRealized.Amrp2 ?? 0.0M;
                _dictionary[chid].Grp3 += mpRealized.Amrp3 ?? 0.0M;
                _dictionary[chid].Budget += mpRealized.price ?? 0.0M;
            }
        }

        public void VisibleTuplesChanged(IEnumerable<MediaPlanTuple> visibleMpTuples)
        {
            var visibleMediaPlans = visibleMpTuples.Select(mpTuple => mpTuple.MediaPlan);
            _visibleMediaPlans.ReplaceRange(visibleMediaPlans);
            CalculateGoals();
        }

        public void RecalculateGoalsExpected(int chid)
        {
            if (showData != Data.Expected)
                return;

            ResetDictionaryValues(chid);
            var mediaPlans = _visibleMediaPlans.Where(mp => mp.chid == chid);
            foreach (MediaPlan mediaPlan in mediaPlans)
            {
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp1 += mediaPlan.Insertations * mediaPlan.Amrp1;
                _dictionary[chid].Grp2 += mediaPlan.Insertations * mediaPlan.Amrp2;
                _dictionary[chid].Grp3 += mediaPlan.Insertations * mediaPlan.Amrp3;
                _dictionary[chid].Budget += mediaPlan.Price;           
            }
            _values.ReplaceRange(_dictionary.Values.Where(pg => _selectedChannels
                                            .Select(ch => ch.chid).Contains(pg.Channel.chid)));
        }

        public void RecalculateGoalsRealized(int chrdsid)
        {
            if (showData != Data.Realized)
                return;

            if (!_forecastData.ChrdsidChidDict.ContainsKey(chrdsid))
                return;

            int chid = _forecastData.ChrdsidChidDict[chrdsid];
            ResetDictionaryValues(chid);

            var mediaPlansRealized = _mpRealized.Where(mpr => mpr.chid == chrdsid && mpr.Status != 5 && mpr.Status != -1);

            // CHECK WHAT TO DO IF WE HAVE NULL FOR AMRP? 
            foreach (MediaPlanRealized mpRealized in mediaPlansRealized)
            {
                _dictionary[chid].Insertations += 1;
                _dictionary[chid].Grp1 += mpRealized.Amrp1 ?? 0.0M;
                _dictionary[chid].Grp2 += mpRealized.Amrp2 ?? 0.0M;
                _dictionary[chid].Grp3 += mpRealized.Amrp3 ?? 0.0M;
                _dictionary[chid].Budget += mpRealized.price ?? 0.0M;
            }

            _values.ReplaceRange(_dictionary.Values.Where(pg => _selectedChannels
                                            .Select(ch => ch.chid).Contains(pg.Channel.chid)));
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

        public void ChangeDataForShowing(string dataName)
        {
            Data data;
            switch (dataName)
            {
                case "expected": data = Data.Expected; break;
                case "realized": data = Data.Realized; break;
                case "expectedrealized": data = Data.ExpectedAndRealized; break;
                default: data = Data.Expected; break;
            }

            if (data == showData)
                return;

            showData = data;

            CalculateGoals();
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
