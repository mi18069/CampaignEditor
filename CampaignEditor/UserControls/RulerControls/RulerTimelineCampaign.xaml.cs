using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace CampaignEditor.UserControls
{
    /// <summary>
    /// For given Campaign and version, get all channels and when one channel and date is selected, show 
    /// RulerTimeline for that channel and that day
    /// </summary>
    public partial class RulerTimelineCampaign : UserControl
    {
        private CampaignDTO _campaign;
        private int _campaignVersion;
        private List<ChannelDTO> _channels = new List<ChannelDTO>();
        private List<DateOnly> _dates = new List<DateOnly>();

        public ChannelCmpController _channelCmpController;
        public ChannelController _channelController;
        public MediaPlanVersionController _mediaPlanVersionController;

        public MediaPlanController _mediaPlanController;
        public MediaPlanTermController _mediaPlanTermController;
        public SpotController _spotController;

        public RulerTimelineCampaign()
        {
            InitializeComponent();
        }


        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(campaign.cmpid);
            if (mpVersion != null)
            {
                int version = mpVersion.version;
                _campaignVersion = version;

                await FillChannels(campaign);
                await FillDays(campaign);

                rulerTimeline._mediaPlanController = _mediaPlanController;
                rulerTimeline._mediaPlanTermController = _mediaPlanTermController;
                rulerTimeline._spotController = _spotController;
                rulerTimeline.Initialize();
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

        private async void cbChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChannels.SelectedItem is ComboBoxItem selectedChannelItem && 
                cbDates.SelectedItem is ComboBoxItem selectedDateItem)
            {
                if (selectedChannelItem.DataContext is ChannelDTO selectedChannel && 
                    selectedDateItem.DataContext is DateOnly selectedDate)
                {

                    await rulerTimeline.LoadData(_campaign.cmpid, selectedChannel.chid, selectedDate, _campaignVersion);
                    
                }
            }
        }

        private async Task FillDays(CampaignDTO campaign)
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
            FillDatesComboBox(_dates);
        }

        private void FillDatesComboBox(List<DateOnly> dates)
        {
            foreach (DateOnly date in dates)
            {
                ComboBoxItem cbiDate = new ComboBoxItem();
                cbiDate.DataContext = date;
                cbiDate.Content = date.ToShortDateString();

                cbDates.Items.Add(cbiDate);
            }
        }

        private async void cbDates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbChannels.SelectedItem is ComboBoxItem selectedChannelItem &&
                cbDates.SelectedItem is ComboBoxItem selectedDateItem)
            {
                if (selectedChannelItem.DataContext is ChannelDTO selectedChannel &&
                    selectedDateItem.DataContext is DateOnly selectedDate)
                {

                    await rulerTimeline.LoadData(_campaign.cmpid, selectedChannel.chid, selectedDate, _campaignVersion);

                }
            }
        }
    }
}
