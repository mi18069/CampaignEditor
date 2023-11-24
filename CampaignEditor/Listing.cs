using CampaignEditor.StartupHelpers;
using Database.Entities;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.DTOs.ChannelDTO;
using System.Threading.Channels;
using CampaignEditor.Controllers;
using Database.Repositories;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.PricelistDTO;
using CampaignEditor.Helpers;

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

        private Dictionary<int, string> _chidChannelDictionary = new Dictionary<int, string>();
        private Dictionary<int, PricelistDTO> _chidPricelistDictionary = new Dictionary<int, PricelistDTO>();
        private Dictionary<char, SpotDTO> _spotcodeSpotDictionary = new Dictionary<char, SpotDTO>();

        CampaignDTO _campaign;


        public Listing(IAbstractFactory<MediaPlanConverter> factoryMpConverter,
            IAbstractFactory<MediaPlanTermConverter> factoryMpTermConverter,
            IChannelRepository channelRepository, IChannelCmpRepository channelCmpRepository,
            ISpotRepository spotRepository, IPricelistChannelsRepository pricelistChannelsRepository,
            IPricelistRepository pricelistRepository)
        {
            _mpConverter = factoryMpConverter.Create();
            _mpTermConverter = factoryMpTermConverter.Create();

            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _spotController = new SpotController(spotRepository);
            _pricelistController = new PricelistController(pricelistRepository);
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;

            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            _chidChannelDictionary.Clear();
            _chidPricelistDictionary.Clear();
            foreach (var channelCmp in channelCmps)
            {
                var channel = await _channelController.GetChannelById(channelCmp.chid);
                _chidChannelDictionary.Add(channel.chid, channel.chname.Trim());
                var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                _chidPricelistDictionary.Add(channel.chid, pricelist);
            }


            _spotcodeSpotDictionary.Clear();
            var spots = await _spotController.GetSpotsByCmpid(campaign.cmpid);
            foreach (var spot in spots)
            {
                _spotcodeSpotDictionary.Add(spot.spotcode.Trim()[0], spot);
            }
        }
        public async Task PopulateWorksheet(List<MediaPlanTuple> mpTuples, ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            mpTuples = mpTuples.OrderBy(mpt => mpt.MediaPlan.chid).ThenBy(mpt => mpt.MediaPlan.stime).ToList();

            List<string> columnHeaders = new List<string> { "Channel", "Program", "Position", "Start time",
            "End time", "Block time", "Amr1", "Amr% 1", "Amr1 Trim", "Prog coef", "Dp coef", "Seas coef", "Sec coef", "Date", "Spot"};

            int numOfColumns = columnHeaders.Count;

            // Set the column headers in Excel
            for (int columnIndex = 0; columnIndex < numOfColumns; columnIndex++)
            {
                worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Value = columnHeaders[columnIndex];
                worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            }

            // Set the cell values and colors in Excel

            int rowIndex = 0;
            foreach (var mpTuple in mpTuples)
            {
                var mediaPlan = mpTuple.MediaPlan;
                var terms = mpTuple.Terms.Where(term => term != null && term.Spotcode != null && term.Spotcode.Trim().Count() > 0);
                foreach (var term in terms)
                {
                    foreach (char spotcode in term.Spotcode.Trim())
                    {
                        var a = rowIndex + 2 + rowOff;
                        worksheet.Cells[rowIndex + 2 + rowOff, 0 + 1 + colOff].Value = _chidChannelDictionary[mediaPlan.chid];
                        worksheet.Cells[rowIndex + 2 + rowOff, 1 + 1 + colOff].Value = mediaPlan.Name;
                        worksheet.Cells[rowIndex + 2 + rowOff, 2 + 1 + colOff].Value = mediaPlan.Position;
                        worksheet.Cells[rowIndex + 2 + rowOff, 3 + 1 + colOff].Value = mediaPlan.Stime;
                        worksheet.Cells[rowIndex + 2 + rowOff, 4 + 1 + colOff].Value = mediaPlan.Etime;
                        worksheet.Cells[rowIndex + 2 + rowOff, 5 + 1 + colOff].Value = mediaPlan.Blocktime ?? mediaPlan.Stime;
                        worksheet.Cells[rowIndex + 2 + rowOff, 6 + 1 + colOff].Value = mediaPlan.Amr1;
                        worksheet.Cells[rowIndex + 2 + rowOff, 7 + 1 + colOff].Value = Math.Round(mediaPlan.Amrp1, 2);
                        worksheet.Cells[rowIndex + 2 + rowOff, 8 + 1 + colOff].Value = mediaPlan.Amr1trim;
                        worksheet.Cells[rowIndex + 2 + rowOff, 9 + 1 + colOff].Value = mediaPlan.Progcoef;
                        worksheet.Cells[rowIndex + 2 + rowOff, 10 + 1 + colOff].Value = mediaPlan.Dpcoef;
                        worksheet.Cells[rowIndex + 2 + rowOff, 11 + 1 + colOff].Value = await _mpConverter.CalculateTermSeascoef(mediaPlan, _chidPricelistDictionary[mediaPlan.chid], _mpTermConverter.ConvertToDTO(term));
                        worksheet.Cells[rowIndex + 2 + rowOff, 12 + 1 + colOff].Value = await _mpConverter.CalculateTermSeccoef(mediaPlan, _chidPricelistDictionary[mediaPlan.chid], _spotcodeSpotDictionary[spotcode]);
                        worksheet.Cells[rowIndex + 2 + rowOff, 13 + 1 + colOff].Value = term.Date;
                        worksheet.Cells[rowIndex + 2 + rowOff, 14 + 1 + colOff].Value = spotcode;


                        for (int columnIndex = 0; columnIndex < numOfColumns; columnIndex++)
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

                        rowIndex += 1;
                    }
                }
            }
        }
    }
}
