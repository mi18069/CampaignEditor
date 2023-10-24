using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CampaignEditor.UserControls
{
    public partial class CampaignForecast : Page
    {
        private SchemaController _schemaController;
        private ChannelController _channelController;
        private ChannelCmpController _channelCmpController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private MediaPlanHistController _mediaPlanHistController;
        private MediaPlanRefController _mediaPlanRefController;
        private SpotController _spotController;
        private GoalsController _goalsController;
        private MediaPlanVersionController _mediaPlanVersionController;

        private DatabaseFunctionsController _databaseFunctionsController;

        private readonly IAbstractFactory<AddSchema> _factoryAddSchema;
        private readonly IAbstractFactory<AMRTrim> _factoryAmrTrim;
        private readonly IAbstractFactory<PrintCampaignInfo> _factoryPrintCmpInfo;


        private SelectedMPGoals SelectedMediaPlan = new SelectedMPGoals();

        private bool canUserEdit = true;
        private bool isEditableVersion = true;

        private CampaignDTO _campaign;
        private int _cmpVersion;
        public int CmpVersion { get { return _cmpVersion; } }
        private int _maxVersion = 1;
        private List<int> _versions = new List<int>();
        private List<ChannelDTO> _channels = new List<ChannelDTO>();

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;
        
        private ObservableCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableCollection<MediaPlanTuple>();

        private ObservableRangeCollection<ChannelDTO> _selectedChannels = new ObservableRangeCollection<ChannelDTO>();

        private ObservableCollection<MediaPlanHist> _showMPHist = new ObservableCollection<MediaPlanHist>();

        private MPGoals mpGoals = new MPGoals();

        MediaPlanConverter _mpConverter;
        MediaPlanTermConverter _mpTermConverter;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }


        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository,
            IChannelCmpRepository channelCmpRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanRefRepository mediaPlanRefRepository,
            ISpotRepository spotRepository,
            IGoalsRepository goalsRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IAbstractFactory<AddSchema> factoryAddSchema,
            IAbstractFactory<AMRTrim> factoryAmrTrim,
            IAbstractFactory<MediaPlanConverter> factoryMpConverter,
            IAbstractFactory<MediaPlanTermConverter> factoryMpTermConverter,
            IAbstractFactory<PrintCampaignInfo> factoryPrintCmpInfo)
        {
            this.DataContext = this;

            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanHistController = new MediaPlanHistController(mediaPlanHistRepository);
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _spotController = new SpotController(spotRepository);
            _goalsController = new GoalsController(goalsRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);

            _factoryAddSchema = factoryAddSchema;
            _factoryAmrTrim = factoryAmrTrim;
            _factoryPrintCmpInfo = factoryPrintCmpInfo;

            _mpConverter = factoryMpConverter.Create();
            _mpTermConverter = factoryMpTermConverter.Create();

            InitializeComponent();

            if (MainWindow.user.usrlevel == 2)
            {
                canUserEdit = false;
                btnResetDates.IsEnabled = canUserEdit;
            }
        }


        #region Initialization
        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            if (mpVersion != null)
            {
                _maxVersion = mpVersion.version;
            }

            _cmpVersion = _maxVersion;
            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            dgHist.ItemsSource = _showMPHist;

            SubscribeControllers();
            // Initializing important lists
            InitializeVersions(_maxVersion);
            await InitializeChannels();
            FillCbVersions();

        }

        private void SubscribeControllers()
        {
            SubscribeDataGridControllers();
            SubscribeSGGridControllers(); 
        }


        private void SubscribeDataGridControllers()
        {
            dgMediaPlans._converter = _mpConverter;
            dgMediaPlans._factoryAddSchema = _factoryAddSchema;
            dgMediaPlans._factoryAmrTrim = _factoryAmrTrim;
            dgMediaPlans._schemaController = _schemaController;
            dgMediaPlans._mediaPlanController = _mediaPlanController;
            dgMediaPlans._mediaPlanTermController = _mediaPlanTermController;
            dgMediaPlans._spotController = _spotController;
            dgMediaPlans.AddMediaPlanClicked += dgMediaPlans_AddMediaPlanClicked;
            dgMediaPlans.DeleteMediaPlanClicked += dgMediaPlans_DeleteMediaPlanClicked;
        }

        private void SubscribeSGGridControllers()
        {
            sgGrid._mediaPlanController = _mediaPlanController;
            sgGrid._mediaPlanTermController = _mediaPlanTermController;
            sgGrid._spotController = _spotController;
            sgGrid._channelController = _channelController;
            sgGrid._allMediaPlans = _allMediaPlans;
        }

        private void InitializeVersions(int maxVersion)
        {
            for (int i = 0; i < maxVersion; i++)
            {
                _versions.Add(i + 1);
            }
        }

        private async Task InitializeChannels()
        {
            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            foreach (ChannelCmpDTO chnCmp in channelCmps)
            {
                ChannelDTO channel = await _channelController.GetChannelById(chnCmp.chid);
                _channels.Add(channel);
            }
            _channels = _channels.OrderBy(c => c.chname).ToList();
        }

        // Inserting new Data into database
        public async Task InsertAndLoadData(int version)
        {

            await checkIfMaxVersionChanged(version);

            // Inserting new MediaPlans in database
            List<Task> insertingTasks = new List<Task>();       
            foreach (var channel in _channels)
            {
                Task task = Task.Run(() => InsertInDatabase(channel.chid, version));
                insertingTasks.Add(task);
            }
            // waiting for all tasks to finish
            await Task.WhenAll(insertingTasks);

            // We'll make nChannel threads, and for each thread we'll run startAMRCalculation for each MediaPlan
            List<Task> amrTasks = new List<Task>();
            List<List<MediaPlanDTO>> mediaPlansByChannels = new List<List<MediaPlanDTO>>();
            foreach (ChannelDTO channel in _channels)
            {
                List<MediaPlanDTO> mediaPlans = (await _mediaPlanController.GetAllChannelCmpMediaPlans(channel.chid, _campaign.cmpid, version)).ToList();
                mediaPlansByChannels.Add(mediaPlans);
            }

            foreach (List<MediaPlanDTO> mediaPlanList in mediaPlansByChannels)
            {
                Task task = Task.Run(() => StartAMRByMediaPlan(_campaign.cmpid, 40, 40, mediaPlanList));
                amrTasks.Add(task);
            }
            await Task.WhenAll(amrTasks);
         
            await CalculateMPValues(_campaign.cmpid, version);
            
            await LoadData(version, true);

        }

        public async Task MakeNewVersion(ObservableCollection<MediaPlanTuple> allMediaPlans) 
        {
            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            int newVersion = mpVer.version + 1;
            await _mediaPlanVersionController.IncrementMediaPlanVersion(mpVer);           

            // Make new MediaPlan, MediaPlanTerms and MediaPlanHists in database
            foreach (MediaPlanTuple mediaPlanTuple in allMediaPlans) 
            {
                MediaPlan mp = mediaPlanTuple.MediaPlan;
                MediaPlanDTO mpDTO = _mpConverter.ConvertToDTO(mp);
                CreateMediaPlanDTO createMPDTO = new CreateMediaPlanDTO(mpDTO);
                createMPDTO.version = newVersion;

                var mediaPlan = await _mediaPlanController.CreateMediaPlan(createMPDTO);

                if (mediaPlan != null)
                {
                    // Adding MediaPlanTerms in database
                    foreach (MediaPlanTerm mpTerm in mediaPlanTuple.Terms)
                    {
                        if (mpTerm != null)
                        {
                            MediaPlanTermDTO mpTermDTO = _mpTermConverter.ConvertToDTO(mpTerm);
                            CreateMediaPlanTermDTO createMPTermDTO = new CreateMediaPlanTermDTO(mpTermDTO);
                            createMPTermDTO.xmpid = mediaPlan.xmpid;
                            await _mediaPlanTermController.CreateMediaPlanTerm(createMPTermDTO);
                        }

                    }

                    // Adding MediaPlanHists in database
                    var hists = await _mediaPlanHistController.GetAllMediaPlanHistsByXmpid(mpDTO.xmpid);
                    
                    foreach (var hist in hists) 
                    { 
                        CreateMediaPlanHistDTO createMpHistDTO = new CreateMediaPlanHistDTO(hist);
                        createMpHistDTO.xmpid = mediaPlan.xmpid;
                        await _mediaPlanHistController.CreateMediaPlanHist(createMpHistDTO);
                    }

                }

            }

            await checkIfMaxVersionChanged(newVersion);            
        }

        private async Task DeleteVersion(int fromVersion, int toVersion)
        {
            for (int i = fromVersion; i <= toVersion; i++)
            {
                var mps = await _mediaPlanController.GetAllMediaPlansByCmpid(_campaign.cmpid, i);
                foreach (var mp in mps)
                {
                    await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mp.xmpid);
                    await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(mp.xmpid);
                    await _mediaPlanController.DeleteMediaPlanById(mp.xmpid);
                }
            }
            await _mediaPlanVersionController.UpdateMediaPlanVersion(_campaign.cmpid, fromVersion - 1);
        }

        private async Task StartAMRByMediaPlan(int cmpid, int minusTime, int plusTime, List<MediaPlanDTO> mediaPlans)
        {
            foreach (MediaPlanDTO mediaPlan in mediaPlans)
            {
                await _databaseFunctionsController.StartAMRCalculation(cmpid, minusTime, plusTime, mediaPlan.xmpid);
            }

        }

        private async Task checkIfMaxVersionChanged(int version)
        {

            if (_maxVersion < version)
            {
                cbVersions.Items.Add(version);
                _maxVersion = version;
                cbVersions.SelectedIndex = cbVersions.Items.Count - 1;
            }
            _cmpVersion = version;
            
        }

        public async Task LoadData(int version, bool isEnabled = false)
        {
            _cmpVersion = version;
            isEditableVersion = (_maxVersion == _cmpVersion) || isEnabled;

            // Filling lvChannels and dictionary
            FillLvChannels();
            await FillMPList();
            await FillGoals();
            await FillLoadedDateRanges();
            // For dgMediaPlans
            await InitializeDataGrid();
            var sgGrid = await InitializeSGGrid();
            InitializeSWGGrid(sgGrid);
            await InitializeCGGrid();

        }

        private async Task InitializeDataGrid()
        {
            dgMediaPlans.CanUserEdit = new ObservableBool(canUserEdit && isEditableVersion);
            dgMediaPlans._selectedChannels = _selectedChannels;
            dgMediaPlans._allMediaPlans = _allMediaPlans;
            Dictionary<int, string> chidChannelDictionary = new Dictionary<int, string>();
            foreach (ChannelDTO channel in _channels)
            {
                chidChannelDictionary.Add(channel.chid, channel.chname.Trim());
            }
            dgMediaPlans.chidChannelDictionary = chidChannelDictionary;

            await dgMediaPlans.Initialize(_campaign);
            
        }

        private async Task InitializeCGGrid()
        {
            ObservableCollection<MediaPlan> mediaPlans = new ObservableCollection<MediaPlan>();
            foreach (var mpTuple in _allMediaPlans)
            {
                mediaPlans.Add(mpTuple.MediaPlan);
            }
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (ChannelDTO channel in _channels) 
            {
                channels.Add(channel);
            }
            cgGrid.Initialize(mediaPlans, channels);
            tiChannelGoals.Focus();
        }

        private async Task<SpotGoalsGrid> InitializeSGGrid()
        {
            await sgGrid.Initialize(_campaign);
            tiSpotGoals.IsSelected = true;
            return sgGrid;
        }

        private void InitializeSWGGrid(SpotGoalsGrid sgGrid)
        {
            swgGrid.Initialize(sgGrid);
        }

        

        #region CampaignForecastView

        public event EventHandler InitializeButtonClicked;

        private void OnInitializeButtonClicked()
        {
            InitializeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger the event
            OnInitializeButtonClicked();
        }


        public delegate void ChangeVersionEventHandler(object sender, ChangeVersionEventArgs e);
        public event ChangeVersionEventHandler VersionChanged;
        private void CbVersions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if any items are selected
            int selectedIndex = cbVersions.SelectedIndex;
            int version = selectedIndex + 1; // Versions start from 1
            var eventArgs = new ChangeVersionEventArgs(version);
            VersionChanged?.Invoke(this, eventArgs);

        }

        public async Task DeleteData(int version)
        {
            await _mediaPlanRefController.DeleteMediaPlanRefById(_campaign.cmpid);
            var mediaPlans = await _mediaPlanController.GetAllMediaPlansByCmpid(_campaign.cmpid, version);

            foreach (var mediaPlan in mediaPlans)
            {
                //await DeleteMPById(mediaPlan.xmpid);
                await SetMPToInactive(mediaPlan.xmpid);
            }

        }

        public event EventHandler<ChangeVersionEventArgs> NewVersionClicked;
        private void btnNewVersion_Click(object sender, RoutedEventArgs e)
        {
            int newVersion = _maxVersion + 1;
            var eventArgs = new ChangeVersionEventArgs(newVersion);
            NewVersionClicked?.Invoke(this, eventArgs);

        }

        private async Task SetMPToInactive(int xmpid)
        {
            await _mediaPlanController.SetActiveMediaPlanById(xmpid, false);
        }

        private async Task DeleteMPById(int xmpid)
        {
            await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(xmpid);
            await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(xmpid);
            await _mediaPlanController.DeleteMediaPlanById(xmpid);
        }


        #endregion


        private async Task CalculateMPValues(int cmpid, int version)
        {
            var mediaPlansDTO = await _mediaPlanController.GetAllMediaPlansByCmpid(cmpid, version);

            foreach (var mediaPlanDTO in mediaPlansDTO)
            {
                var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
                await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
            }

        }

        #region Inserting in database
        private async Task InsertInDatabase(int chid, int version)
        {
            var schemas = await _schemaController.GetAllChannelSchemasWithinDateAndTime(
                chid, DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate),
                _campaign.cmpstime, _campaign.cmpetime);

            foreach (var schema in schemas)
            {
                MediaPlanDTO mediaPlan = await SchemaToMP(schema, version);
                var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlan);
            }

        }

        // reaching or creating mediaPlan
        private async Task<MediaPlanDTO> SchemaToMP(SchemaDTO schema, int version)
        {
            MediaPlanDTO mediaPlan = null;
            // if already exist, fix conflicts
            if ((mediaPlan = await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid, version)) != null)
            {
                // if mediaPlan already exists, return that one
                if (AreEqualMediaPlanAndSchema(mediaPlan, schema))
                    return mediaPlan;
                else
                {

                    await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlan.xmpid);
                    await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(mediaPlan.xmpid);
                    await _mediaPlanController.DeleteMediaPlanById(mediaPlan.xmpid);

                    var itemToRemove = _allMediaPlans.Where(item => item.MediaPlan.schid == schema.id).First();
                    _allMediaPlans.Remove(itemToRemove);

                }

            }

            CreateMediaPlanDTO createMediaPlan = new CreateMediaPlanDTO(schema.id, _campaign.cmpid, schema.chid,
            schema.name.Trim(), version, schema.position, schema.stime, schema.etime, schema.blocktime,
            schema.days, schema.type, schema.special, schema.sdate, schema.edate, schema.progcoef,
            schema.created, schema.modified, 0, 100, 0, 100, 0, 100, 0, 100, 0, 0, 0, 0, 1, 1, 1, 0, true);

            return await _mediaPlanController.CreateMediaPlan(createMediaPlan);


        }

        public bool AreEqualMediaPlanAndSchema(MediaPlanDTO plan1, SchemaDTO schema)
        {
            return plan1.schid == schema.id &&
                   plan1.chid == schema.chid &&
                   plan1.name == schema.name &&
                   plan1.position == schema.position &&
                   plan1.stime == schema.stime &&
                   plan1.etime == schema.etime &&
                   plan1.blocktime == schema.blocktime &&
                   plan1.days == schema.days &&
                   plan1.sdate == schema.sdate &&
                   plan1.edate == schema.edate; 
        }

        private async Task<List<MediaPlanTerm>> MediaPlanToMPTerm(MediaPlanDTO mediaPlan)
        {

            List<DateTime> availableDates = GetAvailableDates(mediaPlan);
            DateTime started = startDate;

            int n = (int)(endDate - startDate).TotalDays;
            var mediaPlanDates = new List<MediaPlanTerm>();

            List<DateTime> sorted = availableDates.OrderBy(d => d).ToList();

            for (int i = 0, j = 0; i <= n && j < sorted.Count(); i++)
            {
                if (started.AddDays(i).Date == sorted[j].Date)
                {
                    CreateMediaPlanTermDTO mpTerm = new CreateMediaPlanTermDTO(mediaPlan.xmpid, DateOnly.FromDateTime(sorted[j]), null);
                    mediaPlanDates.Add(_mpTermConverter.ConvertFromDTO(await _mediaPlanTermController.CreateMediaPlanTerm(mpTerm)));
                    j++;
                }
                else
                {
                    mediaPlanDates.Add(null);
                }
            }

            return mediaPlanDates;
        }
        private List<DateTime> GetAvailableDates(MediaPlanDTO mediaPlan)
        {
            List<DateTime> dates = new List<DateTime>();

            var sDate = startDate > mediaPlan.sdate.ToDateTime(TimeOnly.MinValue) ? startDate : mediaPlan.sdate.ToDateTime(TimeOnly.MinValue);
            var eDate = !mediaPlan.edate.HasValue ? endDate : 
                        endDate < mediaPlan.edate.Value.ToDateTime(TimeOnly.MinValue) ? endDate : mediaPlan.edate.Value.ToDateTime(TimeOnly.MinValue);

            foreach (char c in mediaPlan.days)
            {
                switch (c)
                {
                    case '1':
                        var mondays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Monday);
                        foreach (DateTime date in mondays)
                            dates.Add(date);
                        break;
                    case '2':
                        var tuesdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Tuesday);
                        foreach (DateTime date in tuesdays)
                            dates.Add(date);
                        break;
                    case '3':
                        var wednesdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Wednesday);
                        foreach (DateTime date in wednesdays)
                            dates.Add(date);
                        break;
                    case '4':
                        var thursdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Thursday);
                        foreach (DateTime date in thursdays)
                            dates.Add(date);
                        break;
                    case '5':
                        var fridays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Friday);
                        foreach (DateTime date in fridays)
                            dates.Add(date);
                        break;
                    case '6':
                        var saturdays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Saturday);
                        foreach (DateTime date in saturdays)
                            dates.Add(date);
                        break;
                    case '7':
                        var sundays = GetWeekdaysBetween(sDate, eDate, DayOfWeek.Sunday);
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

        #endregion

        #region Filling lists
        private void FillLvChannels()
        {
            lvChannels.Items.Clear();

            foreach (ChannelDTO channel in _channels)
            {
                lvChannels.Items.Add(channel);
            }

        }

        public void FillCbVersions()
        {
            cbVersions.Items.Clear();
            for (int i=0; i<_maxVersion; i++)
            {
                cbVersions.Items.Add(i + 1);
            }
            cbVersions.SelectedIndex = cbVersions.Items.Count-1;
        }


        private async Task FillMPList()
        {
            _allMediaPlans.Clear();

            int n = (int)(endDate - startDate).TotalDays;

            var mediaPlans = await _mediaPlanController.GetAllMediaPlansByCmpid(_campaign.cmpid, _cmpVersion);
            foreach (MediaPlanDTO mediaPlan in mediaPlans)
            {
                List<MediaPlanTermDTO> mediaPlanTerms = (List<MediaPlanTermDTO>)await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
                var mediaPlanDates = new ObservableCollection<MediaPlanTerm>();
                for (int i = 0, j = 0; i <= n && j < mediaPlanTerms.Count(); i++)
                {
                    if (DateOnly.FromDateTime(startDate.AddDays(i)) == mediaPlanTerms[j].date)
                    {
                        mediaPlanDates.Add(_mpTermConverter.ConvertFromDTO(mediaPlanTerms[j]));
                        j++;
                    }
                    else
                    {
                        mediaPlanDates.Add(null);
                    }
                }

                MediaPlanTuple mpTuple = new MediaPlanTuple(await _mpConverter.ConvertFromDTO(mediaPlan), mediaPlanDates);
                _allMediaPlans.Add(mpTuple);
            }

        }
        #endregion

        #endregion

        #region LoadedDateRanges

        private async Task FillLoadedDateRanges()
        {
            spLoadedDateRanges.Children.Clear();

            var dateRanges = await _mediaPlanRefController.GetAllMediaPlanRefsByCmpid(_campaign.cmpid);

            foreach (var dateRange in dateRanges)
            {

                string start = TimeFormat.YMDStringToRepresentative(dateRange.datestart.ToString());
                string end = TimeFormat.YMDStringToRepresentative(dateRange.dateend.ToString());

                Label label = new Label();
                label.Content = start + " - " + end;
                label.FontWeight = FontWeights.Bold;
                label.FontSize = 12;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;

                spLoadedDateRanges.Children.Add(label);

            }
        }

        #endregion

        #region lvChannels
        private void lvChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (ChannelDTO channel in lvChannels.SelectedItems)
            {
                channels.Add(channel);
            }
            _selectedChannels.ReplaceRange(channels);
        }

        #region ContextMenu

        private void lvChannels_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem clickedItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (clickedItem != null)
            {
                // Do something with the clicked item
                var channel = clickedItem.DataContext as ChannelDTO;
                // Call your function for the item

                ContextMenu menu = new ContextMenu();
                MenuItem trimAmrs = new MenuItem();
                trimAmrs.Header = "Trim All Channel Amrs";
                trimAmrs.Click += async (obj, ea) =>
                {
                    var chname = channel.chname;

                    var f = _factoryAmrTrim.Create();
                    f.Initialize("Trim all Amrs for Channel " + chname, 100);
                    f.ShowDialog();
                    if (f.changed)
                    {
                        var mediaPlans = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.chid == channel.chid).Select(mpTuple => mpTuple.MediaPlan);
                        foreach (MediaPlan mediaPlan in mediaPlans)
                        {
                            mediaPlan.Amr1trim = f.newValue;
                            mediaPlan.Amr2trim = f.newValue;
                            mediaPlan.Amr3trim = f.newValue;
                            mediaPlan.Amrsaletrim = f.newValue;
                            var mpDTO = _mpConverter.ConvertToDTO(mediaPlan);
                            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mpDTO));
                        }

                    }
                };
                menu.Items.Add(trimAmrs);

                lvChannels.ContextMenu = menu;
            }

            // Prevent selection and deselection
            e.Handled = true;
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        #endregion

        #endregion

        #region Goals

        private async Task FillGoals()
        {
            GoalsDTO goals = await _goalsController.GetGoalsByCmpid(_campaign.cmpid);
            mpGoals.MediaPlans = new ObservableCollection<MediaPlan>(_allMediaPlans.Select(mp => mp.MediaPlan));
            if (goals != null)
            {
                if (goals.budget == 0)
                {
                    lblBudget.FontWeight = FontWeights.Light;
                    lblBudgetTarget.FontWeight = FontWeights.Light;
                    lblBudgetValue.FontWeight = FontWeights.Light;

                    lblpBudget.FontWeight = FontWeights.Light;
                    lblpBudgetTarget.FontWeight = FontWeights.Light;
                    lblpBudgetValue.FontWeight = FontWeights.Light;
                }
                else
                {
                    lblBudget.FontWeight = FontWeights.Bold;
                    lblBudgetTarget.FontWeight = FontWeights.Bold;
                    lblBudgetValue.FontWeight = FontWeights.Bold;

                    lblpBudget.FontWeight = FontWeights.Bold;
                    lblpBudgetTarget.FontWeight = FontWeights.Bold;
                    lblpBudgetValue.FontWeight = FontWeights.Bold;
                }

                lblBudgetTarget.Content = "/" + (goals.budget != 0 ? goals.budget.ToString() : " - ");
                lblBudgetValue.DataContext = mpGoals;
                // for selectedMP values
                lblpBudgetValue.DataContext = SelectedMediaPlan;
                

                if (goals.grp == 0)
                {
                    lblGRP.FontWeight = FontWeights.Light;
                    lblGRPTarget.FontWeight = FontWeights.Light;
                    lblGRPValue.FontWeight = FontWeights.Light;

                    lblpGRP.FontWeight = FontWeights.Light;
                    lblpGRPTarget.FontWeight = FontWeights.Light;
                    lblpGRPValue.FontWeight = FontWeights.Light;
                }
                else
                {
                    lblGRP.FontWeight = FontWeights.Bold;
                    lblGRPTarget.FontWeight = FontWeights.Bold;
                    lblGRPValue.FontWeight = FontWeights.Bold;

                    lblpGRP.FontWeight = FontWeights.Bold;
                    lblpGRPTarget.FontWeight = FontWeights.Bold;
                    lblpGRPValue.FontWeight = FontWeights.Bold;
                }

                lblGRPTarget.Content = "/" + (goals.grp != 0 ? goals.grp.ToString() : " - ");
                lblGRPValue.DataContext = mpGoals;
                lblpGRPValue.DataContext = SelectedMediaPlan;


                if (goals.ins == 0)
                {
                    lblInsertations.FontWeight = FontWeights.Light;
                    lblInsertationsTarget.FontWeight = FontWeights.Light;
                    lblInsertationsValue.FontWeight = FontWeights.Light;

                    lblpInsertations.FontWeight = FontWeights.Light;
                    lblpInsertationsTarget.FontWeight = FontWeights.Light;
                    lblpInsertationsValue.FontWeight = FontWeights.Light;
                }
                else
                {
                    lblInsertations.FontWeight = FontWeights.Bold;
                    lblInsertationsTarget.FontWeight = FontWeights.Bold;
                    lblInsertationsValue.FontWeight = FontWeights.Bold;

                    lblpInsertations.FontWeight = FontWeights.Bold;
                    lblpInsertationsTarget.FontWeight = FontWeights.Bold;
                    lblpInsertationsValue.FontWeight = FontWeights.Bold;
                }

                lblInsertationsTarget.Content = "/" + (goals.ins != 0 ? goals.ins.ToString() : " - ");
                lblInsertationsValue.DataContext = mpGoals;
                lblpInsertationsValue.DataContext = SelectedMediaPlan;


                if (goals.rch == 0)
                {
                    bReach.Visibility = Visibility.Collapsed;
                    bpReach.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bReach.Visibility = Visibility.Visible;
                    lblReachTarget.Content = "/" + "from " + goals.rch_f1.ToString().Trim() + " to " + goals.rch_f2.ToString().Trim() +
                    " , " + goals.rch.ToString().Trim() + " %";
                    lblReachValue.DataContext = mpGoals;
                }
            }
        }

        #endregion

        #region dgMediaPlans

        private async void dgMediaPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _showMPHist.Clear();
            if (dgMediaPlans.Schema.SelectedItems.Count == 0)
            {
                pGoals.Visibility = Visibility.Collapsed;
                return;
            }

            else
            {
                var mediaPlanTuple = dgMediaPlans.Schema.SelectedItems[0] as MediaPlanTuple;
                var mediaPlan = mediaPlanTuple.MediaPlan;

                var mediaPlanHists = await _mediaPlanHistController.GetAllMediaPlanHistsByXmpid(mediaPlan.xmpid);
                mediaPlanHists = mediaPlanHists.OrderBy(m => m.date).ThenBy(m => m.stime);
                foreach (var mediaPlanHist in mediaPlanHists)
                    _showMPHist.Add(mediaPlanHist);

                pGoals.Visibility = Visibility.Visible;
                SelectedMediaPlan.MediaPlan = mediaPlan;
            }
        }

        private async void dgMediaPlans_AddMediaPlanClicked(object? sender, EventArgs e)
        {
            var f = _factoryAddSchema.Create();
            await f.Initialize(_campaign);
            f.ShowDialog();
            if (f._schema != null)
            {
                try
                {
                    var schema = await _schemaController.CreateGetSchema(f._schema);
                    if (schema != null)
                    {
                        MediaPlanDTO mediaPlanDTO = await SchemaToMP(schema, _cmpVersion);

                        await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);
                        var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
                        var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlanDTO);
                        var mpTuple = new MediaPlanTuple(mediaPlan, new ObservableCollection<MediaPlanTerm>(mediaPlanTerms));
                        _allMediaPlans.Add(mpTuple);
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to create Media Plan", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private async void dgMediaPlans_DeleteMediaPlanClicked(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            if (mediaPlanTuple != null)
            {
                _allMediaPlans.Remove(mediaPlanTuple);
                var mediaPlan = mediaPlanTuple.MediaPlan;
                await DeleteMPById(mediaPlan.xmpid);
            }
        }

        #endregion

        #region Page

        // to prevent page from closing this page on backspace 
        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Back && Keyboard.FocusedElement != null)
            {
                DependencyObject currentElement = Keyboard.FocusedElement as DependencyObject;
                while (currentElement != null)
                {
                    if (currentElement is DataGridCell)
                    {
                        // Focus is on a DataGridCell, do not close the page
                        return;
                    }
                    currentElement = VisualTreeHelper.GetParent(currentElement);
                }
                // Focus is not on a DataGridCell, handle event
                e.Handled = true;
            }
        }

        #endregion
       
        #region DgHist
        private async void DgHistsChbCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!canUserEdit || !isEditableVersion)
            {
                e.Handled = true;
                return;
            }              

            if (sender is DataGridCell cell && cell.DataContext is MediaPlanHist mediaPlanHist)
            {

                // Retrieve the corresponding MediaPlanHist object from the DataContext of the row                
                mediaPlanHist.Active = !mediaPlanHist.Active;

                // TODO: Update the object in the database
                await _mediaPlanHistController.UpdateMediaPlanHist(new UpdateMediaPlanHistDTO(_mediaPlanHistController.ConvertToDTO(mediaPlanHist)));

                // Update MediaPlan accordingly
                var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
                if (mediaPlanTuple != null)
                {
                    var mediaPlan = mediaPlanTuple.MediaPlan;

                    await _mpConverter.CalculateAMRs(mediaPlan);

                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
                }
            }
        }


        #endregion

        private async void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var selectedTabItem = tcGrids.SelectedItem as TabItem;
            tiSpotWeekGoals.IsSelected = true;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(async () =>
            {
                // opened tabItem
                using (var memoryStream = new MemoryStream())
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    using (var excelPackage = new ExcelPackage(memoryStream))
                    {

                        // ... rest of the code to populate the worksheet ...
                        // Create a new worksheet
                        var worksheet0 = excelPackage.Workbook.Worksheets.Add("Campaign Info");
                        var f = _factoryPrintCmpInfo.Create();
                        await f.PrintData(_campaign.cmpid, worksheet0, 0, 0);

                        var worksheet1 = excelPackage.Workbook.Worksheets.Add("Program Schema");
                        dgMediaPlans.PopulateWorksheet(worksheet1, 0, 0);

                        tiSpotGoals.IsSelected = true;
                        var worksheet2 = excelPackage.Workbook.Worksheets.Add("Spot Goals 1");
                        sgGrid.PopulateWorksheet(worksheet2, 0, 0);

                        var worksheet3 = excelPackage.Workbook.Worksheets.Add("Spot Goals 2");
                        swgGrid.PopulateWorksheet(worksheet3, 0, 0);

                        var worksheet4 = excelPackage.Workbook.Worksheets.Add("Channel Goals");
                        cgGrid.PopulateWorksheet(worksheet4, 0, 0);

                        // Save the Excel package to a memory stream
                        excelPackage.SaveAs(memoryStream);
                        // Set the position of the memory stream back to the beginning
                        memoryStream.Position = 0;

                        // Show a dialog to the user for saving or opening the Excel file
                        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Filter = "Excel Files (*.xlsx)|*.xlsx",
                            DefaultExt = "xlsx"
                        };

                        SaveFile(saveFileDialog, memoryStream);
                    }
              
                }
            }));
            tiSpotGoals.IsSelected = true;

        }

        private void SaveFile(SaveFileDialog saveFileDialog, MemoryStream memoryStream)
        {

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Save the memory stream to a file
                    File.WriteAllBytes(saveFileDialog.FileName, memoryStream.ToArray());

                    try
                    {
                        string filePath = saveFileDialog.FileName;

                        // Open the saved Excel file using the default associated program
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch 
                    {
                        MessageBox.Show("Unable to open Excel file",
                        "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (IOException ex)
                {
                    MessageBox.Show("Unable to change opened Excel file",
                        "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                {
                    MessageBox.Show("Unable to make Excel file",
                        "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the TabControl instance
            var tabControl = (TabControl)sender;

            // Iterate through all TabItems
            foreach (var tabItem in tabControl.Items.OfType<TabItem>())
            {
                // Get the content of the TabItem
                var tabItemContent = tabItem.Content as FrameworkElement;

                // Find the UserControl within the TabItem content
                var userControl = FindVisualChild<UserControl>(tabItemContent);

                // Access the DataGrid within the UserControl
                var dataGrid = FindVisualChild<DataGrid>(userControl);

                // Retrieve the information from the DataGrid
                // ...
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T desiredChild)
                    return desiredChild;

                var foundChild = FindVisualChild<T>(child);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }

    }
}
            
