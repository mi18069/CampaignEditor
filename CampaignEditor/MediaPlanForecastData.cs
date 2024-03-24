using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.PricelistDTO;
using Database.DTOs.PricesDTO;
using Database.DTOs.SeasonalitiesDTO;
using Database.DTOs.SeasonalityDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.SectablesDTO;
using Database.DTOs.SpotDTO;
using Database.DTOs.TargetDTO;
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
        private Dictionary<int, List<PricesDTO>> _plidPricesDict = new Dictionary<int, List<PricesDTO>>();
        private Dictionary<int, SectableDTO> _plidSectableDict = new Dictionary<int, SectableDTO>();
        private Dictionary<int, List<SectablesDTO>> _secidSectablesDict = new Dictionary<int, List<SectablesDTO>>();
        private Dictionary<int, SeasonalityDTO> _plidSeasonalityDict = new Dictionary<int, SeasonalityDTO>();
        private Dictionary<int, List<SeasonalitiesDTO>> _seasidSeasonalitiesDict = new Dictionary<int, List<SeasonalitiesDTO>>();

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

        public List<ChannelDTO> Channels { get { return _channels; } }
        public List<SpotDTO> Spots { get { return _spots; } }
        public List<PricelistDTO> Pricelists { get { return _pricelists; } }
        public List<TargetDTO> Targets { get { return _targets; } }

        public Dictionary<char, SpotDTO> SpotcodeSpotDict { get { return _spotcodeSpotDict; } }
        public Dictionary<int, PricelistDTO> ChidPricelistDict { get { return _chidPricelistDict; } }
        public Dictionary<int, List<PricesDTO>> PlidPricesDict { get { return _plidPricesDict; } }
        public Dictionary<int, SectableDTO> PlidSectableDict { get { return _plidSectableDict; } }
        public Dictionary<int, List<SectablesDTO>> SecidSectablesDict { get { return _secidSectablesDict; } }
        public Dictionary<int, SeasonalityDTO> PlidSeasonalityDict { get { return _plidSeasonalityDict; } }
        public Dictionary<int, List<SeasonalitiesDTO>> SeasidSeasonalitiesDict { get { return _seasidSeasonalitiesDict; } }


        public MediaPlanForecastData(IChannelRepository channelRepository, ISpotRepository spotRepository,
            IChannelCmpRepository channelCmpRepository, IPricelistRepository pricelistRepository,
            ISeasonalityRepository seasonalityRepository, ISectableRepository sectableRepository,
            ISeasonalitiesRepository seasonalitiesRepository, ISectablesRepository sectablesRepository,
            IPricesRepository pricesRepository, ITargetCmpRepository targetCmpRepository, 
            ITargetRepository targetRepository)
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
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            await InitializeLists();
            await InitializeDictionaries();
        }

        private async Task InitializeLists()
        {
            await InitializeChannels();
            await InitializeSpots();
            await InitializePricelists();
            await InitializeTargets();
        }

        public async Task InitializeChannels()
        {
            _channels.Clear();
            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            foreach (var channelCmp in channelCmps)
            {
                var channel = await _channelController.GetChannelById(channelCmp.chid);
                if (channel != null && !_channels.Any(c => c.chid == channel.chid))
                    _channels.Add(channel);
            }
            _channels = _channels.OrderBy(ch => ch.chname).ToList();
        }

        private async Task InitializeSpots()
        {
            _spots.Clear();
            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            foreach (var spot in spots)
            {
                if (!_spots.Any(s => s.spotcode == spot.spotcode))
                    _spots.Add(spot);
            }
        }

        private async Task InitializePricelists()
        {
            _pricelists.Clear();
            foreach (var channel in _channels)
            {
                var channelCmp = await _channelCmpController.GetChannelCmpByIds(_campaign.cmpid, channel.chid);
                var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                if (!_pricelists.Any(p => p.plid == pricelist.plid))
                {
                    _pricelists.Add(pricelist);
                }
            }
        }

        private async Task InitializeTargets()
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

        private async Task InitializeDictionaries()
        {
            await InitializeSpotcodeSpotDictionary();
            await InitializeChidPricelistDictionary();
            await InitializePlidPricesDictionary();
            await InitializeSectablesDiscionary();
            await InitializeSeasonalitiesDictionary();
        }

        private async Task InitializeSpotcodeSpotDictionary()
        {
            _spotcodeSpotDict.Clear();
            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            foreach (var spot in spots)
            {
                char spotcode = spot.spotcode.Trim()[0];
                if(!_spotcodeSpotDict.ContainsKey(spotcode))
                    _spotcodeSpotDict.Add(spotcode, spot);

            }
        }

        private async Task InitializeChidPricelistDictionary()
        {
            _chidPricelistDict.Clear();
            foreach (var channel in _channels)
            {
                var channelCmp = await _channelCmpController.GetChannelCmpByIds(_campaign.cmpid, channel.chid);
                var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                if (!_chidPricelistDict.ContainsKey(channelCmp.chid))
                {
                    _chidPricelistDict.Add(channelCmp.chid, pricelist);
                }
            }
        }

        private async Task InitializePlidPricesDictionary()
        {
            _plidPricesDict.Clear();
            foreach (PricelistDTO pricelist in _pricelists)
            {
                var prices = (await _pricesController.GetAllPricesByPlId(pricelist.plid)).ToArray();
                if (!_plidPricesDict.ContainsKey(pricelist.plid))
                    _plidPricesDict.Add(pricelist.plid, prices.ToList());
            }
        }

        private async Task InitializeSectablesDiscionary()
        {
            _plidSectableDict.Clear();
            _secidSectablesDict.Clear();
            foreach (var pricelist in _pricelists)
            {
                var sectable = await _sectableController.GetSectableById(pricelist.sectbid);
                _plidSectableDict.Add(pricelist.plid, sectable);
                var sectables = await _sectablesController.GetSectablesById(sectable.sctid);
                if (!_secidSectablesDict.ContainsKey(sectable.sctid))
                    _secidSectablesDict.Add(sectable.sctid, sectables.ToList());
            }

        }

        private async Task InitializeSeasonalitiesDictionary()
        {
            _plidSeasonalityDict.Clear();
            _seasidSeasonalitiesDict.Clear();
            foreach (var pricelist in _pricelists)
            {
                var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);
                _plidSeasonalityDict.Add(pricelist.plid, seasonality);
                var seasonalities = await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);
                if (!_seasidSeasonalitiesDict.ContainsKey(seasonality.seasid))
                    _seasidSeasonalitiesDict.Add(seasonality.seasid, seasonalities.ToList());
            }
        }
    }
}
