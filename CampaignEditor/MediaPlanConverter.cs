using AutoMapper;
using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.PricelistDTO;
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
        private Dictionary<char, SpotDTO> spotcodeSpotDict = new Dictionary<char, SpotDTO>();
        CampaignDTO _campaign;

        private readonly IMapper _mapperFromDTO;

        private MediaPlanHistController _mediaPlanHistController;
        private MediaPlanTermController _mediaPlanTermController;
        private SpotController _spotController;

        private ChannelCmpController _channelCmpController;
        private PricelistController _pricelistController;
        private SeasonalityController _seasonalityController;
        private SectableController _sectableController;
        private SeasonalitiesController _seasonalitiesController;
        private SectablesController _sectablesController;
        private PricesController _pricesController;

        public MediaPlanConverter(IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanTermRepository mediaPlanTermRepository, ISpotRepository spotRepository,
            IChannelCmpRepository channelCmpRepository, IPricelistRepository pricelistRepository,
            ISeasonalityRepository seasonalityRepository, ISectableRepository sectableRepository,
            ISeasonalitiesRepository seasonalitiesRepository, ISectablesRepository sectablesRepository,
            IPricesRepository pricesRepository)
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
            _spotController = new SpotController(spotRepository);

            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _sectableController = new SectableController(sectableRepository);
            _seasonalitiesController = new SeasonalitiesController(seasonalitiesRepository);
            _sectablesController = new SectablesController(sectablesRepository);
            _pricesController = new PricesController(pricesRepository);
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            spotcodeSpotDict.Clear();
            var spots = await _spotController.GetSpotsByCmpid(campaign.cmpid);
            foreach (var spot in spots)
            {
                spotcodeSpotDict.Add(spot.spotcode.Trim()[0], spot);
            }
        }

        public async Task<MediaPlan> ConvertFirstFromDTO(MediaPlanDTO mediaPlanDTO)
        {
            var mediaPlan = _mapperFromDTO.Map<MediaPlanDTO, MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            await CalculateFirst(mediaPlan);

            return mediaPlan;
        }

        public async Task<MediaPlan> ConvertFromDTO(MediaPlanDTO mediaPlanDTO, bool calculatePrice = false, IEnumerable<MediaPlanTermDTO> terms = null)
        {

            var mediaPlan = _mapperFromDTO.Map<MediaPlanDTO, MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            await ComputeExtraProperties(mediaPlan, calculatePrice, terms);

            return mediaPlan;
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
                        var spot = spotcodeSpotDict[spotcode];
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

        private async Task CalculateDPCoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms = null)
        {

            var prices = await _pricesController.GetAllPricesByPlId(pricelist.plid);

            string blocktime = mediaPlan.blocktime ?? mediaPlan.stime;

            foreach (var price in prices)
            {
                // No need to check for day because mediaPlan will be adjusted correctly
                if ((TimeFormat.CompareRepresentative(price.dps, blocktime) != 1) &&
                    (TimeFormat.CompareRepresentative(price.dpe, blocktime) != -1))
                {
                    if (price.days.Trim().Contains(mediaPlan.days.Trim()))
                    {
                        mediaPlan.Dpcoef = price.price;
                        return;
                    }
                }
            }
            
            mediaPlan.Dpcoef = 1.0;
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
            var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);

            await CalculateAMRs(mediaPlan);
            await CalculateDPCoef(mediaPlan, pricelist);
            await ComputeExtraProperties(mediaPlan, true);

        }

        public async Task ComputeExtraProperties(MediaPlan mediaPlan, bool calculatePrice = false, IEnumerable<MediaPlanTermDTO> terms = null)
        {
            var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
            if (terms == null)
            {
                terms = await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
            }

            CalculateLengthAndInsertations(mediaPlan, terms);

            if (calculatePrice)
            {
                await CalculateAvgSeasSecCoefs(mediaPlan, pricelist, terms);
                await CalculatePrice(mediaPlan, pricelist, terms);
            }
            await CalculatePricePerSeconds(mediaPlan, pricelist, terms);
            CalculateCPP(mediaPlan, pricelist);

        }

        public async Task CoefsChanged(MediaPlan mediaPlan)
        {
            var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);

            double coefs = mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.Seascoef * mediaPlan.Seccoef;


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

        private async Task CalculatePrice(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {
                await CalculatePriceSecondsPricelist(mediaPlan, pricelist, terms);                
            }
            // For cpp pricelists
            else
            {
                await CalculatePriceCPPPricelist(mediaPlan, pricelist, terms);               
            }
        }

        private async Task CalculatePricePerSeconds(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {
                double coefs = mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.Seascoef * mediaPlan.Seccoef;

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

        private void CalculateCPP(MediaPlan mediaPlan, PricelistDTO pricelist)
        {
            // For seconds pricelists
            if (pricelist.pltype == 1)
            {
                if (mediaPlan.Amrp1 > 0)
                {
                    mediaPlan.Cpp = mediaPlan.Price / (mediaPlan.Amrp1);
                }
                else
                {
                    mediaPlan.Cpp = 0;
                }
            }
            // For cpp pricelists
            else
            {
                mediaPlan.Cpp = pricelist.price;
            }
        }

        public async Task CalculateAvgSeasSecCoefs(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            await CalculateAvgSeccoef(mediaPlan, pricelist, terms);
            await CalculateAvgSeascoef(mediaPlan, pricelist, terms);
            
        }

        private async Task CalculatePriceSecondsPricelist(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            double price = 0;
            foreach (MediaPlanTermDTO mpTerm in terms)
            {
                if (mpTerm != null && mpTerm.spotcode != null)
                {
                    foreach (char spotcode in mpTerm.spotcode.Trim())
                    {
                        SpotDTO spotDTO = spotcodeSpotDict[spotcode];

                        double seccoef = await CalculateTermSeccoef(mediaPlan, pricelist, spotDTO);
                        double seascoef = await CalculateTermSeascoef(mediaPlan, pricelist, mpTerm);
                        double coefs = seascoef * seccoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                        price += coefs * spotDTO.spotlength;
                    }
                }
               

            }

            mediaPlan.Price = price;
        }

        private async Task CalculatePriceCPPPricelist(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            double price = 0;
            foreach (MediaPlanTermDTO mpTerm in terms)
            {
                if (mpTerm != null && mpTerm.spotcode != null)
                {
                    foreach (char spotcode in mpTerm.spotcode.Trim())
                    {
                        SpotDTO spotDTO = spotcodeSpotDict[spotcode];

                        double seccoef = await CalculateTermSeccoef(mediaPlan, pricelist, spotDTO);
                        double seascoef = await CalculateTermSeascoef(mediaPlan, pricelist, mpTerm);
                        double coefs = seascoef * seccoef * mediaPlan.Progcoef * mediaPlan.Dpcoef;

                        price += (pricelist.price / 30) * spotDTO.spotlength * mediaPlan.Amrpsale * coefs;
                    }
                }
                

            }
            mediaPlan.Price = price;

        }

        public async Task<double> CalculateTermSeccoef(MediaPlan mediaPlan, PricelistDTO pricelist, SpotDTO spotDTO)
        {

            var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
            var sectables = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, spotDTO.spotlength);
            var seccoef = 1.0;
            if (sectable.sctid != 1)
            {
                seccoef = sectables == null ? 1 : sectables.coef * ((double)30 / spotDTO.spotlength);
            }
            
            return seccoef;
        }

        public async Task<double> CalculateTermSeascoef(MediaPlan mediaPlan, PricelistDTO pricelist, MediaPlanTermDTO mpTerm)
        {
            var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);

            var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);
            double seasCoef = 1.0;

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

        private async Task CalculateAvgSeccoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {

            var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
            /*var sectables = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, (int)Math.Ceiling(mediaPlan.AvgLength));
            var seccoef = sectables == null ? 1 : sectables.coef;
            mediaPlan.Seccoef = seccoef;*/
            if (sectable.sctid == 1)
            {
                mediaPlan.Seccoef = 1.0;
                return;
            }

            int secCount = 0;
            double seccoef = 0.0;

            foreach (var term in terms)
            {
                if (term != null && term.spotcode != null)
                {
                    foreach (char c in term.spotcode.Trim())
                    {
                        SpotDTO spot = spotcodeSpotDict[c];
                        var sec = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, spot.spotlength);
                        if (sec != null)
                            seccoef += sec.coef * ((double)30 / spot.spotlength);
                        else
                            seccoef += 1.0;
                        secCount += 1;
                    }
                }            
            }

            if (secCount == 0)
            {
                mediaPlan.Seccoef = 1.0;
            }
            else
            {
                mediaPlan.Seccoef = seccoef / secCount;
            }
        }

        private async Task CalculateAvgSeascoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);

            var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);
            double seasCoef = 0;
            int seasCount = 0;
            foreach (var term in terms)
            {
                if (term != null && term.spotcode != null)
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
                mediaPlan.Seascoef = 1.0;
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
            double median = CalculateMedian(mediaPlanHistList.Select(x => x.amrp1).ToList());
            double mad = CalculateMAD(mediaPlanHistList.Select(x => x.amrp1).ToList(), median);

            // Set the threshold for outlier detection
            double threshold = 3.5; // Adjust this value based on your requirements

            // Find the outliers in the list
            List<MediaPlanHist> outliers = mediaPlanHistList.Where(x =>
                Math.Abs(x.amrp1 - median) / mad > threshold).ToList();

            foreach (var outlier in outliers)
            {
                outlier.outlier = true;
                await _mediaPlanHistController.UpdateMediaPlanHist(new UpdateMediaPlanHistDTO(outlier));
            }
        }

        public double CalculateMedian(List<double> values)
        {
            List<double> sortedValues = values.OrderBy(x => x).ToList();
            int count = sortedValues.Count;

            if (count % 2 == 0)
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
            else
                return sortedValues[count / 2];
        }

        public double CalculateMAD(List<double> values, double median)
        {
            List<double> absoluteDeviations = values.Select(x => Math.Abs(x - median)).ToList();
            double mad = CalculateMedian(absoluteDeviations);

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

            double eps = 0.0001;

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
