using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.SchemaDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    public partial class CampaignForecast : Page
    {
        private SchemaController _schemaController;
        private ChannelController _channelController;
        private CampaignController _campaignController;
        private ChannelCmpController _channelCmpController;
        private MediaPlanController _mediaPlanController;

        private ClientDTO _client;
        private CampaignDTO _campaign;

        private DateTime initFrom;
        private DateTime initTo;

        private Dictionary<ChannelDTO, List<MediaPlanDTO>> _channelMPDict =
            new Dictionary<ChannelDTO, List<MediaPlanDTO>>();
        private List<SchemaDTO> _schemaList = new List<SchemaDTO>();
        private ObservableCollection<MediaPlanDTO> _showMP = new ObservableCollection<MediaPlanDTO>();
        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository, ICampaignRepository campaignRepository, 
            IChannelCmpRepository channelCmpRepository,
            IMediaPlanRepository mediaPlanRepository)
        {
            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _campaignController = new CampaignController(campaignRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);

            InitializeComponent();
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;

            if (await _mediaPlanController.GetMediaPlanByCmpId(_campaign.cmpid) == null)
            {
                DateTime now = DateTime.Now;
                dpFrom.SelectedDate = now;
                dpTo.SelectedDate = now;

                gridForecast.Visibility = Visibility.Collapsed;
                gridInit.Visibility = Visibility.Visible;
            }
            else
            {

                
            }

            dgSchema.ItemsSource = _showMP;
        }

        private async Task InitializeData()
        {
            // Filling lvChannels and dictionary
            lvChannels.Items.Clear();
            _channelMPDict.Clear();

            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            foreach (var channelCmp in channelCmps)
            {
                ChannelDTO channel = await _channelController.GetChannelById(channelCmp.chid);
                lvChannels.Items.Add(channel);

                var schemas = await _schemaController.GetAllChannelSchemasWithinDate(channel.chid, DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpsdate)), DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpedate)));

                var mediaPlans = new List<MediaPlanDTO>();
                foreach (var schema in schemas)
                {
                    MediaPlanDTO mediaPlan = await SchemaToMP(schema);
                    var b = mediaPlan;
                    mediaPlans.Add(mediaPlan);
                }
                _channelMPDict.Add(channel, mediaPlans);
            }

            dgSchema.ItemsSource = _showMP;

        }

        // reaching or creating mediaPlan
        private async Task<MediaPlanDTO> SchemaToMP(SchemaDTO schema)
        {
            if (await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid) != null)
                return await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid);
            else
            {
                CreateMediaPlanDTO mediaPlan = new CreateMediaPlanDTO(schema.id, _campaign.cmpid, schema.chid,
                    schema.name.Trim(), 1, schema.position, schema.stime, schema.etime, schema.blocktime,
                    schema.days, schema.type, schema.special, schema.sdate, schema.edate, schema.progcoef,
                    schema.created, schema.modified, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);

                return await _mediaPlanController.CreateMediaPlan(mediaPlan);
            }
        }

        // When we initialize forecast, we need to do set dates for search
        private async void Init_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if ((DateTime)dpFrom.SelectedDate! < (DateTime)dpTo.SelectedDate!)
            {
                initFrom = (DateTime)dpFrom.SelectedDate!;
                initTo = (DateTime)dpTo.SelectedDate!;

                gridInit.Visibility = Visibility.Hidden;
                gridForecast.Visibility = Visibility.Visible;

                await InitializeData();
            }
            else
            {
                MessageBox.Show("Invalid dates");
            }
            


        }

        private void lvChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selectedItems = e.AddedItems;
            var deselectedItems = e.RemovedItems;

            
            if (selectedItems.Count>0) 
            {
                ChannelDTO selectedItem = selectedItems[0]! as ChannelDTO;

                for (int k = 0; k < _channelMPDict[selectedItem].Count; k++)
                {
                    MediaPlanDTO mediaPlan = _channelMPDict[selectedItem][k];
                    _showMP.Add(mediaPlan);
                }
            }

            if (deselectedItems.Count>0)
            {
                ChannelDTO deselectedItem = deselectedItems[0]! as ChannelDTO;

                for (int k = 0; k < _channelMPDict[deselectedItem].Count;k++)
                {
                    MediaPlanDTO mediaPlan = _channelMPDict[deselectedItem][k];
                    _showMP.Remove(mediaPlan);
                }
            }

            // Adding new MediaPlans if new Channel is selected
            /*for (int i=0; i< selectedCount; i++) 
            {
                ChannelDTO selectedChannel = lvChannels.SelectedItems[i] as ChannelDTO;
                bool wasPreviouslySelected = false;
                for (int j=0; j< pSelectedCount; j++) 
                {
                    ChannelDTO pSelectedChannel = _previouslySelectedChannels[j];
                    if (selectedChannel.chid == pSelectedChannel.chid)
                        wasPreviouslySelected = true;
                }
                if (selectedItem )
                {
                    _previouslySelectedChannels.Add(selectedChannel);
                    
                    for (int k=0; k < _channelMPDict[selectedChannel].Count; k++) 
                    {
                        MediaPlanDTO mediaPlan = _channelMPDict[selectedChannel][k];
                        _showMP.Add(mediaPlan);
                    }
                }
            }

            //Deleting mediaPlans for Channels that are no longer selected
            for (int i=0; i< pSelectedCount; i++) 
            {
                ChannelDTO pSelectedChannel = _previouslySelectedChannels[i];
                bool wasUnselected = true;
                for (int j=0; j< selectedCount; j++) 
                {
                    ChannelDTO selectedChannel = lvChannels.SelectedItems[j] as ChannelDTO;
                    if (selectedChannel.chid == pSelectedChannel.chid)
                        wasUnselected = false;
                }
                if (wasUnselected)
                {
                    _previouslySelectedChannels.Remove(pSelectedChannel);

                    for (int k=0; k<_channelMPDict[pSelectedChannel].Count; ) 
                    {
                        MediaPlanDTO mediaPlan = _channelMPDict[pSelectedChannel][k];
                        _showMP.Remove(mediaPlan);
                    }
                }
            }*/
        }
    }
}
