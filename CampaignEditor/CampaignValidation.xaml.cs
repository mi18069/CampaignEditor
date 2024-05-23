using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Page in which we'll graphically see times of reserved and realized terms
    /// </summary>
    public partial class CampaignValidation : Page
    {

        private ChannelCmpController _channelCmpController;
        private ChannelController _channelController;
        private MediaPlanVersionController _mediaPlanVersionController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private MediaPlanRealizedController _mediaPlanRealizedController;
        private SpotController _spotController;
        private DatabaseFunctionsController _databaseFunctionsController;
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;

        private List<ChannelDTO> _channels = new List<ChannelDTO>();
        private List<DateOnly> _dates = new List<DateOnly>();

        public MediaPlanForecastData _forecastData;
        public MediaPlanConverter _mpConverter;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        private ObservableRangeCollection<MediaPlanRealized> _mediaPlanRealized = new ObservableRangeCollection<MediaPlanRealized>();
        private Dictionary<int, string> _spotnumNameDict = new Dictionary<int, string>();

        public CampaignValidation(
            IChannelCmpRepository channelCmpRepository,
            IChannelRepository channelRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanRealizedRepository mediaPlanRealizedRepository,
            ISpotRepository spotRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IBrandRepository brandRepository,
            ICmpBrndRepository cmpBrndRepository)
        {
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanRealizedController = new MediaPlanRealizedController(mediaPlanRealizedRepository);
            _spotController = new SpotController(spotRepository);
            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);
            _brandController = new BrandController(brandRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {

            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(campaign.cmpid);
            if (mpVersion != null)
            {
                gridNotInitialized.Visibility = System.Windows.Visibility.Collapsed;
                gridValidation.Visibility = System.Windows.Visibility.Visible;

                await FillChannels(campaign);
                await FillDates(campaign);

                await GetMediaPlanRealized(campaign);
                await CheckRealizedPrice();

                validationStack._channelCmpController = _channelCmpController;
                validationStack._channelController = _channelController;
                validationStack._mediaPlanVersionController = _mediaPlanVersionController;
                validationStack._mediaPlanController = _mediaPlanController;
                validationStack._mediaPlanTermController = _mediaPlanTermController;
                validationStack._spotController = _spotController;
                validationStack._channels = _channels;
                validationStack._dates = _dates;
                validationStack._mpConverter = _mpConverter;
                validationStack._forecastData = _forecastData;
                validationStack._allMediaPlans = _allMediaPlans;
                validationStack._mediaPlanRealized = _mediaPlanRealized;
                validationStack._mediaPlanRealizedController = _mediaPlanRealizedController;

                await validationStack.Initialize(campaign);
            }
            else
            {
                gridValidation.Visibility = System.Windows.Visibility.Collapsed;
                gridNotInitialized.Visibility = System.Windows.Visibility.Visible;               
            }
        }

        private async Task FillChannels(CampaignDTO campaign)
        {           
            _channels = _forecastData.Channels;
            FillChannelsComboBox(_channels);
        }

        private void FillChannelsComboBox(IEnumerable<ChannelDTO> channels)
        {
            foreach (ChannelDTO channel in channels)
            {
                ComboBoxItem cbiChannel = new ComboBoxItem();
                cbiChannel.DataContext = channel;
                cbiChannel.Content = channel.chname;

                cbChannels.Items.Add(cbiChannel);
            }
        }

        private async Task FillDates(CampaignDTO campaign)
        {
            var startDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
            var endDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

            if (startDate > endDate)
            {
                return;
            }

            DateTime date = startDate;
            while (date <= endDate)
            {
                _dates.Add(DateOnly.FromDateTime(date));
                date = date.AddDays(1);
            }
        }

        private async Task GetMediaPlanRealized(CampaignDTO campaign)
        {
            var mpRealized = await _mediaPlanRealizedController.GetAllMediaPlansRealizedByCmpid(campaign.cmpid);
            _mediaPlanRealized.ReplaceRange(mpRealized);
        }

        private async Task CheckRealizedPrice()
        {
            foreach (var mediaPlanRealized in _mediaPlanRealized)
            {
                if (mediaPlanRealized.price == null || mediaPlanRealized.status == 5)
                {
                    await CalculateCoefs(mediaPlanRealized);
                }
            }
        }

        private async Task CalculateCoefs(MediaPlanRealized mediaPlanRealized)
        {
            var chid = _forecastData.ChrdsidChidDict[mediaPlanRealized.chid.Value];
            var pricelist = _forecastData.ChidPricelistDict[chid];
            _mpConverter.CalculateRealizedDPCoef(mediaPlanRealized);
            _mpConverter.CalculateRealizedCoefs(mediaPlanRealized, pricelist);


            int spotnum = mediaPlanRealized.spotnum.Value;
            if (_spotnumNameDict.ContainsKey(spotnum))
            {
                mediaPlanRealized.spotname = _spotnumNameDict[spotnum];
            }
            else
            {
                var progName = await _mediaPlanRealizedController.GetDedicatedSpotName(spotnum);
                if (progName != null)
                {
                    _spotnumNameDict[spotnum] = progName.Trim();
                    mediaPlanRealized.spotname = progName.Trim();
                }
            }
            
            
            
        }

        private async void cbChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChannels.SelectedItem is ComboBoxItem selectedChannelItem)
            {
                if (selectedChannelItem.DataContext is ChannelDTO selectedChannel)
                {
                    await validationStack.LoadData(selectedChannel.chid);

                }
            }
        }

        private async void btnCheckRealized_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var campaign = _forecastData.Campaign;
            var cmpBrnd = (await _cmpBrndController.GetCmpBrndsByCmpId(campaign.cmpid)).ToList();
            if (cmpBrnd == null || cmpBrnd.Count == 0)
            {
                MessageBox.Show("No brand is selected for this campaign", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var brandid = cmpBrnd[0].brbrand;
            try
            {
                await _databaseFunctionsController.StartRealizationFunction(campaign.cmpid,
                    brandid, campaign.cmpsdate, campaign.cmpedate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching for new realizations", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Finished searching for new realizations", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);

            cbChannels.SelectedIndex = -1;
            await GetMediaPlanRealized(campaign);
            await CheckRealizedPrice();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintValidation.Print(validationStack._dayTermDict, validationStack._dayRealizedDict);
        }
    }
}
