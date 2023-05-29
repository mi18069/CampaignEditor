using AutoMapper;
using CampaignEditor.Controllers;
using Database.DTOs.MediaPlanDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public class MediaPlanConverter
    {
        private readonly IMapper _mapper;

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



        public MediaPlanConverter(IMapper mapper, IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanTermRepository mediaPlanTermRepository, ISpotRepository spotRepository,
            IChannelCmpRepository channelCmpRepository, IPricelistRepository pricelistRepository,
            ISeasonalityRepository seasonalityRepository, ISectableRepository sectableRepository,
            ISeasonalitiesRepository seasonalitiesRepository, ISectablesRepository sectablesRepository,
            IPricesRepository pricesRepository)
        {
            _mapper = mapper;

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

        public async Task<MediaPlan> ConvertFromDTO(MediaPlanDTO mediaPlanDTO)
        {
            var mediaPlan = _mapper.Map<MediaPlan>(mediaPlanDTO);

            // Perform additional computations and set extra properties
            await ComputeExtraProperties(mediaPlan);

            return mediaPlan;
        }

        private async Task ComputeExtraProperties(MediaPlan mediaPlan)
        {
            var terms = await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
            var hists = await _mediaPlanHistController.GetAllMediaPlanHistsByXmpid(mediaPlan.xmpid);

            mediaPlan.amr1 = MathFunctions.ArithmeticMean(hists.Select(h => h.amr1));
            mediaPlan.amr2 = MathFunctions.ArithmeticMean(hists.Select(h => h.amr2));
            mediaPlan.amr3 = MathFunctions.ArithmeticMean(hists.Select(h => h.amr3));
            mediaPlan.amrsale = MathFunctions.ArithmeticMean(hists.Select(h => h.amrsale));
            mediaPlan.amrp1 = MathFunctions.ArithmeticMean(hists.Select(h => h.amrp1));
            mediaPlan.amrp2 = MathFunctions.ArithmeticMean(hists.Select(h => h.amrp2));
            mediaPlan.amrp3 = MathFunctions.ArithmeticMean(hists.Select(h => h.amrp3));
            mediaPlan.amrpsale = MathFunctions.ArithmeticMean(hists.Select(h => h.amrpsale));

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
            mediaPlan.Length = insertations == 0 ? 0 : (double)length / insertations;

            var channelCmp = await _channelCmpController.GetChannelCmpByIds(mediaPlan.cmpid, mediaPlan.chid);
            var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
            var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);
            var sectable = await _sectableController.GetSectableById(pricelist.sectbid);

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

            var sectables = await _sectablesController.GetSectablesByIdAndSec(sectable.sctid, length);
            var seccoef = sectables == null ? 1 : sectables.coef;
            mediaPlan.seccoef = seccoef;

            mediaPlan.Cpp = pricelist.price;
            float dpCoef = 1;
            var prices = await _pricesController.GetAllPricesByPlId(pricelist.plid);
            foreach (var price in prices)
            {
                if (TimeFormat.CompareReporesentative(mediaPlan.stime, price.dps) != -1 &&
                    (mediaPlan.etime == null || 
                    TimeFormat.CompareReporesentative(mediaPlan.etime, price.dpe) != 1))
                {
                    dpCoef = price.price;
                }
            }
            mediaPlan.dpcoef = dpCoef;

            mediaPlan.price = mediaPlan.Cpp * mediaPlan.Amrpsale * mediaPlan.Progcoef *
                mediaPlan.Dpcoef * mediaPlan.Seascoef * mediaPlan.Seccoef * mediaPlan.Length;

        }

        public MediaPlanDTO ConvertToDTO(MediaPlan mediaPlan)
        {
            var mediaPlanDTO = _mapper.Map<MediaPlanDTO>(mediaPlan);
            return mediaPlanDTO;
        }
    }
}
