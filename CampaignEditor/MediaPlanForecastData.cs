using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using Database.DTOs.PricelistDTO;
using Database.DTOs.PricesDTO;
using Database.DTOs.SeasonalitiesDTO;
using Database.DTOs.SeasonalityDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.SectablesDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.TargetDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public class MediaPlanForecastData
    {
        private CampaignDTO _campaign;
        private List<ChannelDTO> _channels = new List<ChannelDTO>();
        private List<SpotDTO> _spots = new List<SpotDTO>();
        private List<PricelistDTO> _pricelists = new List<PricelistDTO>();
        private List<TargetDTO> _targets = new List<TargetDTO>();

        private Dictionary<char, SpotDTO> _spotcodeSpotDict = new Dictionary<char, SpotDTO>();
        private Dictionary<int, PricelistDTO> _chidPricelistDict = new Dictionary<int, PricelistDTO>();
        private Dictionary<int, decimal> _chidChcoefDict = new Dictionary<int, decimal>();
        private Dictionary<int, int> _chrdsChidDict = new Dictionary<int, int>();
        private Dictionary<int, List<PricesDTO>> _plidPricesDict = new Dictionary<int, List<PricesDTO>>();
        private Dictionary<int, SectableDTO> _plidSectableDict = new Dictionary<int, SectableDTO>();
        private Dictionary<int, List<SectablesDTO>> _secidSectablesDict = new Dictionary<int, List<SectablesDTO>>();
        private Dictionary<int, SeasonalityDTO> _plidSeasonalityDict = new Dictionary<int, SeasonalityDTO>();
        private Dictionary<int, List<SeasonalitiesDTO>> _seasidSeasonalitiesDict = new Dictionary<int, List<SeasonalitiesDTO>>();
        private Dictionary<DayPartDTO, List<DPTimeDTO>> _dayPartsDict = new Dictionary<DayPartDTO, List<DPTimeDTO>>();

        private ChannelController _channelController;
        private SpotController _spotController;
        private ChannelCmpController _channelCmpController;
        private PricelistController _pricelistController;
        private SeasonalityController _seasonalityController;
        private SectableController _sectableController;
        private SeasonalitiesController _seasonalitiesController;
        private SectablesController _sectablesController;
        private PricesController _pricesController;
        private TargetCmpController _targetCmpController;
        private TargetController _targetController;
        private DayPartController _dayPartController;
        private DPTimeController _dpTimeController;
        private PricelistChannelsController _pricelistChannelsController;

        public CampaignDTO Campaign { get {return _campaign; }
            set { _campaign = value; } }
        public List<ChannelDTO> Channels { get { return _channels; } }
        public List<SpotDTO> Spots { get { return _spots; } }
        public List<PricelistDTO> Pricelists { get { return _pricelists; } }
        public List<TargetDTO> Targets { get { return _targets; } }

        public Dictionary<char, SpotDTO> SpotcodeSpotDict { get { return _spotcodeSpotDict; } }
        public Dictionary<int, PricelistDTO> ChidPricelistDict { get { return _chidPricelistDict; } }
        public Dictionary<int, int> ChrdsidChidDict { get { return _chrdsChidDict; } }
        public Dictionary<int, List<PricesDTO>> PlidPricesDict { get { return _plidPricesDict; } }
        public Dictionary<int, SectableDTO> PlidSectableDict { get { return _plidSectableDict; } }
        public Dictionary<int, List<SectablesDTO>> SecidSectablesDict { get { return _secidSectablesDict; } }
        public Dictionary<int, SeasonalityDTO> PlidSeasonalityDict { get { return _plidSeasonalityDict; } }
        public Dictionary<int, List<SeasonalitiesDTO>> SeasidSeasonalitiesDict { get { return _seasidSeasonalitiesDict; } }
        public Dictionary<DayPartDTO, List<DPTimeDTO>> DayPartsDict { get { return _dayPartsDict; } }
        public Dictionary<int, decimal> ChidChcoefDict { get { return _chidChcoefDict; } }

        public MediaPlanForecastData(IChannelRepository channelRepository, ISpotRepository spotRepository,
            IChannelCmpRepository channelCmpRepository, IPricelistRepository pricelistRepository,
            ISeasonalityRepository seasonalityRepository, ISectableRepository sectableRepository,
            ISeasonalitiesRepository seasonalitiesRepository, ISectablesRepository sectablesRepository,
            IPricesRepository pricesRepository, ITargetCmpRepository targetCmpRepository, 
            ITargetRepository targetRepository,
            IDayPartRepository dayPartRepository, IDPTimeRepository dPTimeRepository,
            IPricelistChannelsRepository pricelistChannelsRepository)
        {
            _channelController = new ChannelController(channelRepository);
            _spotController = new SpotController(spotRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _sectableController = new SectableController(sectableRepository);
            _seasonalitiesController = new SeasonalitiesController(seasonalitiesRepository);
            _sectablesController = new SectablesController(sectablesRepository);
            _pricesController = new PricesController(pricesRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
            _targetController = new TargetController(targetRepository);
            _dayPartController = new DayPartController(dayPartRepository);
            _dpTimeController = new DPTimeController(dPTimeRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            await InitializeLists();
        }

        private async Task InitializeLists()
        {
            await InitializeTargets();
            await InitializeSpots();
            await InitializeChannels();
            await InitializePricelists();
            await InitializeDayParts();
        }

        public async Task InitializeTargets()
        {
            _targets.Clear();
            var targetCmps = await _targetCmpController.GetTargetCmpByCmpid(_campaign.cmpid);
            targetCmps = targetCmps.OrderBy(tCmp => tCmp.priority);
            foreach (var targetCmp in targetCmps)
            {
                var target = await _targetController.GetTargetById(targetCmp.targid);
                if (!_targets.Any(t => t.targid == target.targid))
                {
                    _targets.Add(target);
                }
            }
        }

        public async Task InitializeSpots()
        {
            _spots.Clear();
            _spotcodeSpotDict.Clear();

            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            foreach (var spot in spots.Distinct())
            {
                char spotcode = spot.spotcode.Trim()[0];
                _spots.Add(spot);
                _spotcodeSpotDict[spotcode] = spot;
            }

            _spots = _spots.OrderBy(s => s.spotcode).ToList();
        }

        public async Task InitializeChannels(bool addNewPricelist = false)
        {
            _channels.Clear();

            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            foreach (var channelCmp in channelCmps)
            {
                var channel = await _channelController.GetChannelById(channelCmp.chid);
                
                if (channel != null)
                {
                    _channels.Add(channel);

                    // When adding new Channel, there is no need to reinitialize old pricelists
                    // just add new pricelist in dictionary if it's not already in it
                    if (addNewPricelist && !_chidPricelistDict.ContainsKey(channel.chid))
                    {
                        var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                        _chidPricelistDict[channel.chid] = pricelist;
                        _chrdsChidDict[channel.chrdsid] = channel.chid;

                        if (!_pricelists.Any(p => p.plid == pricelist.plid))
                        {
                            _pricelists.Add(pricelist);
                            await AddInPricelistsDictionary(pricelist);
                        }
                    }
                }

            }
            _channels = _channels.OrderBy(ch => ch.chname).ToList();
        }      

        public async Task InitializePricelists()
        {
            _pricelists.Clear();
            _chidPricelistDict.Clear();
            _chrdsChidDict.Clear();

            _plidPricesDict.Clear();
            _plidSectableDict.Clear();
            _secidSectablesDict.Clear();
            _plidSeasonalityDict.Clear();
            _seasidSeasonalitiesDict.Clear();

            foreach (var channel in _channels)
            {
                var channelCmp = await _channelCmpController.GetChannelCmpByIds(_campaign.cmpid, channel.chid);
                var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                _chidPricelistDict[channelCmp.chid] = pricelist;
                _chrdsChidDict[channel.chrdsid] = channel.chid;
                if (!_pricelists.Any(p => p.plid == pricelist.plid))
                {
                    _pricelists.Add(pricelist);
                    await AddInPricelistsDictionary(pricelist);
                }
            }

        }

        private async Task AddInPricelistsDictionary(PricelistDTO pricelist)
        {
            await AddPricesInDict(pricelist);
            await AddSectableInDict(pricelist);
            await AddSeasonalityInDict(pricelist);
            await AddChcoefInDict(pricelist);
        }

        private async Task AddPricesInDict(PricelistDTO pricelist)
        {
            var prices = (await _pricesController.GetAllPricesByPlId(pricelist.plid));
            _plidPricesDict[pricelist.plid] = prices.ToList();
        }

        private async Task AddSectableInDict(PricelistDTO pricelist)
        {
            var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
            _plidSectableDict[pricelist.plid] = sectable;

            if (!_secidSectablesDict.ContainsKey(sectable.sctid))
            {
                var sectables = await _sectablesController.GetSectablesById(sectable.sctid);
                _secidSectablesDict[sectable.sctid] = sectables.ToList();
            }
        }

        private async Task AddSeasonalityInDict(PricelistDTO pricelist)
        {
            var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);
            _plidSeasonalityDict[pricelist.plid] = seasonality;

            if (!_seasidSeasonalitiesDict.ContainsKey(seasonality.seasid))
            {
                var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);
                _seasidSeasonalitiesDict[seasonality.seasid] = seasonalities.ToList();
            }
        }
        private async Task AddChcoefInDict(PricelistDTO pricelist)
        {
            var plchns = await _pricelistChannelsController.GetAllPricelistChannelsByPlid(pricelist.plid);

            foreach (var plchn in plchns)
            {
                _chidChcoefDict[plchn.chid] = plchn.chcoef;
            }
        }
        public async Task InitializeDayParts()
        {
            _dayPartsDict.Clear();
            var dayParts = await _dayPartController.GetAllClientDayParts(_campaign.clid);
            foreach (var dayPart in dayParts)
            {
                List<DPTimeDTO> dpTimesList = new List<DPTimeDTO>();
                var dpTimes = await _dpTimeController.GetAllDPTimesByDPId(dayPart.dpid);
                foreach (var dpTime in dpTimes)
                {
                    dpTimesList.Add(dpTime);
                }
                _dayPartsDict[dayPart] = dpTimesList;
            }
        }
       
    }
}
