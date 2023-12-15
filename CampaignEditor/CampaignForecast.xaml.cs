using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using CampaignEditor.UserControls.ForecastGrids;
using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using static CampaignEditor.Campaign;

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
        private readonly Listing _factoryListing;


        private SelectedMPGoals SelectedMediaPlan = new SelectedMPGoals();

        private bool canUserEdit = true;
        private bool isEditableVersion = true;

        private CampaignDTO _campaign;
        private int _cmpVersion;
        public int CmpVersion { get { return _cmpVersion; } }
        private int _maxVersion = 1;
        private List<int> _versions = new List<int>();
        private ObservableCollection<ChannelDTO> _channels = new ObservableCollection<ChannelDTO>();

        List<DayOfWeek> filteredDays = new List<DayOfWeek> 
        { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
          DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday};

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        private ConcurrentBag<MediaPlanTuple> _concurrentAllMediaPlans = new ConcurrentBag<MediaPlanTuple>();

        private ObservableRangeCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableRangeCollection<MediaPlanTuple>();

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
            IAbstractFactory<PrintCampaignInfo> factoryPrintCmpInfo,
            IAbstractFactory<Listing> factoryListing)
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
            _factoryListing = factoryListing.Create();

            _mpConverter = factoryMpConverter.Create();
            _mpTermConverter = factoryMpTermConverter.Create();

            InitializeComponent();

        }

        // Event handler to forward the mouse wheel event to the ScrollViewer
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        #region Triggers
        public async void OnAddChannelDelegated(object sender, EventArgs e)
        {
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            if (mpVersion != null)
            {
                await InitializeChannels();
                await LoadData(_maxVersion);
            }
        }

        public async void GoalsChanged(object sender, EventArgs e)
        {
            SetLoadingPage?.Invoke(this, null);
            await FillGoals();
            SetContentPage?.Invoke(this, null);

        }

        public async void SpotsChanged(object sender, EventArgs e)
        {
            SetLoadingPage?.Invoke(this, null);
            await dgMediaPlans.InitializeSpots(_campaign.cmpid);
            await InitializeGrids();
            await _mpConverter.Initialize(_campaign);
            SetContentPage?.Invoke(this, null);

        }

        #endregion

        #region Initialization
        public async Task Initialize(CampaignDTO campaign, bool isReadOnly)
        {
            _campaign = campaign;

            canUserEdit = !isReadOnly;

            if (!canUserEdit)
            {
                btnClear.IsEnabled = false;
                btnNewVersion.IsEnabled = false;
                btnFetchData.IsEnabled = false;
                btnRecalculateData.IsEnabled = false;
                btnResetDates.IsEnabled = false;
            }

            CampaignEventLinker.AddForecast(_campaign.cmpid, this);

            await _mpConverter.Initialize(campaign);

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
            FillLvFilterDays();
        }

        private void FillLvFilterDays()
        {
            lvFilterDays.Items.Add(DayOfWeek.Monday);
            lvFilterDays.Items.Add(DayOfWeek.Tuesday);
            lvFilterDays.Items.Add(DayOfWeek.Wednesday);
            lvFilterDays.Items.Add(DayOfWeek.Thursday);
            lvFilterDays.Items.Add(DayOfWeek.Friday);
            lvFilterDays.Items.Add(DayOfWeek.Saturday);
            lvFilterDays.Items.Add(DayOfWeek.Sunday);

        }

        private void lvFilterDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filteredDays.Clear();
            if (lvFilterDays.SelectedItems.Count == 0)
            {
                foreach (DayOfWeek day in lvFilterDays.Items)
                    filteredDays.Add(day);
            }
            else
            {
                foreach (DayOfWeek day in lvFilterDays.SelectedItems)
                    filteredDays.Add(day);
            }


            dgMediaPlans._filteredDays = filteredDays;

            var selectedChannels = lvChannels.SelectedItems.Cast<ChannelDTO>();
            _selectedChannels.ReplaceRange(selectedChannels);
            //_factoryListing.selectedChannels = _selectedChannels.ToList();
        }

        private void SubscribeControllers()
        {
            SubscribeDataGridControllers();
            SubscribeSGGridControllers();
            SubscribeSWGGridControllers();
            SubscribeSDGGridControllers();         
        }


        private void SubscribeDataGridControllers()
        {
            dgMediaPlans._converter = _mpConverter;
            dgMediaPlans._factoryAddSchema = _factoryAddSchema;
            dgMediaPlans._factoryAmrTrim = _factoryAmrTrim;
            dgMediaPlans._schemaController = _schemaController;
            dgMediaPlans._mediaPlanController = _mediaPlanController;
            dgMediaPlans._mediaPlanTermController = _mediaPlanTermController;
            dgMediaPlans._databaseFunctionsController = _databaseFunctionsController;
            dgMediaPlans._mpConverter = _mpConverter;
            dgMediaPlans._spotController = _spotController;
            dgMediaPlans.AddMediaPlanClicked += dgMediaPlans_AddMediaPlanClicked;
            dgMediaPlans.DeleteMediaPlanClicked += dgMediaPlans_DeleteMediaPlanClicked;
            // to open Update dialog
            dgMediaPlans.UpdateMediaPlanClicked += dgMediaPlans_UpdateMediaPlanClicked;
            // when mediaPlan is changed
            dgMediaPlans.UpdatedMediaPlan += dgMediaPlans_UpdatedMediaPlan;
            dgMediaPlans.RecalculateMediaPlan += dgMediaPlans_RecalculateMediaPlan;
            // When updating Terms
            dgMediaPlans.UpdatedTerm += dgMediaPlans_UpdatedTerm;
        }

        private void SubscribeSGGridControllers()
        {
            sgGrid._mediaPlanController = _mediaPlanController;
            sgGrid._mediaPlanTermController = _mediaPlanTermController;
            sgGrid._spotController = _spotController;
            sgGrid._channelController = _channelController;
            sgGrid._allMediaPlans = _allMediaPlans;
        }

        private void SubscribeSWGGridControllers()
        {
            swgGrid._mediaPlanController = _mediaPlanController;
            swgGrid._mediaPlanTermController = _mediaPlanTermController;
            swgGrid._spotController = _spotController;
            swgGrid._channelController = _channelController;
            swgGrid._allMediaPlans = _allMediaPlans;
        }

        private void SubscribeSDGGridControllers()
        {
            sdgGrid._mediaPlanController = _mediaPlanController;
            sdgGrid._mediaPlanTermController = _mediaPlanTermController;
            sdgGrid._spotController = _spotController;
            sdgGrid._channelController = _channelController;
            sdgGrid._allMediaPlans = _allMediaPlans;
        }

        private void InitializeVersions(int maxVersion)
        {
            for (int i = 0; i < maxVersion; i++)
            {
                _versions.Add(i + 1);
            }
        }

        public async Task AddChannels(List<ChannelDTO> channels)
        {
            List<Task> addChannelTask = new List<Task>();
            foreach (var channel in channels)
            {
                Task task = Task.Run(() => InsertDataForChannel(_maxVersion, channel.chid));
                addChannelTask.Add(task);
            }

            await Task.WhenAll(addChannelTask);
            await LoadData(_maxVersion);

        }

        public async Task InsertDataForChannel(int version, int chid)
        {

            // Inserting new MediaPlans in database
            await InsertInDatabase(chid, version);
            List<MediaPlanDTO> mediaPlans = (await _mediaPlanController.GetAllChannelCmpMediaPlans(chid, _campaign.cmpid, version)).ToList();
            await StartAMRByMediaPlan(_campaign.cmpid, 40, 40, mediaPlans);

        }

        private async Task InitializeChannels()
        {
            _channels.Clear();
            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (ChannelCmpDTO chnCmp in channelCmps)
            {
                ChannelDTO channel = await _channelController.GetChannelById(chnCmp.chid);
                channels.Add(channel);
            }

            foreach (var channel in channels)
            {
                _channels.Add(channel);
            }
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
            

            List<List<MediaPlanDTO>> mediaPlansByChannels = new List<List<MediaPlanDTO>>();
            foreach (ChannelDTO channel in _channels)
            {
                List<MediaPlanDTO> mediaPlans = (await _mediaPlanController.GetAllChannelCmpMediaPlans(channel.chid, _campaign.cmpid, version)).ToList();
                mediaPlansByChannels.Add(mediaPlans);
            }
            
            // We'll make nChannel threads, and for each thread we'll run startAMRCalculation for each MediaPlan
            List<Task> amrTasks = new List<Task>();
            foreach (List<MediaPlanDTO> mediaPlanList in mediaPlansByChannels)
            {
                Task task = Task.Run(() => StartAMRByMediaPlan(_campaign.cmpid, 40, 40, mediaPlanList));
                amrTasks.Add(task);
            }
            await Task.WhenAll(amrTasks);


            //await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40);

            await CalculateMPValuesForCampaign(_campaign.cmpid, version);

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
            await InitializeCGGrid();
            await InitializeGrids();
        }

        public async Task InitializeGrids()
        {
            await sgGrid.Initialize(_campaign, _cmpVersion);
            await swgGrid.Initialize(_campaign, _cmpVersion);
            await sdgGrid.Initialize(_campaign, _cmpVersion);
            await _factoryListing.Initialize(_campaign);
            tiSpotGoals.IsSelected = true;
        }

        private async Task InitializeDataGrid()
        {
            dgMediaPlans.CanUserEdit = canUserEdit && isEditableVersion;
            dgMediaPlans._selectedChannels = _selectedChannels;
            dgMediaPlans._allMediaPlans = _allMediaPlans;
            dgMediaPlans._filteredDays = filteredDays;

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

            ObservableCollection<MediaPlan> mediaPlans = new ObservableCollection<MediaPlan>(_allMediaPlans.Select(mp => mp.MediaPlan));    
            cgGrid.Initialize(mediaPlans, _channels.ToList());
            tiChannelGoals.Focus();
        }

        #region Drag and Drop selected Targets
        private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ListViewItem)
                {
                    ListViewItem draggedItem = sender as ListViewItem;
                    DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                }
            }
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e)
        {
            ChannelDTO droppedData = e.Data.GetData(typeof(ChannelDTO)) as ChannelDTO;
            ChannelDTO target = ((ListBoxItem)(sender)).DataContext as ChannelDTO;

            int removedIdx = lvChannels.Items.IndexOf(droppedData);
            int targetIdx = lvChannels.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                _channels.Insert(targetIdx + 1, droppedData);
                _channels.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (_channels.Count + 1 > remIdx)
                {
                    _channels.Insert(targetIdx, droppedData);
                    _channels.RemoveAt(remIdx);
                }
            }

        }

        #endregion

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

        public event EventHandler<ChangeVersionEventArgs> SetLoadingPage;
        public event EventHandler<ChangeVersionEventArgs> SetContentPage;

        #region Helper buttons
        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all spots?", "Result", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                await ClearAllMPTerms();
                lvChannels.SelectedItems.Clear();

            }
        }

        private async Task ClearAllMPTerms()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            foreach (var mediaPlanTuple in _allMediaPlans)
            {
                foreach (var mediaPlanTerm in mediaPlanTuple.Terms)
                {
                    if (mediaPlanTerm == null ||
                        mediaPlanTerm.Spotcode == null)
                    {
                        continue;
                    }
                    else if (mediaPlanTerm.Date > today &&
                        mediaPlanTerm.Spotcode.Trim() != "")
                    {
                        mediaPlanTerm.Spotcode = null;
                        await _mediaPlanTermController.UpdateMediaPlanTerm(
                            new UpdateMediaPlanTermDTO(_mpTermConverter.ConvertToDTO(mediaPlanTerm)));
                    }
                }
                await RecalculateMPValues(mediaPlanTuple.MediaPlan);
            }
        }


        private async Task RecalculateMPValues(MediaPlan mediaPlan)
        {
            await _mpConverter.ComputeExtraProperties(mediaPlan, true);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
        }

        private async void btnFetchData_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Load data from database?", "Result", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetLoadingPage?.Invoke(this, null);

                await LoadData(_maxVersion);

                SetContentPage?.Invoke(this, null);

            }
        }

        private async void btnRecalculateData_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Recalculate all data values?", "Result", MessageBoxButton.OKCancel,
     MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetLoadingPage?.Invoke(this, null);


                // When pricelists or calculating inside MediaPlans are changed 
                await CalculateMPValuesForCampaign(_campaign.cmpid, _maxVersion);

                await LoadData(_maxVersion);

                SetContentPage?.Invoke(this, null);

            }
        }

        #endregion
        

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

        private async Task CalculateMPValuesForCampaign(int cmpid, int version)
        {
            // Start all tasks and gather them in a list
            List<Task> calculatingChannelMPTasks = _channels.Select(channel =>
                Task.Run(() => CalculateMPValuesForChannel(cmpid, channel.chid, version)))
                .ToList();

            // Wait for all tasks to complete
            await Task.WhenAll(calculatingChannelMPTasks);
        }

        private async Task CalculateMPValuesForChannel(int cmpid, int chid, int version)
        {

            var mediaPlansDTO = await _mediaPlanController.GetAllMediaPlansByCmpidAndChannel(cmpid, chid, version);

            foreach (var mediaPlanDTO in mediaPlansDTO)
            {
                await CalculateMPValues(mediaPlanDTO);
            }
        }

        private async Task CalculateMPValues(MediaPlanDTO mediaPlanDTO)
        {
            var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
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
                    if (MessageBox.Show($"New program conflicts with existing:\n{mediaPlan.name}\n" +
                        $"This action will replace existing program with new one", "Result", MessageBoxButton.OKCancel, MessageBoxImage.Warning) 
                        == MessageBoxResult.Cancel)
                    {
                        return mediaPlan;
                    }
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
            schema.created, schema.modified, 0, 100, 0, 100, 0, 100, 0, 100, 0, 0, 0, 0, 1, 1, 1, 0, true, 0);

            return await _mediaPlanController.CreateMediaPlan(createMediaPlan);


        }

        public bool AreEqualMediaPlanAndSchema(MediaPlanDTO plan1, SchemaDTO schema)
        {
            return plan1.schid == schema.id &&
                   plan1.chid == schema.chid &&
                   plan1.name.Trim() == schema.name.Trim() &&
                   plan1.position == schema.position &&
                   plan1.stime == schema.stime &&
                   plan1.etime == schema.etime &&
                   plan1.blocktime == schema.blocktime &&
                   plan1.days == schema.days &&
                   plan1.sdate == schema.sdate &&
                   plan1.edate == schema.edate; 
        }

        private async Task<ObservableArray<MediaPlanTerm?>> MediaPlanToMPTerm(MediaPlanDTO mediaPlan)
        {

            List<DateTime> availableDates = GetAvailableDates(mediaPlan);
            DateTime started = startDate;

            int n = (int)(endDate - startDate).TotalDays;
            var mediaPlanDates = new ObservableArray<MediaPlanTerm?>(n+1);

            List<DateTime> sorted = availableDates.OrderBy(d => d).ToList();

            for (int i = 0, j = 0; i <= n; i++)
            {
                if (j >= sorted.Count())
                {
                    mediaPlanDates[i] = null;
                    continue;
                }
                if (started.AddDays(i).Date == sorted[j].Date)
                {
                    CreateMediaPlanTermDTO mpTerm = new CreateMediaPlanTermDTO(mediaPlan.xmpid, DateOnly.FromDateTime(sorted[j]), null);
                    mediaPlanDates[i] = _mpTermConverter.ConvertFromDTO(await _mediaPlanTermController.CreateMediaPlanTerm(mpTerm));
                    j++;
                }
                else
                {
                    mediaPlanDates[i] = null;
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
            lvChannels.ItemsSource = _channels;

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

        private async Task FillMPListByChannel(int daysNum, int chid)
        {
            var mediaPlansByChannel = await _mediaPlanController.GetAllMediaPlansByCmpidAndChannel(_campaign.cmpid, chid, _cmpVersion);
            if (mediaPlansByChannel.Count() == 0)
            {
                await InsertDataForChannel(_maxVersion, chid);
                mediaPlansByChannel = await _mediaPlanController.GetAllMediaPlansByCmpidAndChannel(_campaign.cmpid, chid, _cmpVersion);
            }
            foreach (MediaPlanDTO mediaPlan in mediaPlansByChannel)
            {
                List<MediaPlanTermDTO> mediaPlanTerms = (List<MediaPlanTermDTO>)await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
                var mediaPlanDates = new ObservableArray<MediaPlanTerm?>(daysNum+1);
                for (int i = 0, j = 0; i <= daysNum; i++)
                {
                    if (j >= mediaPlanTerms.Count())
                    {
                        mediaPlanDates[i] = null;
                        continue;
                    }

                    if (DateOnly.FromDateTime(startDate.AddDays(i)) == mediaPlanTerms[j].date)
                    {
                        mediaPlanDates[i] = _mpTermConverter.ConvertFromDTO(mediaPlanTerms[j]);
                        j++;
                    }
                    else
                    {
                        mediaPlanDates[i] = null;
                    }
                }

                MediaPlanTuple mpTuple = new MediaPlanTuple(await _mpConverter.ConvertFromDTO(mediaPlan), mediaPlanDates);
                _concurrentAllMediaPlans.Add(mpTuple);
            }
        } 
        private async Task FillMPList()
        {
            _allMediaPlans.Clear();
            _concurrentAllMediaPlans.Clear();

            int daysNum = (int)(endDate - startDate).TotalDays;


            List<Task> insertingTasks = new List<Task>();
            foreach (var channel in _channels)
            {
                Task task = Task.Run(() => FillMPListByChannel(daysNum, channel.chid));
                insertingTasks.Add(task);
            }
            // waiting for all tasks to finish
            await Task.WhenAll(insertingTasks);

            _allMediaPlans.ReplaceRange(_concurrentAllMediaPlans);

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

            sdgGrid.SelectedChannelsChanged(_selectedChannels);
            swgGrid.SelectedChannelsChanged(_selectedChannels);
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
                trimAmrs.Header = "Trim Channel Amrs";
                trimAmrs.Click += async (obj, ea) =>
                {
                    var chname = channel.chname;

                    var f = _factoryAmrTrim.Create();
                    f.Initialize("Trim Channel " + chname, 100);
                    f.ShowDialog();
                    if (f.changed)
                    {
                        var mediaPlans = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.chid == channel.chid).Select(mpTuple => mpTuple.MediaPlan);
                        foreach (MediaPlan mediaPlan in mediaPlans)
                        {
                            if (f.attributesToTrim[0])
                                mediaPlan.Amr1trim = f.newValue;
                            if (f.attributesToTrim[1])
                                mediaPlan.Amr2trim = f.newValue;
                            if (f.attributesToTrim[2])
                                mediaPlan.Amr3trim = f.newValue;
                            if (f.attributesToTrim[3])
                                mediaPlan.Amrsaletrim = f.newValue;
                            var mpDTO = _mpConverter.ConvertToDTO(mediaPlan);
                            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mpDTO));
                        }

                    }
                };
                menu.Items.Add(trimAmrs);

                MenuItem recalculateChannel = new MenuItem();
                recalculateChannel.Header = "Recalculate channel values";
                recalculateChannel.Click += async (obj, ea) =>
                {
                    if (MessageBox.Show($"Recalculate channel values for channel:\n{channel.chname.Trim()}?",
                        "Result:", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        SetLoadingPage?.Invoke(this, null);
                        // When pricelists or calculating inside MediaPlans are changed 
                        await CalculateMPValuesForChannel(_campaign.cmpid, channel.chid, _maxVersion);

                        await LoadData(_maxVersion);
                        SetContentPage?.Invoke(this, null);
                    }
                    
                };
                menu.Items.Add(recalculateChannel);

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

        private async void dgMediaPlans_UpdateMediaPlanClicked(object? sender, UpdateMediaPlanTupleEventArgs e)
        {
            MediaPlanTuple mediaPlanTuple = e.MediaPlanTuple;
            MediaPlan mediaPlan = mediaPlanTuple.MediaPlan;

            var f = _factoryAddSchema.Create();
            ChannelDTO selectedChannel = await _channelController.GetChannelById(mediaPlan.chid);
            
            await f.Initialize(_campaign, selectedChannel, mediaPlan);
            f.ShowDialog();
            if (f.updateMediaPlan == true)
            {
                try
                {
                    await UpdateMediaPlanTuple(mediaPlanTuple, true, true);
                }
                catch
                {
                    MessageBox.Show("Unable to update Program", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private async void dgMediaPlans_UpdatedMediaPlan(object? sender, UpdateMediaPlanEventArgs e)
        {
            MediaPlan mediaPlan = e.MediaPlan;
            
            await UpdatedMediaPlan(mediaPlan, true);
        }
        private async Task UpdatedMediaPlan(MediaPlan mediaPlan, bool updateSchema = false)
        {
            if (updateSchema)
            {
                var schema = await _schemaController.GetSchemaById(mediaPlan.schid);
                schema.stime = mediaPlan.stime;
                schema.etime = mediaPlan.etime;
                schema.blocktime = mediaPlan.blocktime;
                await _schemaController.UpdateSchema(new UpdateSchemaDTO(schema));
            }

            await _databaseFunctionsController.StartAMRCalculation(mediaPlan.cmpid, 40, 40, mediaPlan.xmpid);
            var mediaPlanToUpdateDTO = _mpConverter.ConvertToDTO(mediaPlan);
            mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanToUpdateDTO);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
        }

        private async Task UpdateMediaPlanTuple(MediaPlanTuple mediaPlanTuple, bool updateSchema = false, bool initNewTerms = false)
        {
            MediaPlan mediaPlan = mediaPlanTuple.MediaPlan;
            MediaPlanDTO mediaPlanDTO = _mpConverter.ConvertToDTO(mediaPlan);
            var mediaPlanTerms = mediaPlanTuple.Terms;

            if (initNewTerms)
            {
                await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlanDTO.xmpid);
                var newMediaPlanTerms = await MediaPlanToMPTerm(mediaPlanDTO);
                /*mediaPlanTuple.Terms.Clear();

                foreach (var mpTerm in newMediaPlanTerms)
                {
                    mediaPlanTuple.Terms.Add(mpTerm);
                }*/
                mediaPlanTuple.Terms = newMediaPlanTerms;
            }

            if (updateSchema)
            {
                var schema = await _schemaController.GetSchemaById(mediaPlan.schid);
                schema.stime = mediaPlan.stime;
                schema.etime = mediaPlan.etime;
                schema.blocktime = mediaPlan.blocktime;
                await _schemaController.UpdateSchema(new UpdateSchemaDTO(schema));
            }

            await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);

            var mpTupleNew = new MediaPlanTuple(mediaPlan, mediaPlanTerms);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlanDTO));
            var newMediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
            mediaPlanTuple.MediaPlan = newMediaPlan;

            _allMediaPlans.Remove(mediaPlanTuple);
            _allMediaPlans.Add(mpTupleNew);
        }      


        private async void dgMediaPlans_AddMediaPlanClicked(object? sender, EventArgs e)
        {
            var f = _factoryAddSchema.Create();
            ChannelDTO selectedChannel = null;
            if (lvChannels.SelectedItems.Count == 1)
            {
                selectedChannel = lvChannels.SelectedItem as ChannelDTO;
            }
            await f.Initialize(_campaign, selectedChannel);
            f.ShowDialog();
            if (f._schema != null)
            {
                try
                {
                    var schema = await _schemaController.CreateGetSchema(f._schema);
                    if (schema != null)
                    {
                        MediaPlanDTO mediaPlanDTO = await SchemaToMP(schema, _cmpVersion);

                        if (_allMediaPlans.Any(mp => mp.MediaPlan.xmpid == mediaPlanDTO.xmpid))
                        {
                            return;
                        }
                        await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);
                        var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
                        var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlanDTO);
                        var mpTuple = new MediaPlanTuple(mediaPlan, mediaPlanTerms);
                        _allMediaPlans.Add(mpTuple);
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to create Program", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private async void dgMediaPlans_DeleteMediaPlanClicked(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            if (mediaPlanTuple != null)
            {
                if (MessageBox.Show($"Delete program\n{mediaPlanTuple.MediaPlan.Name.Trim()}?", "Result",
    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    _allMediaPlans.Remove(mediaPlanTuple);
                    var mediaPlan = mediaPlanTuple.MediaPlan;
                    await DeleteMPById(mediaPlan.xmpid);
                }
            }
        }

        private async void dgMediaPlans_RecalculateMediaPlan(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            if (mediaPlanTuple != null)
            {
                if (MessageBox.Show($"Recalculate values for program\n{mediaPlanTuple.MediaPlan.Name.Trim()}?", "Result", 
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    var mpDTO = await _mediaPlanController.GetMediaPlanById(mediaPlanTuple.MediaPlan.xmpid);
                    var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mpDTO);
                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
                    mediaPlanTuple.MediaPlan = mediaPlan;

                    _allMediaPlans.Remove(mediaPlanTuple);
                    _allMediaPlans.Add(mediaPlanTuple);
                }
                    
            }
        }

        private async void dgMediaPlans_UpdatedTerm(object? sender, UpdatedTermEventArgs e)
        {
            MediaPlanTerm term = e.Term;
            char? spotcode = e.Spotcode;

            await UpdatedTerm(term, spotcode);
        }

        private async Task UpdatedTerm(MediaPlanTerm term, char? spotcode)
        {
            MediaPlanDTO mediaPlan = await _mediaPlanController.GetMediaPlanById(term.Xmpid);
            ChannelDTO channel = await _channelController.GetChannelById(mediaPlan.chid);
            DateOnly date = term.Date;
            SpotDTO? spot = null;
            if (spotcode.HasValue)
                spot = await _spotController.GetSpotsByCmpidAndCode(_campaign.cmpid, spotcode.ToString());

            sdgGrid.RecalculateGoals(channel, date, spot, true);
            swgGrid.RecalculateGoals(channel, date, spot, true);
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

            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(async () =>
            {
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

                        var worksheet2 = excelPackage.Workbook.Worksheets.Add("Spot Goals 1");
                        sgGrid.PopulateWorksheet(worksheet2, 0, 0);

                        var worksheet3 = excelPackage.Workbook.Worksheets.Add("Spot Goals 2");
                        swgGrid.PopulateWorksheet(worksheet3, 1, 1);

                        var worksheet4 = excelPackage.Workbook.Worksheets.Add("Spot Goals 3");
                        sdgGrid.PopulateWorksheet(worksheet4, 1, 1);

                        var worksheet5 = excelPackage.Workbook.Worksheets.Add("Channel Goals");
                        cgGrid.PopulateWorksheet(worksheet5, 0, 0);

                        var worksheet6 = excelPackage.Workbook.Worksheets.Add("Spot listing");
                        var list = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.Insertations > 0 && _selectedChannels.Any(ch => ch.chid == mpTuple.MediaPlan.chid)).ToList();
                        await _factoryListing.PopulateWorksheet(list, worksheet6, 0, 0);

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

        private void btnSelectChannels_Click(object sender, RoutedEventArgs e)
        {
            // If all channels are selected
            if (lvChannels.SelectedItems.Count == lvChannels.Items.Count)
            {
                lvChannels.UnselectAll();
            }
            else
            {
                lvChannels.SelectAll();
            }
        }

        private void chbOnlyIns_Checked(object sender, RoutedEventArgs e)
        {
            if (dgMediaPlans.filterByIns == false)
            {
                dgMediaPlans.filterByIns = true;
                dgMediaPlans.FilterByInsChanged();
            }
        }

        private void chbOnlyIns_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgMediaPlans.filterByIns == true)
            {
                dgMediaPlans.filterByIns = false;
                dgMediaPlans.FilterByInsChanged();
            }
        }
    }
   
}
            
