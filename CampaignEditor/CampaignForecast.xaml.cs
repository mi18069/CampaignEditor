using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private MediaPlanTermController _mediaPlanTermController;

        private ClientDTO _client;
        private CampaignDTO _campaign;

        private DateTime initFrom;
        private DateTime initTo;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        private Dictionary<ChannelDTO, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>> _channelMPDict =
            new Dictionary<ChannelDTO, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>>();
        private List<SchemaDTO> _schemaList = new List<SchemaDTO>();
        private ObservableCollection<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>> _showMP 
            = new ObservableCollection<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>();
        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository, ICampaignRepository campaignRepository, 
            IChannelCmpRepository channelCmpRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository )
        {
            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _campaignController = new CampaignController(campaignRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);

            InitializeComponent();
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

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
            InitializeDateColumns();

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

                var mediaPlans = new List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>();
                foreach (var schema in schemas)
                {
                    MediaPlanDTO mediaPlan = await SchemaToMP(schema);
                    var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlan); 
                    mediaPlans.Add(Tuple.Create(mediaPlan, mediaPlanTerms));
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

        private async Task<List<MediaPlanTermDTO>> MediaPlanToMPTerm(MediaPlanDTO mediaPlan)
        {

            List<DateTime> availableDates = GetAvailableDates(mediaPlan);
            DateTime started = startDate;

            int n = (int)(endDate - startDate).TotalDays;
            var mediaPlanDates = new List<MediaPlanTermDTO>();

            List<DateTime> sorted = availableDates.OrderBy(d => d).ToList();

            for (int i = 0, j = 0; i < n && j < sorted.Count(); i++)
            {
                if (started.AddDays(i).Date == sorted[j].Date)
                {
                    CreateMediaPlanTermDTO mpTerm = new CreateMediaPlanTermDTO(mediaPlan.xmpid, DateOnly.FromDateTime(sorted[j]), null);
                    mediaPlanDates.Add(await _mediaPlanTermController.CreateMediaPlanTerm(mpTerm));
                    j++;
                }
            }

            return mediaPlanDates;
        }
        private List<DateTime> GetAvailableDates(MediaPlanDTO mediaPlan)
        {
            List<DateTime> dates = new List<DateTime>();


            foreach (char c in mediaPlan.days)
            {
                switch (c)
                {
                    case '1':
                        var mondays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Monday);
                        foreach (DateTime date in mondays)
                            dates.Add(date);
                        break;
                    case '2':
                        var tuesdays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Tuesday);
                        foreach (DateTime date in tuesdays)
                            dates.Add(date);
                        break;
                    case '3':
                        var wednesdays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Wednesday);
                        foreach (DateTime date in wednesdays)
                            dates.Add(date);
                        break;
                    case '4':
                        var thursdays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Thursday);
                        foreach (DateTime date in thursdays)
                            dates.Add(date);
                        break;
                    case '5':
                        var fridays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Friday);
                        foreach (DateTime date in fridays)
                            dates.Add(date);
                        break;
                    case '6':
                        var saturdays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Saturday);
                        foreach (DateTime date in saturdays)
                            dates.Add(date);
                        break;
                    case '7':
                        var sundays = GetWeekdaysBetween(startDate, endDate, DayOfWeek.Sunday);
                        foreach (DateTime date in sundays)
                            dates.Add(date);
                        break;
                }

            }
            return dates;

        }

        private List<DateTime> GetWeekdaysBetween(DateTime startDate, DateTime endDate, DayOfWeek dayOfWeek)
        {
            var dates = new List<DateTime>();

            // calculate the number of days between the start date and the next occurrence of the day of the week
            var daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;

            // get the first date in the range
            var date = startDate.AddDays(daysToAdd);

            // add the day of the week repeatedly to get all the dates in the range
            while (date <= endDate)
            {
                dates.Add(date);
                date = date.AddDays(7);
            }

            return dates;
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
                    Tuple<MediaPlanDTO, List<MediaPlanTermDTO>> mediaPlanTuple = _channelMPDict[selectedItem][k];
                    _showMP.Add(mediaPlanTuple);
                }
            }

            if (deselectedItems.Count>0)
            {
                ChannelDTO deselectedItem = deselectedItems[0]! as ChannelDTO;

                for (int k = 0; k < _channelMPDict[deselectedItem].Count;k++)
                {
                    Tuple<MediaPlanDTO, List<MediaPlanTermDTO>> mediaPlanTuple = _channelMPDict[deselectedItem][k];
                    _showMP.Remove(mediaPlanTuple);
                }
            }
        }
        private void InitializeDateColumns()
        {
            DateTime startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            DateTime endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            // Get a list of all dates between start and end date, inclusive
            List<DateTime> dates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
            .Select(offset => startDate.AddDays(offset))
                                  .ToList();

            // Create a column for each date
            foreach (DateTime date in dates)
            {
                // Create a new DataGridTextColumn
                DataGridTextColumn column = new DataGridTextColumn();

                // Set the column header to the date
                column.Header = date.ToString("dd/MM/yyyy");


                // Add the column to the DataGrid
                dgSchema.Columns.Add(column);
            }

        }
    }
}
