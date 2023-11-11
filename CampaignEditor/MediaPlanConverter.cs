using AutoMapper;
using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.PricelistDTO;
using Database.DTOs.PricesDTO;
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

        public async Task<MediaPlan> ConvertFromDTO(MediaPlanDTO mediaPlanDTO, IEnumerable<MediaPlanTermDTO> terms = null)
        {

            var mediaPlan = _mapperFromDTO.Map<MediaPlanDTO, MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            await ComputeExtraProperties(mediaPlan, terms);

            return mediaPlan;
        }

        public async Task<MediaPlan> ConvertFirstFromDTO(MediaPlanDTO mediaPlanDTO)
        {
            var mediaPlan = _mapperFromDTO.Map<MediaPlanDTO, MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            await CalculateFirst(mediaPlan);

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

        private async Task CalculateSeccoef(MediaPlan mediaPlan, PricelistDTO pricelist)
        {

            var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
            var sectables = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, (int)Math.Ceiling(mediaPlan.AvgLength));
            var seccoef = sectables == null ? 1 : sectables.coef;
            mediaPlan.seccoef = seccoef;
        }

        private async Task CalculateSeascoef(MediaPlan mediaPlan, PricelistDTO pricelist, IEnumerable<MediaPlanTermDTO> terms)
        {
            var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);

            var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);
            double seasCoef = 0;
            int seasCount = 0;
            foreach (var term in terms)
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
            mediaPlan.seascoef = seasCount == 0 ? 1 : seasCoef / seasCount;
        }

        public async Task CalculateLengthAndInsertations(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms)
        {
            var insertations = 0;
            var length = 0;
            foreach (var term in terms)
            {
                if (term != null && term.spotcode != null && term.spotcode.Length > 0)
                {
                    foreach (var spotcode in term.spotcode)
                    {
                        var spot = await _spotController.GetSpotsByCmpidAndCode(mediaPlan.cmpid, spotcode.ToString());
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
                if ((TimeFormat.CompareReporesentative(price.dps, blocktime) != 1) &&
                    (TimeFormat.CompareReporesentative(price.dpe, blocktime) != -1))
                {
                    mediaPlan.Dpcoef = price.price;
                    return;
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
            await ComputeExtraProperties(mediaPlan);
            
        }

        public async Task ComputeExtraProperties(MediaPlan mediaPlan, IEnumerable<MediaPlanTermDTO> terms = null)
        {
            var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
            if (terms == null)
            {
                terms = await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
            }

            await CalculateLengthAndInsertations(mediaPlan, terms);
            await CalculateSeccoef(mediaPlan, pricelist);
            await CalculateSeascoef(mediaPlan, pricelist, terms);
            CalculatePrices(mediaPlan, pricelist);

        }

        private void CalculatePrices(MediaPlan mediaPlan, PricelistDTO pricelist)
        {
            // For seconds type pricelists
            if (pricelist.pltype == 1)
            {

                mediaPlan.PricePerSecond = mediaPlan.Dpcoef / mediaPlan.Amrp1;
                double coefs = mediaPlan.Progcoef * mediaPlan.Dpcoef * mediaPlan.Seascoef * mediaPlan.Seccoef;
                mediaPlan.Price = coefs * mediaPlan.Length;
                mediaPlan.Cpp = mediaPlan.Price / (mediaPlan.Amrp1);

            }
            // For cpp pricelists
            else if (pricelist.pltype == 0)
            {
                mediaPlan.Cpp = pricelist.price;
                mediaPlan.Price = (pricelist.price / 30) * mediaPlan.Length * mediaPlan.Amrpsale * mediaPlan.Progcoef *
                    mediaPlan.Dpcoef * mediaPlan.Seascoef * mediaPlan.Seccoef;
                if (mediaPlan.Length > 0)
                    mediaPlan.PricePerSecond = mediaPlan.Price / mediaPlan.Length;
                else
                    mediaPlan.PricePerSecond = 0;
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
    }
}
