using CampaignEditor.Controllers;
using Database.DTOs.ActivityDTO;
using Database.DTOs.BrandDTO;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.CmpBrndDTO;
using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.PricelistDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public class CampaignOverviewData
    {

        private CmpBrndController _cmpBrndController;
        private BrandController _brandController;

        private TargetCmpController _targetCmpController;
        private TargetController _targetController;

        private SpotController _spotController;

        private GoalsController _goalsController;

        private ChannelCmpController _channelCmpController;
        private ChannelController _channelController;
        private PricelistChannelsController _pricelistChannelsController;
        private PricelistController _pricelistController;
        private ActivityController _activityController;

        private DayPartController _dayPartController;
        private DPTimeController _dpTimeController;

        public CampaignOverviewData(ICmpBrndRepository cmpBrndRepo, IBrandRepository brandRepo,
            ITargetCmpRepository targetCmpRepo, ITargetRepository targetRepo, ISpotRepository spotRepo,
            IGoalsRepository goalsRepo, IChannelCmpRepository channelCmpRepo, IChannelRepository channelRepo,
            IPricelistChannelsRepository pricelistChannelRepo, IPricelistRepository pricelistRepo,
            IActivityRepository activityRepo, IDayPartRepository dayPartRepository, IDPTimeRepository dPTimeRepository)
        {
            _cmpBrndController = new CmpBrndController(cmpBrndRepo);
            _brandController = new BrandController(brandRepo);
            _targetCmpController = new TargetCmpController(targetCmpRepo);
            _targetController = new TargetController(targetRepo);
            _spotController = new SpotController(spotRepo);
            _goalsController = new GoalsController(goalsRepo);
            _channelCmpController = new ChannelCmpController(channelCmpRepo);
            _channelController = new ChannelController(channelRepo);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelRepo);
            _pricelistController = new PricelistController(pricelistRepo);
            _activityController = new ActivityController(activityRepo);
            _dayPartController = new DayPartController(dayPartRepository);
            _dpTimeController = new DPTimeController(dPTimeRepository);
        }

        public async Task<List<TargetDTO>> GetTargets(int cmpid)
        {
            List<TargetDTO> targets = new List<TargetDTO>();

            try
            {
                var targetCmps = await _targetCmpController.GetTargetCmpByCmpid(cmpid);
                targetCmps = targetCmps.OrderBy(tgtCmp => tgtCmp.priority);

                foreach (var targetCmp in targetCmps)
                {
                    var target = await _targetController.GetTargetById(targetCmp.targid);
                    targets.Add(target);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot retrieve data for targets\n {ex.Message}");
            }

            return targets;
        }

        public async Task<List<SpotDTO>> GetSpots(int cmpid)
        {
            List<SpotDTO> spots = new List<SpotDTO>();
            try
            {
                spots = (List<SpotDTO>)await _spotController.GetSpotsByCmpid(cmpid);
                spots = spots.OrderBy(s => s.spotcode).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot retrieve data for spots\n {ex.Message}");
            }

            return spots;
        }

        public async Task<GoalsDTO> GetGoals(int cmpid)
        {
            GoalsDTO goals = null;
            try
            {
                goals = (GoalsDTO)await _goalsController.GetGoalsByCmpid(cmpid);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot retrieve data for goals\n {ex.Message}");
            }

            return goals;
        }

        public async Task<ActivityDTO> GetActivity(CampaignDTO campaign)
        {
            ActivityDTO activity = null;
            try
            {
                activity = await _activityController.GetActivityById(campaign.activity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot retrieve data for goals\n {ex.Message}");
            }

            return activity;
        }

        public async Task<List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>> GetChannelTuples(int cmpid)
        {
            List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> tuples = new List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>();

            try
            {
                var selectedChannels = await _channelCmpController.GetChannelCmpsByCmpid(cmpid);

                foreach (var selected in selectedChannels)
                {
                    ChannelDTO channel = await _channelController.GetChannelById(selected.chid);
                    PricelistDTO pricelist = await _pricelistController.GetPricelistById(selected.plid);
                    ActivityDTO activity = await _activityController.GetActivityById(selected.actid);

                    var tuple = Tuple.Create(channel, pricelist, activity);
                    tuples.Add(tuple);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot retrieve data for channels\n {ex.Message}");
            }

            return tuples;
        }

        public async Task<IEnumerable<BrandDTO>> GetBrands(int cmpid)
        {

            List<BrandDTO> selectedBrands = new List<BrandDTO>();
            try
            {
                var selectedCmpBrnds = await _cmpBrndController.GetCmpBrndsByCmpId(cmpid);

                foreach (var cmpBrnd in selectedCmpBrnds)
                {
                    var brand = await _brandController.GetBrandById(cmpBrnd.brbrand);
                    selectedBrands.Add(brand);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot retrieve data for channels\n {ex.Message}");
            }

            return selectedBrands;
        }

        public async Task<Dictionary<DayPartDTO, List<DPTimeDTO>>> GetClientDayParts(int clid)
        {
            Dictionary<DayPartDTO, List<DPTimeDTO>> dictionary = new Dictionary<DayPartDTO, List<DPTimeDTO>>();

            var dayParts = await _dayPartController.GetAllClientDayParts(clid);
            foreach (var dayPart in dayParts)
            {
                List<DPTimeDTO> dpTimesList = new List<DPTimeDTO>();
                var dpTimes = await _dpTimeController.GetAllDPTimesByDPId(dayPart.dpid);
                foreach (var dpTime in dpTimes) 
                {
                    dpTimesList.Add(dpTime);
                }
                dpTimesList = dpTimesList.OrderBy(dpt => dpt.stime).ToList();
                dictionary[dayPart] = dpTimesList;
            }

            return dictionary;
        }      

    }
}
