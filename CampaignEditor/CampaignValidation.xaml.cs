using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using CampaignEditor.UserControls;
using Database.DTOs.ChannelDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Page in which we'll graphically see times of reserved and realized ads
    /// </summary>
    public partial class CampaignValidation : Page
    {

        private ChannelCmpController _channelCmpController;
        private ChannelController _channelController;
        private MediaPlanVersionController _mediaPlanVersionController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private SpotController _spotController;

        private List<ChannelDTO> _channels = new List<ChannelDTO>();
        private List<DateOnly> _dates = new List<DateOnly>();


        public CampaignValidation(
            IChannelCmpRepository channelCmpRepository,
            IChannelRepository channelRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            ISpotRepository spotRepository)
        {
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _spotController = new SpotController(spotRepository);

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

                validationStack._channelCmpController = _channelCmpController;
                validationStack._channelController = _channelController;
                validationStack._mediaPlanVersionController = _mediaPlanVersionController;
                validationStack._mediaPlanController = _mediaPlanController;
                validationStack._mediaPlanTermController = _mediaPlanTermController;
                validationStack._spotController = _spotController;
                validationStack._channels = _channels;
                validationStack._dates = _dates;

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
            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(campaign.cmpid);
            List<ChannelDTO> channels = new List<ChannelDTO>();

            foreach (var channelCmp in channelCmps)
            {
                var channel = await _channelController.GetChannelById(channelCmp.chid);
                channels.Add(channel);
            }

            channels = channels.OrderBy(c => c.chname).ToList();

            _channels = channels;
            FillChannelsComboBox(channels);
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
    }
}
