using CampaignEditor.StartupHelpers;
using Database.Entities;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using Database.DTOs.ChannelDTO;
using CampaignEditor.Controllers;
using Database.Repositories;
using Database.DTOs.CampaignDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.PricelistDTO;

namespace CampaignEditor
{
    public class Listing
    {
        private MediaPlanConverter _mpConverter;
        private MediaPlanTermConverter _mpTermConverter;

        private ChannelController _channelController;
        private ChannelCmpController _channelCmpController;
        private SpotController _spotController;
        private PricelistController _pricelistController;

        public List<ChannelDTO> selectedChannels = new List<ChannelDTO>();
        public List<MediaPlanTuple> visibleTuples = new List<MediaPlanTuple>();

        private Dictionary<int, ChannelDTO> _chidChannelDictionary = new Dictionary<int, ChannelDTO>();
        private Dictionary<int, PricelistDTO> _chidPricelistDictionary;
        private Dictionary<char, SpotDTO> _spotcodeSpotDictionary;

        CampaignDTO _campaign;


        public Listing(IAbstractFactory<MediaPlanConverter> factoryMpConverter,
            IAbstractFactory<MediaPlanTermConverter> factoryMpTermConverter,
            IChannelRepository channelRepository, IChannelCmpRepository channelCmpRepository,
            ISpotRepository spotRepository, IPricelistChannelsRepository pricelistChannelsRepository,
            IPricelistRepository pricelistRepository)
        {
            _mpTermConverter = factoryMpTermConverter.Create();

            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _spotController = new SpotController(spotRepository);
            _pricelistController = new PricelistController(pricelistRepository);
        }

        public void Initialize(CampaignDTO campaign, IEnumerable<ChannelDTO> channels,
            Dictionary<int, PricelistDTO> chidPricelistDictionary, 
            Dictionary<char, SpotDTO> spotcodeSpotDictionary,
            MediaPlanConverter mpConverter)
        {
            _campaign = campaign;
            _mpConverter = mpConverter;

            _chidPricelistDictionary = chidPricelistDictionary;
            _spotcodeSpotDictionary = spotcodeSpotDictionary;

            _chidChannelDictionary.Clear();
            foreach (var channel in channels)
            {
                if (!_chidChannelDictionary.ContainsKey(channel.chid))
                {
                    _chidChannelDictionary[channel.chid] = channel;
                }
            }
            /*_chidChannelDictionary.Clear();
            _chidPricelistDictionary.Clear();
            foreach (var channel in channels)
            {
                try
                {
                    _chidChannelDictionary.Add(channel.chid, channel);
                    var channelCmp = await _channelCmpController.GetChannelCmpByIds(_campaign.cmpid, channel.chid);
                    var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                    _chidPricelistDictionary.Add(channel.chid, pricelist);
                }
                catch { }
            }


            _spotcodeSpotDictionary.Clear();
            foreach (var spot in spots)
            {
                _spotcodeSpotDictionary.Add(spot.spotcode.Trim()[0], spot);
            }*/
        }

        private bool[] TransformVisibleColumns(bool[] visibleGridColumns)
        {
            bool[] visibleColumns = new bool[27];
            visibleColumns[0] = true;
            for (int i = 0; i < 18; i++)
            {
                visibleColumns[i + 1] = visibleGridColumns[i];
            }
            // Skipping Amr sale columns
            for (int i = 21; i < 26; i++)
            {
                visibleColumns[i] = visibleGridColumns[i];
            }
            // Skipping CPP and Ins
            visibleColumns[26] = visibleGridColumns[28]; // CPSP

            return visibleColumns;
        }

        public void PopulateWorksheet(List<MediaPlanTuple> mpTuples, bool[] visibleGridColumns, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            mpTuples = mpTuples.OrderBy(mpt => mpt.MediaPlan.chid).ThenBy(mpt => mpt.MediaPlan.stime).ToList();
            DateTime startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            DateTime endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            bool[] visibleColumns = TransformVisibleColumns(visibleGridColumns);
            int visibleColumnsNum = visibleColumns.Count(v => v);

            AddHeaders(worksheet, visibleColumns, rowOff, colOff);

            int rowOffset = 1;
            int dateIndex = 0;
            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                bool first = true;
                foreach (var mpTuple in mpTuples)
                {
                    var mediaPlan = mpTuple.MediaPlan;
                    var term = mpTuple.Terms[dateIndex];
                    if (term == null || term.Spotcode == null || term.Spotcode.Trim().Count() == 0)
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

                    foreach (char spotcode in term.Spotcode.Trim())
                    {
                        AddSpotcode(worksheet, mediaPlan, term, spotcode, visibleColumns, rowOff + rowOffset, colOff);                          
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
            "Amr% 3","Amr3 Trim","Affinity","Prog coef", "Dp coef", "Seas coef", "Sec coef", "CPSP",  "Length", "Spot"};

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
        private void AddSpotcode(ExcelWorksheet worksheet, MediaPlan mediaPlan, MediaPlanTerm term, char spotcode, bool[] visibleColumns, int rowOff, int colOff)
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
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Affinity;
                colOffset += 1;
            }
            if (visibleColumns[20])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Progcoef;
                colOffset += 1;
            }
            if (visibleColumns[21])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Dpcoef;
                colOffset += 1;
            }

            if (visibleColumns[22])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = _mpConverter.CalculateTermSeascoef(mediaPlan, _chidPricelistDictionary[mediaPlan.chid], _mpTermConverter.ConvertToDTO(term));
                colOffset += 1;
            }
            if (visibleColumns[23])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = _mpConverter.CalculateTermSeccoef(mediaPlan, _chidPricelistDictionary[mediaPlan.chid], _spotcodeSpotDictionary[spotcode]);
                colOffset += 1;
            }
            if (visibleColumns[24])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.PricePerSecond;
                colOffset += 1;
            }
            if (visibleColumns[25])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = (_spotcodeSpotDictionary[spotcode] as SpotDTO).spotlength;
                colOffset += 1;
            }
            if (visibleColumns[26])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = spotcode;
                colOffset += 1;
            }


        }
    }
}
