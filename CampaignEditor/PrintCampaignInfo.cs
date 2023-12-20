using CampaignEditor.Controllers;
using Database.Repositories;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.DTOs.GoalsDTO;
using Database.DTOs.TargetCmpDTO;
using Database.DTOs.TargetDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ChannelCmpDTO;
using Database.Entities;
using System.Linq;
using CampaignEditor.DTOs.CampaignDTO;

namespace CampaignEditor
{
    public class PrintCampaignInfo
    {
        private GoalsController _goalsController;
        private TargetCmpController _targetCmpController;
        private TargetController _targetController;
        private SpotController _spotController;
        private ChannelCmpController _channelCmpController;
        private ChannelController _channelController;
        private PricelistController _pricelistController;
        private ActivityController _activityController;
        private CampaignController _campaignController;
        private ClientController _clientController;
        private CmpBrndController _cmpBrndController;
        private BrandController _brandController;

        public PrintCampaignInfo(IGoalsRepository goalsRepository,
                                 ITargetCmpRepository targetCmpRepository,
                                 ITargetRepository targetRepository,
                                 ISpotRepository spotRepository,
                                 IChannelCmpRepository channelCmpRepository,
                                 IChannelRepository channelRepository,
                                 IPricelistRepository pricelistRepository,
                                 IActivityRepository activityRepository,
                                 ICampaignRepository campaignRepository,
                                 IClientRepository clientRepository,
                                 ICmpBrndRepository cmpBrndRepository,
                                 IBrandRepository brandRepository)
        {
            _goalsController = new GoalsController(goalsRepository);
            _targetController = new TargetController(targetRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
            _spotController = new SpotController(spotRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _activityController = new ActivityController(activityRepository);
            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);
            _brandController = new BrandController(brandRepository);

        }

        public async Task PrintData(int cmpid, IEnumerable<ChannelDTO> selectedChannels, ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {
            int[] moved = new int[] { 0, 0 }; // How much each grid takes space
            int[] offset = new int[] { rowOff, colOff }; // How much each grid takes space
            int rowSpace = 1;
            int colSpace = 1;

            var campaign = await _campaignController.GetCampaignById(cmpid);
            moved = await PopulateInfoWorksheet(worksheet, campaign, rowOff + offset[0], colOff + offset[1]);
            offset[1] += moved[1] + colSpace;

            var targetCmps = await _targetCmpController.GetTargetCmpByCmpid(cmpid);
            targetCmps = targetCmps.OrderBy(tcmp => tcmp.priority);
            List<TargetDTO> targets = new List<TargetDTO>();
            foreach (var targetCmp in targetCmps)
            {
                targets.Add(await _targetController.GetTargetById(targetCmp.targid));
            }
            moved = PopulateTargetsWorksheet(worksheet, targets, rowOff + offset[0], colOff + offset[1]);
            offset[0] += moved[0] + rowSpace;

            var goals = await _goalsController.GetGoalsByCmpid(cmpid);
            moved = PopulateGoalsWorksheet(worksheet, goals, rowOff + offset[0], colOff + offset[1]);
            offset[0] = rowOff;
            offset[1] += moved[1] + colSpace;

            var spots = await _spotController.GetSpotsByCmpid(cmpid);
            spots = spots.OrderBy(s => s.spotcode);
            moved = PopulateSpotsWorksheet(worksheet, spots.ToList(), rowOff + offset[0], colOff + offset[1]);
            offset[1] += moved[1] + colSpace;

            var channelCmps = new List<ChannelCmpDTO>();
            foreach (var channel in selectedChannels)
            {
                var channelCmp = await _channelCmpController.GetChannelCmpByIds(campaign.cmpid, channel.chid);
                channelCmps.Add(channelCmp);
            }
            moved = await (PopulateChannelsWorksheet(worksheet, channelCmps, rowOff + offset[0], colOff + offset[1]));
            offset[1] += moved[1] + colSpace;

        }

        private void SetHeader(ExcelWorksheet worksheet, string value, int row, int col)
        {
            worksheet.Cells[row, col].Value = value;
            worksheet.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
        }

        private void SetValue(ExcelWorksheet worksheet, string value, int row, int col)
        {
            worksheet.Cells[row, col].Value = value;
        }

        private void SetDescription(ExcelWorksheet worksheet, string value, int row, int col, int colsize)
        {
            // Merge the cells
            var range = worksheet.Cells[row, col, row, col + colsize];
            range.Merge = true;
            worksheet.Cells[row, col].Value = value;
            worksheet.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
            worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private void SetHeaders(ExcelWorksheet worksheet, List<string> headers, int rowOff, int colOff, bool vertical = true)
        {
            if (vertical)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    SetHeader(worksheet, headers[i], i + rowOff, colOff);
                }
            }
            else
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    SetHeader(worksheet, headers[i], rowOff, i + colOff);
                }
            }

        }

        private void SetValues(ExcelWorksheet worksheet, List<string> headers, int rowOff, int colOff, bool vertical = true)
        {
            if (vertical)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    SetValue(worksheet, headers[i], i + rowOff, colOff);
                }
            }
            else
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    SetValue(worksheet, headers[i], rowOff, i + colOff);
                }
            }
        }

        public int[] PopulateGoalsWorksheet(ExcelWorksheet worksheet, GoalsDTO goals, int rowOff = 0, int colOff = 0)
        {
            int descSize = 2;
            // Set Desctiption to data
            SetDescription(worksheet, "CAMPAIGN GOALS", 1 + rowOff, 1 + colOff, descSize - 1);

            List<string> headers = new List<string>{"Ins", "GRP", "Budget"};
            // Set the column headers in Excel
            SetHeaders(worksheet, headers, 1 + 1 + rowOff, 1 + colOff);


            // Set the cell values in Excel
            List<string> values = new List<string> { goals.ins != 0 ? goals.ins.ToString() : "-" ,
                                                     goals.grp != 0 ? goals.grp.ToString() : "-",
                                                     goals.budget != 0 ? goals.budget.ToString() : "-"};
            SetValues(worksheet, values, 1 + 1 + rowOff, 2 + colOff);

            int rowMoved = headers.Count + 1;
            int colMoved = descSize;
            return new int[] { rowMoved, colMoved };
        }

        public int[] PopulateTargetsWorksheet(ExcelWorksheet worksheet, List<TargetDTO> targets, int rowOff = 0, int colOff = 0)
        {
            int descSize = 2;
            // Set Desctiption to data
            SetDescription(worksheet, "TARGETS", 1 + rowOff, 1 + colOff, descSize - 1);

            List<string> headers = new List<string> { "Primary", "Secondary", "Tertiary" };
            // Set the column headers in Excel
            SetHeaders(worksheet, headers, 1 + 1 + rowOff, 1 + colOff);


            // Set the cell values in Excel
            List<string> values = new List<string>();
            foreach (var target in targets)
            {
                values.Add(target.targname.Trim());
            }
            while (values.Count < headers.Count)
            {
                values.Add("-");
            }
            SetValues(worksheet, values, 1 + 1 + rowOff, 2 + colOff);

            int rowMoved = headers.Count + 1;
            int colMoved = descSize;
            return new int[] { rowMoved, colMoved };
        }

        public int[] PopulateSpotsWorksheet(ExcelWorksheet worksheet, List<SpotDTO> spots, int rowOff = 0, int colOff = 0)
        {
            int descSize = 3;
            // Set Desctiption to data
            SetDescription(worksheet, "SPOTS", 1 + rowOff, 1 + colOff, descSize - 1);

            List<string> headers = new List<string>();
            for (int i=0; i<spots.Count; i++)
            {
                headers.Add(((char)('A' + i)).ToString());
            }

            // Set the column headers in Excel
            SetHeaders(worksheet, headers, 1 + 1 + rowOff, 1 + colOff);


            // Set the cell values in Excel
            List<string> spotNames = new List<string>();
            List<string> spotLengths = new List<string>();
            foreach (var spot in spots)
            {
                spotNames.Add(spot.spotname.Trim());
                spotLengths.Add(spot.spotlength.ToString());
            }

            SetValues(worksheet, spotNames, 1 + 1 + rowOff, 2 + colOff);
            SetValues(worksheet, spotLengths, 1 + 1 + rowOff, 3 + colOff);

            int rowMoved = headers.Count + 1;
            int colMoved = descSize;
            return new int[] { rowMoved, colMoved };
        }
        
        public async Task<int[]> PopulateChannelsWorksheet(ExcelWorksheet worksheet, List<ChannelCmpDTO> channelCmps, int rowOff = 0, int colOff = 0)
        {
            int descSize = 3;
            // Set Desctiption to data
            SetDescription(worksheet, "Channels", 1 + rowOff, 1 + colOff, descSize - 1);

            List<string> headers = new List<string>();
            List<string> pricelists = new List<string>();
            List<string> activities = new List<string>();

            foreach (var channelCmp in channelCmps)
            {
                headers.Add((await _channelController.GetChannelById(channelCmp.chid)).chname);
                pricelists.Add((await _pricelistController.GetPricelistById(channelCmp.plid)).plname);
                activities.Add((await _activityController.GetActivityById(channelCmp.actid)).act);
            }

            // Set the column headers in Excel
            SetHeaders(worksheet, headers, 1 + 1 + rowOff, 1 + colOff);
            // Set values
            SetValues(worksheet, pricelists, 1 + 1 + rowOff, 2 + colOff);
            SetValues(worksheet, activities, 1 + 1 + rowOff, 3 + colOff);

            int rowMoved = headers.Count + 1;
            int colMoved = descSize;
            return new int[] { rowMoved, colMoved };
        }

        public async Task<int[]> PopulateInfoWorksheet(ExcelWorksheet worksheet, CampaignDTO campaign, int rowOff = 0, int colOff = 0)
        {
            int descSize = 2;
            // Set Desctiption to data
            SetDescription(worksheet, "CAMPAIGN INFO", 1 + rowOff, 1 + colOff, descSize - 1);

            List<string> headers = new List<string> { "Client", "Campaign", "Start date",
            "End date", "DP Start", "DP End", "Active", "Brand 1", "Brand 2"};
            // Set the column headers in Excel
            SetHeaders(worksheet, headers, 1 + 1 + rowOff, 1 + colOff);


            // Set the cell values in Excel
            List<string> values = new List<string>();
            values.Add((await _clientController.GetClientById(campaign.clid)).clname.Trim());
            values.Add(campaign.cmpname.Trim());
            values.Add(TimeFormat.YMDStringToRepresentative(campaign.cmpsdate));
            values.Add(TimeFormat.YMDStringToRepresentative(campaign.cmpedate));
            values.Add(campaign.cmpstime.Substring(0, 5));
            values.Add(campaign.cmpetime.Substring(0, 5));
            values.Add(campaign.active.ToString());
            var cmpBrands = await _cmpBrndController.GetCmpBrndsByCmpId(campaign.cmpid);
            foreach (var cmpBrand in cmpBrands)
            {
                values.Add((await _brandController.GetBrandById(cmpBrand.brbrand)).brand);
            }
            int brandCount = cmpBrands.Count();
            while(brandCount < 2)
            {
                values.Add("-");
                brandCount++;
            }

            SetValues(worksheet, values, 1 + 1 + rowOff, 2 + colOff);

            int rowMoved = headers.Count + 1;
            int colMoved = descSize;
            return new int[] { rowMoved, colMoved };
        }
    }
}
