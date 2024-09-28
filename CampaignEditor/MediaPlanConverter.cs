using AutoMapper;
using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.PricelistDTO;
using Database.DTOs.SeasonalitiesDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public class MediaPlanConverter
    {
        private MediaPlanForecastData _forecastData;

        private readonly IMapper _mapperFromDTO;

        private MediaPlanHistController _mediaPlanHistController;
        private MediaPlanTermController _mediaPlanTermController;

        private Dictionary<Tuple<int, SpotDTO>, SpotCoefs> _plidSpotCoefsDict = new Dictionary<Tuple<int, SpotDTO>, SpotCoefs>();

        public MediaPlanConverter(IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanTermRepository mediaPlanTermRepository)
        {

            var configurationFromDTO = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MediaPlanDTO, MediaPlan>()
                .ForMember(dest => dest.Amr1trim, opt => opt.Ignore())
                .ForMember(dest => dest.Amr2trim, opt => opt.Ignore())
                .ForMember(dest => dest.Amr3trim, opt => opt.Ignore())
                .ForMember(dest => dest.Amrsaletrim, opt => opt.Ignore());
            });
            _mapperFromDTO = configurationFromDTO.CreateMapper();

            _mediaPlanHistController = new MediaPlanHistController(mediaPlanHistRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
        }

        public void Initialize(MediaPlanForecastData forecastData)
        {
            _forecastData = forecastData;
            FillPlidSpotCoefsDict();
        }            

        private void FillPlidSpotCoefsDict()
        {
            _plidSpotCoefsDict.Clear();

            foreach (var pricelist in _forecastData.Pricelists)
            {
                var sectable = _forecastData.PlidSectableDict[pricelist.plid];               

                var seasonality = _forecastData.PlidSeasonalityDict[pricelist.plid];
                var seasonalities = _forecastData.SeasidSeasonalitiesDict[seasonality.seasid];

                    
                foreach (var spot in _forecastData.Spots)
                {
                    List<DateRangeSeasCoef> dateRanges = new List<DateRangeSeasCoef>();

                    var sectables = _forecastData.SecidSectablesDict[sectable.sctid].FirstOrDefault(secs => secs.sec == spot.spotlength, null);
                    decimal seccoef = sectables == null ? (decimal)spot.spotlength/30 : sectables.coef;

                    if (seasonality == null || seasonalities == null || seasonalities.Count == 0)
                    {
                        var seasCoef = 1.0M;
                        DateRangeSeasCoef dateRange = new DateRangeSeasCoef(TimeFormat.YMDStringToDateOnly(_forecastData.Campaign.cmpsdate),
                                                        TimeFormat.YMDStringToDateOnly(_forecastData.Campaign.cmpedate),
                                                        seasCoef);
                        dateRanges.Add(dateRange);
                    }
                    else
                    {
                        seasonalities = GetConsecutiveSeasonalityRanges(_forecastData.Campaign ,seasonalities);
                    }
                    foreach (var seasonalityRange in seasonalities)
                    {
   
                        var seasCoef = seasonalityRange.coef;
                        DateRangeSeasCoef dateRange = new DateRangeSeasCoef(TimeFormat.YMDStringToDateOnly(seasonalityRange.stdt),
                                                        TimeFormat.YMDStringToDateOnly(seasonalityRange.endt),
                                                        seasCoef);
                        dateRanges.Add(dateRange);
                    }

                    Dictionary<int, decimal> cbrCoefs = new Dictionary<int, decimal>();

                    foreach (var channel in _forecastData.Channels)
                    {
                        decimal coef = 1.0M;
                        var cbr = _forecastData.Cobrands.FirstOrDefault(cbr => cbr.chid == channel.chid && cbr.spotcode == spot.spotcode[0]);
                        if (cbr != null)
                            coef = cbr.coef;

                        cbrCoefs.Add(channel.chid, coef);                     

                    }

                    _plidSpotCoefsDict[Tuple.Create(pricelist.plid, spot)] = new SpotCoefs(dateRanges, cbrCoefs, seccoef);

                }

            }
            
        }

        private List<SeasonalitiesDTO> GetConsecutiveSeasonalityRanges(CampaignDTO campaign, IEnumerable<SeasonalitiesDTO> seasonalities)
        {
            List<SeasonalitiesDTO> consecutiveSeasonalities = new List<SeasonalitiesDTO>();

            var orderedSeasonlities = seasonalities.OrderBy(seas => seas.stdt).ToList();
            DateOnly startDate = TimeFormat.YMDStringToDateOnly(campaign.cmpsdate);
            DateOnly endDate = TimeFormat.YMDStringToDateOnly(campaign.cmpedate);
            DateOnly currentDate = startDate;           



            foreach (var seasonalitiesRange in orderedSeasonlities)
            {
                if (TimeFormat.YMDStringToDateOnly(seasonalitiesRange.endt) < startDate)
                    continue;
                if (TimeFormat.YMDStringToDateOnly(seasonalitiesRange.stdt) > endDate)
                    break;
                // Check if the current date is before the start date of the next interval
                if (currentDate < TimeFormat.YMDStringToDateOnly(seasonalitiesRange.stdt))
                {
                    // Add interval from current date to the start date of the next interval
                    var seasonalitiesPreRangePart = new SeasonalitiesDTO
                    (
                        seasid: -1,
                        stdt: currentDate.ToString("yyyyMMdd"),
                        endt: TimeFormat.YMDStringToDateOnly(seasonalitiesRange.stdt).AddDays(-1).ToString("yyyyMMdd"),
                        coef: 1.0M
                    );
                    consecutiveSeasonalities.Add(seasonalitiesPreRangePart);
                    currentDate = TimeFormat.YMDStringToDateOnly(seasonalitiesPreRangePart.endt).AddDays(1);

                }
                
                var maxStartDate = startDate > currentDate ?
                    startDate : currentDate;

                var minEndDate = endDate < TimeFormat.YMDStringToDateOnly(seasonalitiesRange.endt) ?
                    endDate : TimeFormat.YMDStringToDateOnly(seasonalitiesRange.endt);

                var seasonalitiesRangePart = new SeasonalitiesDTO
                (
                    seasid: seasonalitiesRange.seasid,
                    stdt: maxStartDate.ToString("yyyyMMdd"),
                    endt: minEndDate.ToString("yyyyMMdd"),
                    coef: seasonalitiesRange.coef
                );
                consecutiveSeasonalities.Add(seasonalitiesRangePart);
                

                // Update current date to the end date of the current interval plus one day
                currentDate = TimeFormat.YMDStringToDateOnly(seasonalitiesRange.endt).AddDays(1);
            }

            // Add the interval from the last end date to the end date of the campaign
            if (currentDate <= endDate)
            {
                var seasonalitiesRangePart = new SeasonalitiesDTO
                (
                    seasid: -1,
                    stdt: currentDate.ToString("yyyyMMdd"),
                    endt: endDate.ToString("yyyyMMdd"),
                    coef: 1.0M
                );
                consecutiveSeasonalities.Add(seasonalitiesRangePart);
            }

            return consecutiveSeasonalities;

        }

        public List<SpotCoefsTable> GetProgramSpotCoefs(MediaPlan mediaPlan)
        {
            var chid = mediaPlan.chid;
            var pricelist = _forecastData.ChidPricelistDict[chid];

            List<SpotCoefsTable> spotCoefsTables = new List<SpotCoefsTable>();
            foreach (var spot in _forecastData.Spots)
            {
                var spotCoefs = _plidSpotCoefsDict[Tuple.Create(pricelist.plid, spot)];

                foreach (var dateRange in spotCoefs.dateRanges)
                {
                    decimal price = 0.0M;
                    var seascoef = dateRange.seascoef;
                    var seccoef = spotCoefs.seccoef;
                    var cbrCoef = 1.0M;
                    if (spotCoefs.cbrCoefs.ContainsKey(mediaPlan.chid))
                        cbrCoef = spotCoefs.cbrCoefs[mediaPlan.chid];
        
                    decimal coefs = seascoef * seccoef * cbrCoef * mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.CoefA * mediaPlan.CoefB * mediaPlan.Chcoef;
                    if (coefs == 0)
                    {
                        price = 0.0M;
                    }
                    else
                    {
                        if (pricelist.pltype == 1)
                        {

                            //price += coefs * spotDTO.spotlength;
                            decimal standardPrice = coefs * 30;
                            price += PriceWithGRPCheck(mediaPlan, pricelist, standardPrice);
                        }
                        // For cpp pricelists
                        else
                        {

                            decimal amrpSale = mediaPlan.Amrpsale;
                            if (pricelist.mgtype && amrpSale < pricelist.minprice && amrpSale != 0)
                            {
                                amrpSale = pricelist.minprice;
                            }
                            //decimal standardPrice = (pricelist.price / 30) * spotDTO.spotlength * amrpSale * coefs;
                            //decimal standardPrice = (pricelist.price / 30) * spotDTO.spotlength * mediaPlan.Amrpsale * coefs;
                            decimal standardPrice = pricelist.price * amrpSale * coefs;
                            price = PriceWithGRPCheck(mediaPlan, pricelist, standardPrice);
                            //price = (pricelist.price / 30) * spot.spotlength * mediaPlan.Amrpsale * coefs;
                        }
                    }                  

                    SpotCoefsTable spotCoefsTable = new SpotCoefsTable(spot, dateRange, seccoef, cbrCoef, price);

                    spotCoefsTables.Add(spotCoefsTable);
                }
            }

            return spotCoefsTables;
        }

        public decimal GetProgramSpotPrice(MediaPlan mediaPlan, MediaPlanTerm mpTerm,  SpotDTO spot, TermCoefs termCoefs)
        {
            var chid = mediaPlan.chid;
            var pricelist = _forecastData.ChidPricelistDict[chid];

            var spotCoefs = _plidSpotCoefsDict[Tuple.Create(pricelist.plid, spot)];
            decimal price = 0.0M;

            foreach (var dateRange in spotCoefs.dateRanges)
            {
                if (mpTerm.Date < dateRange.fromDate || mpTerm.Date > dateRange.toDate)
                    continue;
                else
                {
                    var seascoef = dateRange.seascoef;
                    var seccoef = spotCoefs.seccoef;
                    var cbrcoef = 1.0M;
                    if (spotCoefs.cbrCoefs.ContainsKey(mediaPlan.chid))
                        cbrcoef = spotCoefs.cbrCoefs[mediaPlan.chid];
                    var chcoef = mediaPlan.Chcoef;
                    decimal coefs = chcoef * seascoef * seccoef * cbrcoef * mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.CoefA * mediaPlan.CoefB;
                    
                    termCoefs.Seascoef = seascoef;
                    termCoefs.Seccoef = seccoef;
                    termCoefs.Cbrcoef = cbrcoef;

                    if (pricelist.pltype == 1)
                    {
                        decimal standardPrice = coefs * 30;
                        price = PriceWithGRPCheck(mediaPlan, pricelist, standardPrice);

                        break;
                    }
                    // For cpp pricelists
                    else
                    {
                        termCoefs.Amrpsale = mediaPlan.Amrpsale;
                        termCoefs.Cpp = pricelist.price;

                        decimal amrpSale = mediaPlan.Amrpsale;
                        if (pricelist.mgtype && amrpSale < pricelist.minprice && amrpSale != 0)
                        {
                            amrpSale = pricelist.minprice;
                        }
                        
                        decimal standardPrice = pricelist.price * amrpSale * coefs;

                        price = PriceWithGRPCheck(mediaPlan, pricelist, standardPrice);
                        break;
                    }
                }
                

            }

            termCoefs.Price = price;
            return price;
        }

        public async Task<MediaPlan> ConvertFirstFromDTO(MediaPlanDTO mediaPlanDTO)
        {
            var mediaPlan = _mapperFromDTO.Map<MediaPlanDTO, MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            await CalculateFirst(mediaPlan);

            return mediaPlan;
        }

        public MediaPlan ConvertFromDTO(MediaPlanDTO mediaPlanDTO, IEnumerable<MediaPlanTermDTO> terms = null, bool calculatePrice = false)
        {

            var mediaPlan = _mapperFromDTO.Map<MediaPlanDTO, MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            ComputeExtraProperties(mediaPlan, terms, calculatePrice);

            return mediaPlan;
        }

        public void SetDayPart(MediaPlan mediaPlan)
        {
            if (mediaPlan.blocktime == null)
            {
                mediaPlan.DayPart = null;
                return;
            }

            string time = mediaPlan.blocktime;
            foreach (var dayPart in _forecastData.DayPartsDict.Keys)
            {
                foreach (var dpTime in _forecastData.DayPartsDict[dayPart])
                {
                    if ((String.Compare(dpTime.stime, time) <= 0) &&
                        (String.Compare(dpTime.etime, time) >= 0))
                    {
                        mediaPlan.DayPart = dayPart;
                        return;
                    }
                }
            }
        }

        public void SetChCoef(MediaPlan mediaPlan)
        {
            mediaPlan.chcoef = _forecastData.ChidChcoefDict[mediaPlan.chid];           
        }

        public async Task CalculateAMRs(MediaPlan mediaPlan)
        {
            var hists = await _mediaPlanHistController.GetAllMediaPlanHistsByXmpid(mediaPlan.xmpid);

            await SetOutliers(hists);

            var filteredHists = hists.Where(h => h.active);

            mediaPlan.Amr1 = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amr1)) * mediaPlan.amr1trim/100;
            mediaPlan.Amr2 = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amr2)) * mediaPlan.amr2trim/100;
            mediaPlan.Amr3 = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amr3)) * mediaPlan.amr3trim/100;
            mediaPlan.Amrsale = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amrsale)) * mediaPlan.amrsaletrim/100;
            mediaPlan.Amrp1 = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amrp1)) * mediaPlan.amr1trim/100;
            mediaPlan.Amrp2 = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amrp2)) * mediaPlan.amr2trim/100;
            mediaPlan.Amrp3 = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amrp3)) * mediaPlan.amr3trim/100;
            mediaPlan.Amrpsale = MathFunctions.ArithmeticMean(filteredHists.Select(h => h.amrpsale)) * mediaPlan.amrsaletrim/100;
        }

        public void CalculateLengthAndInsertations(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms)
        {
            var insertations = 0;
            var length = 0;
            foreach (var term in terms)
            {
                if (term != null && term.spotcode != null && term.spotcode.Length > 0)
                {
                    foreach (var spotcode in term.spotcode.Trim())
                    {
                        var spot = _forecastData.SpotcodeSpotDict[spotcode];
                        if (spot != null)
                        {
                            length += spot.spotlength;
                            insertations += 1;
                        }

                    }
                }

            }
            mediaPlan.Insertations = insertations;
            mediaPlan.Length = length;
        }

        private void CalculateDPCoef(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms = null)
        {

            //var prices = await _pricesController.GetAllPricesByPlId(pricelist.plid);
            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];
            var prices = _forecastData.PlidPricesDict[pricelist.plid];

            string blocktime = mediaPlan.blocktime ?? mediaPlan.stime;

            foreach (var price in prices)
            {
                // No need to check for day because mediaPlan will be adjusted correctly
                if ((TimeFormat.CompareRepresentative(price.dps, blocktime) != 1) &&
                    (TimeFormat.CompareRepresentative(price.dpe, blocktime) != -1))
                {
                    // Checks if all days from mediaPlan is in pricelist    
                    if (mediaPlan.days.Trim().All(c => price.days.Trim().Contains(c)))
                    {
                        mediaPlan.Dpcoef = price.price;
                        return;
                    }
                }
            }
            
            mediaPlan.Dpcoef = 1.0M;
            return;

        }

        private void CalculateRealizedDPCoef(MediaPlanRealized mpRealized)
        {

            //var prices = await _pricesController.GetAllPricesByPlId(pricelist.plid);
            var chid = _forecastData.ChrdsidChidDict[mpRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];
            var prices = _forecastData.PlidPricesDict[pricelist.plid];

            string time = TimeFormat.TimeStrToRepresentative(mpRealized.stimestr);

            foreach (var price in prices)
            {
                // No need to check for day because mediaPlan will be adjusted correctly
                if ((TimeFormat.CompareRepresentative(price.dps, time) != 1) &&
                    (TimeFormat.CompareRepresentative(price.dpe, time) != -1))
                {
                    int day = TimeFormat.GetDayOfWeekInt(mpRealized.date);
                    // Checks if all days from mediaPlan is in pricelist    
                    if (price.days.Contains(day.ToString()))
                    {
                        mpRealized.Dpcoef = price.price;
                        return;
                    }
                }
            }

            mpRealized.Dpcoef = 1.0M;
            return;
        }

        public void CalculateRealizedCoefs(MediaPlanRealized mpRealized, PricelistDTO pricelist)
        {
            CalculateRealizedDPCoef(mpRealized);
            mpRealized.Seccoef = CalculateRealizedSeccoef(mpRealized, pricelist);
            mpRealized.Seascoef = CalculateRealizedSeascoef(mpRealized, pricelist);
            mpRealized.Cbrcoef = CalculateRealizedCbrcoef(mpRealized);

            decimal progcoef = 1.0M;
            decimal coefA = 1.0M;
            decimal coefB = 1.0M;
            decimal chcoef = 1.0M;
            

            if (mpRealized.MediaPlan != null)
            {
                chcoef = mpRealized.MediaPlan.chcoef;
                progcoef = mpRealized.MediaPlan.progcoef;
                coefA = mpRealized.MediaPlan.coefA;
                coefB = mpRealized.MediaPlan.coefB;
            }

            mpRealized.Chcoef = mpRealized.Chcoef ?? chcoef;
            mpRealized.Progcoef = mpRealized.Progcoef ?? progcoef;
            mpRealized.CoefA = mpRealized.CoefA ?? coefA;
            mpRealized.CoefB = mpRealized.CoefB ?? coefB;

            CoefsUpdated(mpRealized, pricelist);
        }

        // When user update coefs manually, recalculate this
        public void CoefsUpdated(MediaPlanRealized mpRealized, PricelistDTO pricelist)
        {
            decimal coefs = mpRealized.Chcoef.Value * mpRealized.CoefA.Value * mpRealized.CoefB.Value * mpRealized.Seccoef.Value * mpRealized.Seascoef.Value * mpRealized.Cbrcoef.Value * mpRealized.Progcoef.Value * mpRealized.Dpcoef!.Value;
            CalculateRealizedPrice(mpRealized, pricelist, coefs);
            CalculateRealizedCPP(mpRealized, pricelist);
        }
        public void CalculateRealizedPrice(MediaPlanRealized mpRealized)
        {
            decimal coefs = mpRealized.Chcoef.Value * mpRealized.CoefA.Value * mpRealized.CoefB.Value * mpRealized.Seccoef.Value * mpRealized.Seascoef.Value * mpRealized.Progcoef.Value * mpRealized.Dpcoef!.Value;
            var chid = _forecastData.ChrdsidChidDict[mpRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];

            CalculateRealizedPrice(mpRealized, pricelist, coefs);
        }

        public void CalculateRealizedPrice(MediaPlanRealized mpRealized, PricelistDTO pricelist, decimal coefs)
        {
            decimal price = 0.0M;

            if (pricelist.pltype == 1)
            {
                decimal standardPrice = coefs * 30;

                price = PriceWithGRPCheck(mpRealized, pricelist, standardPrice);

            }
            // For cpp pricelists
            else
            {
                decimal amrpSale = mpRealized.Amrpsale.Value;
                if (pricelist.mgtype && amrpSale < pricelist.minprice && amrpSale != 0)
                {
                    amrpSale = pricelist.minprice;
                }

                int realizedLength = mpRealized.etime.Value - mpRealized.stime.Value;
                int spotLength = FindClosestSpotLength(realizedLength);
                //decimal standardPrice = (pricelist.price / 30) * spotLength * amrpSale * coefs;
                decimal standardPrice = pricelist.price * amrpSale * coefs;
                price = PriceWithGRPCheck(mpRealized, pricelist, standardPrice);
            }

            mpRealized.price = price;
        }

        private decimal PriceWithGRPCheck(MediaPlanRealized mpRealized, PricelistDTO pricelist, decimal standardValue)
        {
            if (mpRealized.Amrp1 < pricelist.minprice && pricelist.fixprice != 0)
            {
                return pricelist.fixprice;
            }
            return standardValue;
        }

        public decimal CalculateRealizedSeccoef(MediaPlanRealized mpRealized, PricelistDTO pricelist)
        {
          
            var sectable = _forecastData.PlidSectableDict[pricelist.plid];
            int realizedLength = mpRealized.dure.Value;
            int spotLength = FindClosestSpotLength(realizedLength);

            var sectables = _forecastData.SecidSectablesDict[sectable.sctid].FirstOrDefault(secs => secs.sec == spotLength, null);
            var seccoef = 1.0M;
           
            seccoef = sectables == null ? (decimal)spotLength / 30 : sectables.coef;
            return seccoef;
        }

        private int FindClosestSpotLength(int realizedLength)
        {
            foreach (var spot in _forecastData.Spots)
            {
                if (spot.spotlength <= realizedLength && realizedLength <= spot.spotlength)
                {
                    return spot.spotlength;
                }
            }
            // if realized length is +- 1 sec from some spot, return that value
            // if not found, return realized length
            foreach (var spot in _forecastData.Spots)
            {
                if (spot.spotlength - 1 <= realizedLength && realizedLength <= spot.spotlength + 1)
                {
                    return spot.spotlength;
                }
            }

            return realizedLength;
        }

        public decimal CalculateRealizedSeascoef(MediaPlanRealized mpRealized, PricelistDTO pricelist)
        {
            var seasonality = _forecastData.PlidSeasonalityDict[pricelist.plid];
            var seasonalities = _forecastData.SeasidSeasonalitiesDict[seasonality.seasid];
            decimal seasCoef = 1.0M;

            DateOnly date = TimeFormat.YMDStringToDateOnly(mpRealized.date);
            foreach (var seas in seasonalities)
            {
                if (date >= DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(seas.stdt).Date) &&
                    date <= DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(seas.endt).Date))
                {
                    seasCoef = seas.coef;
                    break;
                }
            }

            return seasCoef;
        }

        public decimal CalculateRealizedCbrcoef(MediaPlanRealized mpRealized)
        {
            decimal cbrcoef = 1.0M;
            var spotPair = _forecastData.SpotPairs.FirstOrDefault(sp => !string.IsNullOrEmpty(sp.spotcode) && sp.spotnum == mpRealized.spotnum);
            if (spotPair != null)
            {
                char spotcode = spotPair.spotcode[0];
                int chid = _forecastData.ChrdsidChidDict[mpRealized.chid.Value];
                var cbr = _forecastData.Cobrands.FirstOrDefault(cbr => cbr.chid == chid && cbr.spotcode == spotcode);
                if (cbr != null)
                    cbrcoef = cbr.coef;
            }

            return cbrcoef;
        }

        private void CalculateRealizedCPP(MediaPlanRealized mpRealized, PricelistDTO pricelist)
        {
            // For seconds pricelists
            if (pricelist.pltype == 1)
            {
                if (mpRealized.amrp1 > 0)
                {
                    mpRealized.Cpp = mpRealized.price / (mpRealized.amrp1);
                }
                else
                {
                    mpRealized.Cpp = 0;
                }
            }
            // For cpp pricelists
            else
            {
                mpRealized.Cpp = pricelist.price;
            }
        }



        private async Task CalculateFirst(MediaPlan mediaPlan)
        {
            /*var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);*/

            await CalculateAMRs(mediaPlan);
            //await CalculateDPCoef(mediaPlan, pricelist);
            CalculateDPCoef(mediaPlan);
            var terms = await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
            ComputeExtraProperties(mediaPlan, terms, true);

        } 

        public void ComputeExtraProperties(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms, bool calculatePrice = false)
        {
            SetDayPart(mediaPlan);
            SetChCoef(mediaPlan);
            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];
            CalculateLengthAndInsertations(mediaPlan, terms);
            
            if (calculatePrice)
            {
                CalculateAvgCoefs(mediaPlan, pricelist, terms);
                CalculatePrice(mediaPlan, pricelist, terms);
            }

            CalculatePricePerSeconds(mediaPlan, pricelist);
            CalculateCPP(mediaPlan, pricelist, terms);

        }

        public void CoefsChanged(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms)
        {
            /*var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);*/

            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];
            CalculatePrice(mediaPlan, pricelist, terms);
            
        }

        private void CalculatePrice(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {
                CalculatePriceSecondsPricelist(mediaPlan, pricelist, terms);                
            }
            // For cpp pricelists
            else
            {
                CalculatePriceCPPPricelist(mediaPlan, pricelist, terms);               
            }
        }

        private void CalculatePriceSecondsPricelist(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            decimal price = 0;
            foreach (MediaPlanTermDTO mpTerm in terms)
            {
                if (mpTerm != null && mpTerm.spotcode != null)
                {
                    foreach (char spotcode in mpTerm.spotcode.Trim())
                    {
                        SpotDTO spotDTO = _forecastData.SpotcodeSpotDict[spotcode];

                        decimal seccoef = CalculateTermSeccoef(mediaPlan, spotDTO);
                        decimal seascoef = CalculateTermSeascoef(mediaPlan, mpTerm);
                        decimal cbrcoef = CalculateTermCbrcoef(mediaPlan, spotDTO);
                        decimal coefs = mediaPlan.CoefA * mediaPlan.CoefB * mediaPlan.Chcoef * seascoef * seccoef * cbrcoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;
                        if (coefs == 0)
                        {
                            price = 0.0M;
                            break;
                        }
                        //price += coefs * spotDTO.spotlength;
                        decimal standardPrice = coefs * 30;
                        price += PriceWithGRPCheck(mediaPlan, pricelist, standardPrice);
                    }
                }


            }

            mediaPlan.Price = price;
        }

        private void CalculatePriceCPPPricelist(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            decimal price = 0;
            foreach (MediaPlanTermDTO mpTerm in terms)
            {
                if (mpTerm != null && mpTerm.spotcode != null)
                {
                    foreach (char spotcode in mpTerm.spotcode.Trim())
                    {
                        SpotDTO spotDTO = _forecastData.SpotcodeSpotDict[spotcode];

                        decimal seccoef = CalculateTermSeccoef(mediaPlan, spotDTO);
                        decimal seascoef = CalculateTermSeascoef(mediaPlan, mpTerm);
                        decimal cbrcoef = CalculateTermCbrcoef(mediaPlan, spotDTO);
                        decimal coefs = mediaPlan.CoefA * mediaPlan.CoefB * mediaPlan.Chcoef * seascoef * seccoef * cbrcoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                        if (coefs == 0)
                        {
                            price = 0.0M;
                            break;
                        }
                        decimal amrpSale = mediaPlan.Amrpsale;
                        if (pricelist.mgtype && amrpSale < pricelist.minprice && amrpSale != 0)
                        {
                            amrpSale = pricelist.minprice;
                        }
                        //decimal standardPrice = (pricelist.price / 30) * spotDTO.spotlength * amrpSale * coefs;
                        //decimal standardPrice = (pricelist.price / 30) * spotDTO.spotlength * mediaPlan.Amrpsale * coefs;
                        decimal standardPrice = pricelist.price * amrpSale * coefs;

                        price += PriceWithGRPCheck(mediaPlan, pricelist, standardPrice);
                    }
                }


            }
            mediaPlan.Price = price;

        }

        private void CalculatePricePerSeconds(MediaPlan mediaPlan, PricelistDTO pricelist)
        {

            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {
                decimal coefs = mediaPlan.CoefB * mediaPlan.CoefB * mediaPlan.Chcoef * mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.Seascoef;

                if (mediaPlan.Amrp1 > 0)
                {
                    mediaPlan.PricePerSecond = coefs / mediaPlan.Amrp1;
                }
                else
                {
                    mediaPlan.PricePerSecond = 0;
                }
            }
            // For cpp pricelists
            else
            {
                if (mediaPlan.Length > 0)
                    mediaPlan.PricePerSecond = mediaPlan.Price / mediaPlan.Length;
                else
                    mediaPlan.PricePerSecond = 0;
            }
        }

        private void CalculateCPP(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            // For seconds pricelists
            if (pricelist.pltype == 1)
            {
                if (mediaPlan.Amrp1 > 0)
                {
                    mediaPlan.Cpp = mediaPlan.Price / (mediaPlan.Amrp1);
                    //mediaPlan.Cpp = (mediaPlan.Price / (mediaPlan.Amrp1)) * (mediaPlan.Length / 30);
                }
                else
                {
                    mediaPlan.Cpp = 0;
                }
            }
            // For cpp pricelists
            else
            {
                //CalculateAvgCpp(mediaPlan, pricelist, terms);
                mediaPlan.Cpp = pricelist.price;
            }
        }

        private void CalculateAvgCpp(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            int cppCount = 0;
            decimal cpp = 0.0M;

            foreach (var term in terms)
            {
                if (term != null && term.spotcode != null)
                {
                    foreach (char c in term.spotcode.Trim())
                    {
                        SpotDTO spot = _forecastData.SpotcodeSpotDict[c];
                        cpp += (pricelist.price / (decimal)30) * spot.spotlength;
                        cppCount += 1;
                    }
                }
            }

            if (cppCount == 0)
            {
                mediaPlan.Cpp = pricelist.price;
            }
            else
            {
                mediaPlan.Cpp = cpp / cppCount;
            }
        }

        public void CalculateAvgCoefs(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            CalculateAvgSeccoef(mediaPlan, pricelist, terms);
            CalculateAvgSeascoef(mediaPlan, pricelist, terms);
            CalculateAvgCbrcoef(mediaPlan, pricelist, terms);
        }

        

        private decimal PriceWithGRPCheck(MediaPlan mediaPlan, PricelistDTO pricelist, decimal standardValue)
        {
            if (mediaPlan.Amrp1 < pricelist.minprice && pricelist.fixprice != 0)
            {
                return pricelist.fixprice;
            }
            return standardValue;
        }

        public decimal CalculateTermSeccoef(MediaPlan mediaPlan, SpotDTO spotDTO)
        {

            /*var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
            var sectables = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, spotDTO.spotlength);*/
            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];
            var sectable = _forecastData.PlidSectableDict[pricelist.plid];
            var sectables = _forecastData.SecidSectablesDict[sectable.sctid].FirstOrDefault(secs => secs.sec == spotDTO.spotlength, null);
            var seccoef = 1.0M;
            /*if (sectable.sctid != 1)
            {
                seccoef = sectables == null ? 1 : sectables.coef * ((decimal)30 / spotDTO.spotlength);
            }*/
            seccoef = sectables == null ? (decimal)spotDTO.spotlength/30 : sectables.coef;
            //seccoef *= ((decimal)spotDTO.spotlength / 30);
            return seccoef;
        }

        public decimal CalculateTermSeascoef(MediaPlan mediaPlan, MediaPlanTermDTO mpTerm)
        {
            /*var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);

            var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);*/

            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];
            var seasonality = _forecastData.PlidSeasonalityDict[pricelist.plid];
            var seasonalities = _forecastData.SeasidSeasonalitiesDict[seasonality.seasid];
            decimal seasCoef = 1.0M;

            foreach (var seas in seasonalities)
            {
                if (mpTerm.date >= DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(seas.stdt).Date) &&
                    mpTerm.date <= DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(seas.endt).Date))
                {
                    seasCoef = seas.coef;
                    break;
                }
            }

            return seasCoef;
        }

        public decimal CalculateTermCbrcoef(MediaPlan mediaPlan, SpotDTO spot)
        {

            decimal coef = 1.0M;
            var cbr = _forecastData.Cobrands.FirstOrDefault(cbr => cbr.chid == mediaPlan.chid && cbr.spotcode == spot.spotcode[0]);
            if (cbr != null)
            {
                coef = cbr.coef;
            }

            return coef;
        }

        private void CalculateAvgSeccoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            /*var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
            var sectables = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, (int)Math.Ceiling(mediaPlan.AvgLength));
            var seccoef = sectables == null ? 1 : sectables.coef;
            mediaPlan.Seccoef = seccoef;*/

            var sectable = _forecastData.PlidSectableDict[pricelist.plid];
            /*if (sectable.sctid == 1)
            {
                mediaPlan.Seccoef = 1.0;
                return;
            }*/

            int secCount = 0;
            decimal seccoef = 0.0M;

            foreach (var term in terms)
            {
                if (term != null && term.spotcode != null)
                {
                    foreach (char c in term.spotcode.Trim())
                    {
                        SpotDTO spot = _forecastData.SpotcodeSpotDict[c];
                        //var sec = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, spot.spotlength);
                        var sec = _forecastData.SecidSectablesDict[sectable.sctid].FirstOrDefault(secs => secs.sec == spot.spotlength, null);

                        /*if (sec != null)
                            seccoef += sec.coef * ((decimal)spot.spotlength / 30);
                        else
                            seccoef += 1.0 * ((decimal)spot.spotlength / 30);*/
                        if (sec != null)
                            seccoef += sec.coef;
                        else
                        {
                            //mediaPlan.Seccoef = 0.0;
                            //return;
                            //mediaPlan.Seccoef += (decimal)spot.spotlength/30;
                            seccoef += (decimal)spot.spotlength / 30;
                        }
                        secCount += 1;
                    }
                }            
            }

            if (secCount == 0)
            {
                mediaPlan.Seccoef = 1.0M;
            }
            else
            {
                mediaPlan.Seccoef = seccoef / secCount;
            }
        }

        private void CalculateAvgSeascoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            /*var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);

            var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);*/

            var seasonality = _forecastData.PlidSeasonalityDict[pricelist.plid];
            var seasonalities = _forecastData.SeasidSeasonalitiesDict[seasonality.seasid];

            decimal seasCoef = 0;
            int seasCount = 0;
            foreach (var term in terms)
            {
                if (term != null)
                {
                    foreach (var seas in seasonalities)
                    {
                        if (term.date >= DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(seas.stdt).Date) &&
                            term.date <= DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(seas.endt).Date))
                        {
                            seasCount += 1;
                            seasCoef += seas.coef;
                        }
                    }
                }
                    
                

            }

            if (seasCount == 0)
            {
                mediaPlan.Seascoef = 1.0M;
            }
            else
            {
                mediaPlan.Seascoef = seasCoef / seasCount;
            }
        }

        private void CalculateAvgCbrcoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            var channelCobrands = _forecastData.Cobrands.Where(cbr => cbr.chid == mediaPlan.chid);
            var spotcodes = _forecastData.Spots.Select(s => s.spotcode[0]);
            decimal sumCoef = 0;
            int count = 0;

            foreach (char spotcode in spotcodes)
            {
                int spotcodeCount = terms.Where(t => t != null && t.spotcode != null).Sum(t => t.spotcode.Count(sc => sc == spotcode));
                count += spotcodeCount;

                decimal coef = 1.0M;
                var spotCobrand = channelCobrands.FirstOrDefault(cbr => cbr.spotcode == spotcode);
                if (spotCobrand != null)
                    coef = spotCobrand.coef;

                sumCoef += spotcodeCount * coef;
            }          

            if (count == 0)
            {
                mediaPlan.Cbrcoef = 1.0M;
            }
            else
            {
                mediaPlan.Cbrcoef = sumCoef / count;
            }
        }

        public MediaPlanDTO ConvertToDTO(MediaPlan mediaPlan)
        {
            MediaPlanDTO mediaPlanDTO = new MediaPlanDTO(mediaPlan.xmpid, mediaPlan.schid, mediaPlan.cmpid,
                mediaPlan.chid, mediaPlan.name, mediaPlan.version, mediaPlan.position, mediaPlan.stime, 
                mediaPlan.etime, mediaPlan.blocktime, mediaPlan.days, mediaPlan.type, mediaPlan.special, 
                mediaPlan.sdate, mediaPlan.edate, mediaPlan.Progcoef, mediaPlan.created, mediaPlan.modified, 
                mediaPlan.Amr1, mediaPlan.Amr1trim, mediaPlan.Amr2, mediaPlan.Amr2trim, mediaPlan.Amr3, 
                mediaPlan.Amr3trim, mediaPlan.Amrsale, mediaPlan.Amrsaletrim, mediaPlan.Amrp1, mediaPlan.Amrp2,
                mediaPlan.Amrp3, mediaPlan.Amrpsale, mediaPlan.Dpcoef, mediaPlan.Seascoef, mediaPlan.Seccoef,
                mediaPlan.coefA, mediaPlan.coefB, mediaPlan.cbrcoef, mediaPlan.Price, mediaPlan.active, mediaPlan.PricePerSecond);

            return mediaPlanDTO;
        }

        public async Task SetOutliers(IEnumerable<MediaPlanHist> mediaPlanHistList)
        {
            if (mediaPlanHistList.Count() == 0)
                return;

            // Calculate the median and median absolute deviation (MAD) of the amrp1 attribute
            decimal median = CalculateMedian(mediaPlanHistList.Select(x => x.amrp1).ToList());
            decimal mad = CalculateMAD(mediaPlanHistList.Select(x => x.amrp1).ToList(), median);

            List<MediaPlanHist> outliers = new List<MediaPlanHist>();
            // Set the threshold for outlier detection
            decimal threshold = 3.5M; // Adjust this value based on your requirements
                                      // Check if MAD is zero
            if (mad == 0)
            {
                // Handle the case when MAD is zero (e.g., assign a default value)
                // For example, you can set the threshold to a very large value to ignore outliers
                threshold = decimal.MaxValue; // Set to a very large value

                // Find the outliers in the list
                outliers = mediaPlanHistList.Where(x => Math.Abs(x.amrp1 - median) > threshold).ToList();
            }
            else
            {
                // Set the threshold for outlier detection
                threshold = 3.5M; // Adjust this value based on your requirements

                // Find the outliers in the list
                outliers = mediaPlanHistList.Where(x =>
                    Math.Abs(x.amrp1 - median) / mad > threshold).ToList();
            }
            // Find the outliers in the list
            /*outliers = mediaPlanHistList.Where(x =>
                Math.Abs(x.amrp1 - median) / mad > threshold).ToList();*/

            /*if (mad == 0)
            {
                var a = CalculateMAD(mediaPlanHistList.Select(x => x.amrp1).ToList(), median);
            }*/
            foreach (var outlier in outliers)
            {
                outlier.outlier = true;
                await _mediaPlanHistController.UpdateMediaPlanHist(new UpdateMediaPlanHistDTO(outlier));
            }
        }

        public decimal CalculateMedian(List<decimal> values)
        {
            List<decimal> sortedValues = values.OrderBy(x => x).ToList();
            int count = sortedValues.Count;

            if (count % 2 == 0)
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0M;
            else
                return sortedValues[count / 2];
        }

        public decimal CalculateMAD(List<decimal> values, decimal median)
        {
            List<decimal> absoluteDeviations = values.Select(x => Math.Abs(x - median)).ToList();
            decimal mad = CalculateMedian(absoluteDeviations);
            return mad;
        }

        public void CopyValues(MediaPlan originalMP, MediaPlan copyMP)
        {
            originalMP.xmpid = copyMP.xmpid;
            originalMP.schid = copyMP.schid;
            originalMP.cmpid = copyMP.cmpid;
            originalMP.chid = copyMP.chid;
            originalMP.Name = copyMP.Name;
            originalMP.version = copyMP.version;
            originalMP.Position = copyMP.Position;
            originalMP.Stime = copyMP.Stime;
            originalMP.Etime = copyMP.Etime;
            originalMP.Blocktime = copyMP.Blocktime;
            originalMP.days = copyMP.days;
            originalMP.Type = copyMP.Type;
            originalMP.Special = copyMP.Special;
            originalMP.sdate = copyMP.sdate;
            originalMP.edate = copyMP.edate;
            originalMP.Progcoef = copyMP.Progcoef;
            originalMP.created = copyMP.created;
            originalMP.modified = copyMP.modified;
            originalMP.Amr1trim = copyMP.Amr1trim;
            originalMP.Amr1 = copyMP.Amr1;
            originalMP.Amr2trim = copyMP.Amr2trim;
            originalMP.Amr2 = copyMP.Amr2;
            originalMP.Amr3trim = copyMP.Amr3trim;
            originalMP.Amr3 = copyMP.Amr3;
            originalMP.Amrsaletrim = copyMP.Amrsaletrim;
            originalMP.Amrsale = copyMP.Amrsale;
            originalMP.Amrp1 = copyMP.Amrp1;
            originalMP.Amrp2 = copyMP.Amrp2;
            originalMP.Amrp3 = copyMP.Amrp3;
            originalMP.Amrpsale = copyMP.Amrpsale;
            originalMP.Chcoef = copyMP.Chcoef;
            originalMP.Dpcoef = copyMP.Dpcoef;
            originalMP.Seascoef = copyMP.Seascoef;
            originalMP.Seccoef = copyMP.Seccoef;
            originalMP.CoefA = copyMP.CoefA;
            originalMP.CoefB = copyMP.CoefB;
            originalMP.Price = copyMP.Price;
            originalMP.active = copyMP.active;
            originalMP.PricePerSecond = copyMP.PricePerSecond;
            originalMP.Cpp = copyMP.Cpp;
            originalMP.Length = copyMP.Length;
            originalMP.Insertations = copyMP.Insertations;
        }

        public MediaPlan CopyMP(MediaPlan mediaPlan)
        {
            MediaPlan mediaPlanCopy = new MediaPlan();

            CopyValues(mediaPlanCopy, mediaPlan);

            return mediaPlanCopy;
        }

        public bool SameMPValues(MediaPlan mediaPlan1, MediaPlan mediaPlan2)
        {

            decimal eps = 0.0001M;

            return
            mediaPlan1.xmpid == mediaPlan2.xmpid &&
            mediaPlan1.schid == mediaPlan2.schid &&
            mediaPlan1.cmpid == mediaPlan2.cmpid &&
            mediaPlan1.chid == mediaPlan2.chid &&
            mediaPlan1.name.Trim() == mediaPlan2.name.Trim() &&
            mediaPlan1.version == mediaPlan2.version &&
            mediaPlan1.position == mediaPlan2.position &&
            mediaPlan1.stime == mediaPlan2.stime &&
            mediaPlan1.etime == mediaPlan2.etime &&
            mediaPlan1.blocktime == mediaPlan2.blocktime &&
            mediaPlan1.days == mediaPlan2.days &&
            mediaPlan1.type == mediaPlan2.type &&
            mediaPlan1.special == mediaPlan2.special &&
            mediaPlan1.sdate == mediaPlan2.sdate &&
            mediaPlan1.edate == mediaPlan2.edate &&
            mediaPlan1.progcoef == mediaPlan2.progcoef &&
            mediaPlan1.created == mediaPlan2.created &&
            mediaPlan1.modified == mediaPlan2.modified &&
            (mediaPlan1.amr1 - mediaPlan2.amr1 < eps) &&
            mediaPlan1.amr1trim == mediaPlan2.amr1trim &&
            (mediaPlan1.amr2 - mediaPlan2.amr2 < eps) &&
            mediaPlan1.amr2trim == mediaPlan2.amr2trim &&
            (mediaPlan1.amr3 - mediaPlan2.amr3 < eps) &&
            mediaPlan1.amr3trim == mediaPlan2.amr3trim &&
            (mediaPlan1.amrsale - mediaPlan2.amrsale < eps) &&
            mediaPlan1.amrsaletrim == mediaPlan2.amrsaletrim &&
            (mediaPlan1.amrp1 - mediaPlan2.amrp1 < eps) &&
            (mediaPlan1.amrp2 - mediaPlan2.amrp2 < eps) &&
            (mediaPlan1.amrp3 - mediaPlan2.amrp3 < eps) &&
            (mediaPlan1.amrpsale - mediaPlan2.amrpsale < eps) &&
            mediaPlan1.chcoef == mediaPlan2.chcoef &&
            mediaPlan1.dpcoef == mediaPlan2.dpcoef &&
            mediaPlan1.seascoef == mediaPlan2.seascoef &&
            mediaPlan1.seccoef == mediaPlan2.seccoef &&
            mediaPlan1.coefA == mediaPlan2.coefA &&
            mediaPlan1.coefB == mediaPlan2.coefB &&
            mediaPlan1.price == mediaPlan2.price &&
            mediaPlan1.active == mediaPlan2.active &&
            mediaPlan1.pps == mediaPlan2.pps &&
            mediaPlan1.Cpp == mediaPlan2.Cpp &&
            mediaPlan1.Length == mediaPlan2.Length &&
            mediaPlan1.Insertations == mediaPlan2.Insertations;
        }
    }
}
