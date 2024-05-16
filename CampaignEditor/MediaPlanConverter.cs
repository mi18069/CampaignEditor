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

                    _plidSpotCoefsDict[Tuple.Create(pricelist.plid, spot)] = new SpotCoefs(dateRanges, seccoef);

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
                    decimal coefs = seascoef * seccoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                    if (pricelist.pltype == 1)
                    {
                        price = coefs * 30;
                    }
                    // For cpp pricelists
                    else
                    {
                        price = (pricelist.price / 30) * spot.spotlength * mediaPlan.Amrpsale * coefs;
                    }

                    SpotCoefsTable spotCoefsTable = new SpotCoefsTable(spot, dateRange,
                                                    seccoef, price);

                    spotCoefsTables.Add(spotCoefsTable);
                }
            }

            return spotCoefsTables;
        }

        public decimal GetProgramSpotPrice(MediaPlan mediaPlan, MediaPlanTerm mpTerm,  SpotDTO spot)
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
                    decimal coefs = seascoef * seccoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                    if (pricelist.pltype == 1)
                    {
                        price = coefs * spot.spotlength;
                    }
                    // For cpp pricelists
                    else
                    {
                        price = (pricelist.price / 30.0M) * spot.spotlength * mediaPlan.Amrpsale * coefs;
                    }
                }
                

            }
            

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

        public static bool ContainsDayInDaysString(DateOnly date, string dayString)
        {
            Dictionary<DayOfWeek, char> daysMap =  new Dictionary<DayOfWeek, char>
                                                    {
                                                        { DayOfWeek.Monday, '1' },
                                                        { DayOfWeek.Tuesday, '2' },
                                                        { DayOfWeek.Wednesday, '3' },
                                                        { DayOfWeek.Thursday, '4' },
                                                        { DayOfWeek.Friday, '5' },
                                                        { DayOfWeek.Saturday, '6' },
                                                        { DayOfWeek.Sunday, '7' },
                                                    };

            char dateChar = daysMap[date.DayOfWeek];
            if (dayString.Contains(dateChar))
                return true;
            return false;
        }

        private async Task CalculateFirst(MediaPlan mediaPlan)
        {
            /*var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);*/

            await CalculateAMRs(mediaPlan);
            //await CalculateDPCoef(mediaPlan, pricelist);
            CalculateDPCoef(mediaPlan);
            await ComputeExtraProperties(mediaPlan, true);

        }

        public async Task ComputeExtraProperties(MediaPlan mediaPlan, bool calculatePrice = false)
        {
            /*var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);*/

            
            var terms = await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);

            ComputeExtraProperties(mediaPlan, terms, calculatePrice);
        }      

        public void ComputeExtraProperties(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms, bool calculatePrice = false)
        {
            SetDayPart(mediaPlan);

            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];
            CalculateLengthAndInsertations(mediaPlan, terms);
            
            if (calculatePrice)
            {
                CalculateAvgSeasSecCoefs(mediaPlan, pricelist, terms);
                CalculatePrice(mediaPlan, pricelist, terms);
            }

            CalculatePricePerSeconds(mediaPlan, pricelist);
            CalculateCPP(mediaPlan, pricelist, terms);

        }

        public void CoefsChanged(MediaPlan mediaPlan)
        {
            /*var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);*/

            var pricelist = _forecastData.ChidPricelistDict[mediaPlan.chid];

            decimal coefs = mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.Seascoef * mediaPlan.Seccoef;


            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {

                mediaPlan.Price = coefs * mediaPlan.Length;
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
                mediaPlan.Price = (pricelist.price / 30) * mediaPlan.Length * mediaPlan.Amrpsale * coefs;
                if (mediaPlan.Length > 0)
                    mediaPlan.PricePerSecond = mediaPlan.Price / mediaPlan.Length;
                else
                    mediaPlan.PricePerSecond = 0;
            }
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

        private void CalculatePricePerSeconds(MediaPlan mediaPlan, PricelistDTO pricelist)
        {

            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {
                decimal coefs = mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.Seascoef;

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

        public void CalculateAvgSeasSecCoefs(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            CalculateAvgSeccoef(mediaPlan, pricelist, terms);
            CalculateAvgSeascoef(mediaPlan, pricelist, terms);
            
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
                        decimal coefs = seascoef * seccoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                        //price += coefs * spotDTO.spotlength;
                        price += coefs * 30;
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
                        decimal coefs = seascoef * seccoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                        if (coefs == 0)
                        {
                            price = 0.0M;
                            break;
                        }
                            
                        price += (pricelist.price / 30) * spotDTO.spotlength * mediaPlan.Amrpsale * coefs;
                    }
                }
                

            }
            mediaPlan.Price = price;

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

        public MediaPlanDTO ConvertToDTO(MediaPlan mediaPlan)
        {
            MediaPlanDTO mediaPlanDTO = new MediaPlanDTO(mediaPlan.xmpid, mediaPlan.schid, mediaPlan.cmpid,
                mediaPlan.chid, mediaPlan.name, mediaPlan.version, mediaPlan.position, mediaPlan.stime, 
                mediaPlan.etime, mediaPlan.blocktime, mediaPlan.days, mediaPlan.type, mediaPlan.special, 
                mediaPlan.sdate, mediaPlan.edate, mediaPlan.Progcoef, mediaPlan.created, mediaPlan.modified, 
                mediaPlan.Amr1, mediaPlan.Amr1trim, mediaPlan.Amr2, mediaPlan.Amr2trim, mediaPlan.Amr3, 
                mediaPlan.Amr3trim, mediaPlan.Amrsale, mediaPlan.Amrsaletrim, mediaPlan.Amrp1, mediaPlan.Amrp2,
                mediaPlan.Amrp3, mediaPlan.Amrpsale, mediaPlan.Dpcoef, mediaPlan.Seascoef, mediaPlan.Seccoef,
                mediaPlan.Price, mediaPlan.active, mediaPlan.PricePerSecond);

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
            originalMP.Amr1 = copyMP.Amr1;
            originalMP.Amr1trim = copyMP.Amr1trim;
            originalMP.Amr2 = copyMP.Amr2;
            originalMP.Amr2trim = copyMP.Amr2trim;
            originalMP.Amr3 = copyMP.Amr3;
            originalMP.Amr3trim = copyMP.Amr3trim;
            originalMP.Amrsale = copyMP.Amrsale;
            originalMP.Amrsaletrim = copyMP.Amrsaletrim;
            originalMP.Amrp1 = copyMP.Amrp1;
            originalMP.Amrp2 = copyMP.Amrp2;
            originalMP.Amrp3 = copyMP.Amrp3;
            originalMP.Amrpsale = copyMP.Amrpsale;
            originalMP.Dpcoef = copyMP.Dpcoef;
            originalMP.Seascoef = copyMP.Seascoef;
            originalMP.Seccoef = copyMP.Seccoef;
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
            mediaPlan1.dpcoef == mediaPlan2.dpcoef &&
            mediaPlan1.seascoef == mediaPlan2.seascoef &&
            mediaPlan1.seccoef == mediaPlan2.seccoef &&
            mediaPlan1.price == mediaPlan2.price &&
            mediaPlan1.active == mediaPlan2.active &&
            mediaPlan1.pps == mediaPlan2.pps &&
            mediaPlan1.Cpp == mediaPlan2.Cpp &&
            mediaPlan1.Length == mediaPlan2.Length &&
            mediaPlan1.Insertations == mediaPlan2.Insertations;
        }
    }
}
