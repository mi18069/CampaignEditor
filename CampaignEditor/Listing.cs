using CampaignEditor.StartupHelpers;
using Database.Entities;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using Database.DTOs.ChannelDTO;
using Database.DTOs.CampaignDTO;
using Database.DTOs.SpotDTO;
using Microsoft.VisualBasic;

namespace CampaignEditor
{
    public class Listing
    {
        private MediaPlanConverter _mpConverter;
        private MediaPlanTermConverter _mpTermConverter;

        public List<ChannelDTO> selectedChannels = new List<ChannelDTO>();
        public List<MediaPlanTuple> visibleTuples = new List<MediaPlanTuple>();

        private Dictionary<int, ChannelDTO> _chidChannelDictionary = new Dictionary<int, ChannelDTO>();
        private Dictionary<char, SpotDTO> _spotcodeSpotDictionary;

        CampaignDTO _campaign;


        public Listing(IAbstractFactory<MediaPlanTermConverter> factoryMpTermConverter)
        {
            _mpTermConverter = factoryMpTermConverter.Create();
        }

        public void Initialize(CampaignDTO campaign, IEnumerable<ChannelDTO> channels,
            Dictionary<char, SpotDTO> spotcodeSpotDictionary,
            MediaPlanConverter mpConverter)
        {
            _campaign = campaign;
            _mpConverter = mpConverter;

            _spotcodeSpotDictionary = spotcodeSpotDictionary;

            _chidChannelDictionary.Clear();
            foreach (var channel in channels)
            {
                if (!_chidChannelDictionary.ContainsKey(channel.chid))
                {
                    _chidChannelDictionary[channel.chid] = channel;
                }
            }
        }

        private bool[] TransformVisibleColumns(bool[] visibleGridColumns)
        {
            bool[] visibleColumns = new bool[34];
            visibleColumns[0] = true;
            /*for (int i = 0; i < 18; i++)
            {
                visibleColumns[i + 1] = visibleGridColumns[i];
            }
            // Skipping Amr sale columns
            for (int i = 21; i < 26; i++)
            {
                visibleColumns[i] = visibleGridColumns[i];
            }*/
            for (int i = 0; i < 25; i++)
            {
                visibleColumns[i + 1] = visibleGridColumns[i];
            }
            // Skipping CPP and Ins
            visibleColumns[26] = visibleGridColumns[28]; // CPSP
            for (int i=27; i<34; i++)
            {
                visibleColumns[i] = true;
            }

            return visibleColumns;
        }
        // if terms to print == 0 => print all, 1 => print only added, 2 => print only deleted
        public void PopulateWorksheet(List<MediaPlanTuple> mpTuples, bool[] visibleGridColumns, ExcelWorksheet worksheet, DateTime startDate, DateTime endDate, int rowOff = 1, int colOff = 1, bool showAllDecimals = false, int termsToPrint = 0)
        {
            mpTuples = mpTuples.OrderBy(mpt => mpt.MediaPlan.chid).ThenBy(mpt => mpt.MediaPlan.stime).ToList();


            bool[] visibleColumns = TransformVisibleColumns(visibleGridColumns);
            int visibleColumnsNum = visibleColumns.Count(v => v);

            AddHeaders(worksheet, visibleColumns, rowOff, colOff);

            int rowOffset = 1;
            int dateIndex = 0;
            var firstDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate).Date;
            while (firstDate < startDate)
            {
                dateIndex++;
                firstDate = firstDate.AddDays(1);
            }

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                bool first = true;
                foreach (var mpTuple in mpTuples)
                {
                    var mediaPlan = mpTuple.MediaPlan;
                    var term = mpTuple.Terms[dateIndex];
                    if (term == null)
                        continue;
                    string? spotcode = "";
                    switch (termsToPrint)
                    {
                        case 0: spotcode = term.Spotcode; break;
                        case 1: spotcode = term.Added; break;
                        case 2: spotcode = term.Deleted; break;
                        default: continue;
                    }
                    if (string.IsNullOrWhiteSpace(spotcode))
                    {
                        continue;
                    }
                    if (first)
                    {
                        var excelCell = worksheet.Cells[rowOff + rowOffset, colOff];
                        excelCell.Value = DateOnly.FromDateTime(date);
                        excelCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        excelCell.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);

                        first = false;
                        rowOffset++;
                    }

                    foreach (char code in spotcode.Trim())
                    {
                        AddSpotcode(worksheet, mediaPlan, term, code, visibleColumns, rowOff + rowOffset, colOff, showAllDecimals);                          
                        rowOffset += 1;
                    }                  
                }

                dateIndex += 1;
            }

        }

        private void AddHeaders(ExcelWorksheet worksheet, bool[] visibleColumns, int rowOff, int colOff)
        {
            List<string> columnHeaders = new List<string> { "Date", "Channel", "Program", "Day Part", "Position", "Start time",
            "End time", "Block time", "Type", "Special", "Amr1", "Amr% 1", "Amr1 Trim", "Amr2", "Amr% 2", "Amr2 Trim", "Amr3",
            "Amr% 3","Amr3 Trim", "Amr Sale", "Amr Sale%", "Amr Sale Trim", "Affinity", "Ch coef", "Prog coef", "Dp coef", "Seas coef", "Sec coef", "Coef A", "Coef B", "CPSP",  "Length", "Price", "Spot"};

            int numOfColumns = visibleColumns.Count();

            // Set the column headers in Excel
            int colOffset = 0;
            for (int i = 0; i < numOfColumns; i++)
            {
                if (visibleColumns[i])
                {
                    worksheet.Cells[rowOff, colOff + colOffset].Value = columnHeaders[i];
                    worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                    colOffset += 1;
                }
            }
        }
        private async void AddSpotcode(ExcelWorksheet worksheet, MediaPlan mediaPlan, MediaPlanTerm term, char spotcode, bool[] visibleColumns, int rowOff, int colOff, bool showAllDecimals = false)
        {
            int colOffset = 0;
            if (visibleColumns[0])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = term.Date;
                colOffset += 1;
            }
            if (visibleColumns[1])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = _chidChannelDictionary[mediaPlan.chid].chname;
                colOffset += 1;
            }
            if (visibleColumns[2])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Name;               
                colOffset += 1;
            }
            if (visibleColumns[3])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.DayPart == null ? "" : 
                    mediaPlan.DayPart.name.Trim();
                colOffset += 1;
            }
            if (visibleColumns[4])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Position;               
                colOffset += 1;
            }
            if (visibleColumns[5])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Stime;               
                colOffset += 1;
            }
            if (visibleColumns[6])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Etime;               
                colOffset += 1;
            }
            if (visibleColumns[7])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Blocktime ?? "";               
                colOffset += 1;
            }
            if (visibleColumns[8])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Type;               
                colOffset += 1;
            }
            if (visibleColumns[9])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Special;               
                colOffset += 1;
            }
            if (visibleColumns[10])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr1;               
                colOffset += 1;
            }
            if (visibleColumns[11])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrp1;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrp1, 2);               
                colOffset += 1;
            }
            if (visibleColumns[12])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr1trim;                
                colOffset += 1;
            }
            if (visibleColumns[13])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr2;                
                colOffset += 1;
            }
            if (visibleColumns[14])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrp2;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrp2, 2);               
                colOffset += 1;
            }
            if (visibleColumns[15])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr2trim;                
                colOffset += 1;
            }
            if (visibleColumns[16])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr3;              
                colOffset += 1;
            }
            if (visibleColumns[17])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrp3;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrp3, 2);                
                colOffset += 1;
            }
            if (visibleColumns[18])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr3trim;
                colOffset += 1;
            }
            if (visibleColumns[19])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrsale;
                colOffset += 1;
            }
            if (visibleColumns[20])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrpsale;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrpsale, 2);
                colOffset += 1;
            }
            if (visibleColumns[21])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrsaletrim;
                colOffset += 1;
            }
            if (visibleColumns[22])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Affinity;
                colOffset += 1;
            }
            if (visibleColumns[23])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Chcoef;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Chcoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[24])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Progcoef;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Progcoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[25])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Dpcoef;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Dpcoef, 2);
                colOffset += 1;
            }

            if (visibleColumns[26])
            {
                decimal seascoef = _mpConverter.CalculateTermSeascoef(mediaPlan, _mpTermConverter.ConvertToDTO(term));
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = seascoef;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(seascoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[27])
            {
                decimal seccoef = _mpConverter.CalculateTermSeccoef(mediaPlan, _spotcodeSpotDictionary[spotcode]);
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = seccoef;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(seccoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[28])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.CoefA;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.CoefA, 2);
                colOffset += 1;
            }
            if (visibleColumns[29])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.CoefB;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.CoefB, 2);
                colOffset += 1;
            }
            if (visibleColumns[30])
            {
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.PricePerSecond;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.PricePerSecond, 2);
                colOffset += 1;
            }
            if (visibleColumns[31])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = (_spotcodeSpotDictionary[spotcode] as SpotDTO).spotlength;
                colOffset += 1;
            }
            if (true)
            {
                var termCoefs = new TermCoefs();
                _mpConverter.GetProgramSpotPrice(mediaPlan, term, _spotcodeSpotDictionary[spotcode] as SpotDTO, termCoefs);
                if (showAllDecimals)
                    worksheet.Cells[rowOff, colOff + colOffset].Value = termCoefs.Price;
                else
                    worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(termCoefs.Price, 2).ToString("#,##0.00");
                colOffset += 1;
            }
            if (visibleColumns[32])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = spotcode;
                colOffset += 1;
            }


        }
    }
}
