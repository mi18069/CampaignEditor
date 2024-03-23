using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanHistDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CampaignEditor.UserControls
{
    public partial class CampaignForecast : Page
    {
        private SchemaController _schemaController;
        private ChannelController _channelController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private MediaPlanHistController _mediaPlanHistController;
        private MediaPlanRefController _mediaPlanRefController;
        private GoalsController _goalsController;
        private MediaPlanVersionController _mediaPlanVersionController;
        private ReachController _reachController;

        private DatabaseFunctionsController _databaseFunctionsController;

        private readonly IAbstractFactory<AddSchema> _factoryAddSchema;
        private readonly IAbstractFactory<AMRTrim> _factoryAmrTrim;
        private readonly IAbstractFactory<ImportFromSchema> _factoryImportFromSchema;
        private readonly PrintCampaignInfo _factoryPrintCmpInfo;
        private readonly Listing _factoryListing;
        private readonly PrintForecast _factoryPrintForecast;
        private readonly MediaPlanForecastData _forecastData;
        private ForecastDataManipulation _forecastDataManipulation;


        private bool canUserEdit = true;
        private bool isEditableVersion = true;

        private CampaignDTO _campaign;
        private int _cmpVersion = 1;
        public int CmpVersion { get { return _cmpVersion; } }
        private int _maxVersion = 1;
        private List<int> _versions = new List<int>();
        //private ObservableCollection<ChannelDTO> _channels = new ObservableCollection<ChannelDTO>();
        //private List<SpotDTO> _spots = new List<SpotDTO>();

        List<DayOfWeek> filteredDays = new List<DayOfWeek>
        { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
          DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday};

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        private ConcurrentBag<MediaPlanTuple> _concurrentAllMediaPlans = new ConcurrentBag<MediaPlanTuple>();

        private ObservableRangeCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableRangeCollection<MediaPlanTuple>();

        private ObservableRangeCollection<ChannelDTO> _allChannels = new ObservableRangeCollection<ChannelDTO>();
        private ObservableRangeCollection<ChannelDTO> _selectedChannels = new ObservableRangeCollection<ChannelDTO>();

        private ObservableCollection<MediaPlanHist> _showMPHist = new ObservableCollection<MediaPlanHist>();

        MediaPlanConverter _mpConverter;
        MediaPlanTermConverter _mpTermConverter;


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanRefRepository mediaPlanRefRepository,
            IGoalsRepository goalsRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IAbstractFactory<AddSchema> factoryAddSchema,
            IAbstractFactory<AMRTrim> factoryAmrTrim,
            IAbstractFactory<MediaPlanConverter> factoryMpConverter,
            IAbstractFactory<MediaPlanTermConverter> factoryMpTermConverter,
            IAbstractFactory<PrintCampaignInfo> factoryPrintCmpInfo,
            IAbstractFactory<Listing> factoryListing,
            IAbstractFactory<PrintForecast> factoryPrintForecast,
            IAbstractFactory<ImportFromSchema> factoryImportFromSchema,
            IAbstractFactory<MediaPlanForecastData> factoryForecastData,
            IReachRepository reachRepository,
            IAbstractFactory<ForecastDataManipulation> factoryForecastDataManipulation)
        {
            this.DataContext = this;

            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanHistController = new MediaPlanHistController(mediaPlanHistRepository);
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _goalsController = new GoalsController(goalsRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);
            _reachController = new ReachController(reachRepository);

            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);

            _factoryAddSchema = factoryAddSchema;
            _factoryAmrTrim = factoryAmrTrim;
            _factoryPrintCmpInfo = factoryPrintCmpInfo.Create();
            _factoryListing = factoryListing.Create();
            _factoryPrintForecast = factoryPrintForecast.Create();

            _mpConverter = factoryMpConverter.Create();
            _mpTermConverter = factoryMpTermConverter.Create();
            _forecastData = factoryForecastData.Create();

            _forecastDataManipulation = factoryForecastDataManipulation.Create();

            InitializeComponent();
            _factoryImportFromSchema = factoryImportFromSchema;

        }

        #region Triggers
        /*public async void OnAddChannelDelegated(object sender, EventArgs e)
        {
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            if (mpVersion != null)
            {
                await InitializeChannels();
                await LoadData(_maxVersion);
            }
        }*/

        public async Task GoalsChanged()
        {
            SetLoadingPage?.Invoke(this, null);
            await InitializeGoals();
            SetContentPage?.Invoke(this, null);

        }

        public async Task SpotsChanged()
        {
            SetLoadingPage?.Invoke(this, null);

            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            await _forecastData.Initialize(_campaign);
            _mpConverter.Initialize(_forecastData);
          
            await LoadData(mpVer.version);

            SetContentPage?.Invoke(this, null);
        }

        public async Task ChannelsChanged(List<int> channelsToDelete, List<int> channelsToAdd)
        {
            SetLoadingPage?.Invoke(this, null);

            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            await _forecastData.Initialize(_campaign);
            _mpConverter.Initialize(_forecastData);
            _forecastDataManipulation.Initialize(_campaign, _forecastData, _mpConverter);

            foreach (var chid in channelsToDelete)
            {
                SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("DELETING CHANNELS...", 1));
                await _forecastDataManipulation.DeleteChannelFromCampaign(_campaign.cmpid, chid);
            }
            foreach (var chid in channelsToAdd)
            {
                _forecastDataManipulation.UpdateProgressBar += _forecastDataManipulation_UpdateProgressBar;
                await _forecastDataManipulation.AddChannelInCampaign(_campaign.cmpid, chid, mpVer.version);
                _forecastDataManipulation.UpdateProgressBar -= _forecastDataManipulation_UpdateProgressBar;
            }

            SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("LOADING...", 1));

            await _forecastData.InitializeChannels();
            await LoadData(mpVer.version);

            SetContentPage?.Invoke(this, null);

        }


        #endregion

        #region Initialization
        public async Task Initialize(CampaignDTO campaign, bool isReadOnly)
        {
            _campaign = campaign;
            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            canUserEdit = !isReadOnly;
            //CheckManipulationButtons(canUserEdit, true);
            //CampaignEventLinker.AddForecast(_campaign.cmpid, this);


            await _forecastData.Initialize(_campaign);
            _mpConverter.Initialize(_forecastData);
            _forecastDataManipulation.Initialize(_campaign, _forecastData, _mpConverter);
            //await _forecastDataManipulation.DeleteCampaignForecast(_campaign.cmpid);
            //await _forecastDataManipulation.DeleteCampaignForecastVersion(campaign.cmpid, 2);

            await InitializeVersions();
            //InitializeChannels();
            //InitializeSpots();
            await InitializeGoals();


            SubscribeControllers();

            BindLists();

            FillVersions(_maxVersion);
            FillLvFilterDays();

        }

        private void CheckManipulationButtons(bool canUserEdit, bool isEditableVersion)
        {
            // btnNewVersion is always enabled
            if (!canUserEdit || !isEditableVersion)
            {
                btnNewVersion.IsEnabled = false;
                btnClear.IsEnabled = false;
                btnFetchData.IsEnabled = false;
                btnRecalculateData.IsEnabled = false;
                btnResetDates.IsEnabled = false;
            }
            else
            {
                btnNewVersion.IsEnabled = true;
                btnClear.IsEnabled = true;
                btnFetchData.IsEnabled = true;
                btnRecalculateData.IsEnabled = true;
                if (startDate >= DateTime.Now)
                    btnResetDates.IsEnabled = true;
            }
        }

        private async Task InitializeVersions()
        {

            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            if (mpVersion != null)
            {
                _maxVersion = mpVersion.version;
                _cmpVersion = _maxVersion;
            }

        }

        private async Task InitializeGoals()
        {
            goalsTreeView._goalsController = _goalsController;
            await goalsTreeView.Initialize(_campaign);
        }

        private void SubscribeControllers()
        {
            SubscribeDataGridControllers();
            SubscribeSWGGridControllers();
            SubscribeSDGGridControllers();
            SubscribePrintForecastControllers();
            SubscribeReachTabItemControllers();
        }

        private void SubscribeDataGridControllers()
        {
            dgMediaPlans._factoryAddSchema = _factoryAddSchema;
            dgMediaPlans._factoryAmrTrim = _factoryAmrTrim;
            dgMediaPlans._schemaController = _schemaController;
            dgMediaPlans._mediaPlanController = _mediaPlanController;
            dgMediaPlans._mediaPlanTermController = _mediaPlanTermController;
            dgMediaPlans._databaseFunctionsController = _databaseFunctionsController;
            dgMediaPlans._mpConverter = _mpConverter;
            dgMediaPlans._mpTermConverter = _mpTermConverter;
            dgMediaPlans.AddMediaPlanClicked += dgMediaPlans_AddMediaPlanClicked;
            dgMediaPlans.ImportMediaPlanClicked += dgMediaPlans_ImportMediaPlanClicked;
            dgMediaPlans.DeleteMediaPlanClicked += dgMediaPlans_DeleteMediaPlanClicked;
            // to open Update dialog
            dgMediaPlans.UpdateMediaPlanClicked += dgMediaPlans_UpdateMediaPlanClicked;
            // when mediaPlan is changed
            dgMediaPlans.UpdatedMediaPlan += dgMediaPlans_UpdatedMediaPlan;
            dgMediaPlans.RecalculateMediaPlan += dgMediaPlans_RecalculateMediaPlan;
            dgMediaPlans.ClearMpTerms += dgMediaPlans_ClearMpTerms;
            // When updating Terms
            dgMediaPlans.UpdatedTerm += dgMediaPlans_UpdatedTerm;
            dgMediaPlans.VisibleTuplesChanged += dgMediaPlans_VisibleTuplesChanged;
        }

        private void SubscribeSWGGridControllers()
        {
            swgGrid._mediaPlanController = _mediaPlanController;
            swgGrid._mediaPlanTermController = _mediaPlanTermController;
            swgGrid._allMediaPlans = _allMediaPlans;

            swgGrid.spotGoalsGrid = sgGrid;
        }

        private void SubscribeSDGGridControllers()
        {
            sdgGrid._mediaPlanController = _mediaPlanController;
            sdgGrid._mediaPlanTermController = _mediaPlanTermController;
            sdgGrid._allMediaPlans = _allMediaPlans;
        }

        private void SubscribePrintForecastControllers()
        {
            _factoryPrintForecast.lvChannels = lvChannels;
            _factoryPrintForecast.cgGrid = cgGrid;
            _factoryPrintForecast.mpGrid = dgMediaPlans;
            _factoryPrintForecast.sdgGrid = sdgGrid;
            _factoryPrintForecast.sgGrid = sgGrid;
            _factoryPrintForecast.swgGrid = swgGrid;
            _factoryPrintForecast.factoryListing = _factoryListing;
            _factoryPrintForecast.factoryPrintCmpInfo = _factoryPrintCmpInfo;
            _factoryPrintForecast._allMediaPlans = _allMediaPlans;
            _factoryPrintForecast._campaign = _campaign;

        }

        public void SubscribeReachTabItemControllers()
        {
            reachGrid._databaseFunctionsController = _databaseFunctionsController;
            reachGrid._reachController = _reachController;
            reachGrid.UpdateReach += ReachGrid_UpdateReach;
        }

        private void ReachGrid_UpdateReach(object? sender, UpdateReachEventArgs e)
        {
            var reach = e.Reach;
            goalsTreeView.UpdateTotalReach(reach);
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

        private void BindLists()
        {
            dgHist.ItemsSource = _showMPHist;
            lvChannels.ItemsSource = _allChannels;
        }

        private void FillVersions(int maxVersion)
        {
            cbVersions.Items.Clear();

            for (int i = 0; i < maxVersion; i++)
            {
                _versions.Add(i + 1);
                cbVersions.Items.Add(i + 1);
            }

            cbVersions.SelectedIndex = cbVersions.Items.Count - 1;
        }


        // Inserting new Data into database
        public async Task InsertAndLoadData(int version)
        {

            if (_maxVersion < version)
            {
                AddVersion(version);
            }

            SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("CREATING NEW MEDIA PLAN...", 1));
            _forecastDataManipulation.UpdateProgressBar += _forecastDataManipulation_UpdateProgressBar;
            await _forecastDataManipulation.InsertData(version);
            _forecastDataManipulation.UpdateProgressBar -= _forecastDataManipulation_UpdateProgressBar;

            await LoadData(version);

        }

        private void _forecastDataManipulation_UpdateProgressBar(object? sender, LoadingPageEventArgs e)
        {
            UpdateProgressBar?.Invoke(this, e);
        }

        public async Task MakeNewVersion(IEnumerable<MediaPlanTuple> allMediaPlans)
        {
            // Updating version in database
            int newVersion = -1;
            try
            {
                var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
                newVersion = mpVersion.version + 1;
                await _mediaPlanVersionController.UpdateMediaPlanVersion(_campaign.cmpid, newVersion);
            }
            catch
            {
                MessageBox.Show("Error with inserting new version!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Make new MediaPlan, MediaPlanTerms and MediaPlanHists in database
            try
            {
                await _forecastDataManipulation.InsertNewForecastMediaPlans(allMediaPlans, newVersion);
                AddVersion(newVersion);
                //await LoadData(newVersion);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to make new Media Plan!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await _forecastDataManipulation.DeleteCampaignForecastVersion(_campaign.cmpid, newVersion);
                await LoadData(newVersion - 1);
            }

        }      

        private void AddVersion(int version)
        {
            _maxVersion = version;
            _cmpVersion = version;
            cbVersions.Items.Add(version);
            cbVersions.SelectedIndex = version - 1;
        }

        public async Task LoadData(int version)
        {
            _cmpVersion = version;
            isEditableVersion = (_maxVersion == _cmpVersion);
            CheckManipulationButtons(canUserEdit, isEditableVersion);

            _allChannels.ReplaceRange(_forecastData.Channels);

            try
            {

                // Filling lvChannels and dictionary
                await FillMPList(version);
                await FillLoadedDateRanges();
                InitializeDataGrid();
 
                await InitializeGrids();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading data!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            await FillGoals();

        }

        public async Task InitializeGrids()
        {
            lvChannels.SelectedItems.Clear();

            InitializeCGGrid();
            swgGrid.Initialize(_campaign, _forecastData.Channels, _forecastData.Spots, _cmpVersion);
            sdgGrid.Initialize(_campaign, _forecastData.Channels, _forecastData.Spots, _cmpVersion);
            _factoryListing.Initialize(_campaign, _forecastData.Channels, 
                    _forecastData.ChidPricelistDict, _forecastData.SpotcodeSpotDict, _mpConverter);
            await reachGrid.Initialize(_campaign, _forecastData.Targets);
        }

        private void InitializeDataGrid()
        {
            dgMediaPlans.CanUserEdit = canUserEdit && isEditableVersion;
            dgMediaPlans._selectedChannels = _selectedChannels;
            dgMediaPlans._allMediaPlans = _allMediaPlans;
            dgMediaPlans._filteredDays = filteredDays;

            dgMediaPlans.Initialize(_campaign, _forecastData.Channels, _forecastData.Spots);

        }

        private void InitializeCGGrid()
        {

            ObservableCollection<MediaPlan> mediaPlans = new ObservableCollection<MediaPlan>(_allMediaPlans.Select(mp => mp.MediaPlan));
            cgGrid.Initialize(mediaPlans, _forecastData.Channels);
        }

        #region Drag and Drop selected Channels
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
                _forecastData.Channels.Insert(targetIdx + 1, droppedData);
                _forecastData.Channels.RemoveAt(removedIdx);
                _allChannels.ReplaceRange(_forecastData.Channels);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (_forecastData.Channels.Count + 1 > remIdx)
                {
                    _forecastData.Channels.Insert(targetIdx, droppedData);
                    _forecastData.Channels.RemoveAt(remIdx);
                    _allChannels.ReplaceRange(_forecastData.Channels);

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


        private async void CbVersions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if any items are selected
            int selectedIndex = cbVersions.SelectedIndex;
            int version = selectedIndex + 1; // Versions start from 1
            await LoadData(version);
        }

        private async void btnNewVersion_Click(object sender, RoutedEventArgs e)
        {
            int currentVersion = _cmpVersion;

            if (MessageBox.Show("Make new media plan from version: " + currentVersion + "?",
                "Question", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("MAKING NEW MEDIA PLAN DATA...", 0));
                await MakeNewVersion(_allMediaPlans);
                SetContentPage?.Invoke(this, null);
            }

        }

        public event EventHandler<LoadingPageEventArgs> SetLoadingPage;
        public event EventHandler<LoadingPageEventArgs> UpdateProgressBar;
        public event EventHandler<ChangeVersionEventArgs> SetContentPage;

        #region Helper buttons
        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all spots?", "Question", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("Clearing all spots", 0));
                await _forecastDataManipulation.ClearAllMPTerms(_allMediaPlans);
                lvChannels.SelectedItems.Clear();
                SetContentPage?.Invoke(this, null);

            }
        }

        private async void btnFetchData_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Load data from database?", "Question", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("Fetching data", 0));

                await LoadData(_maxVersion);

                SetContentPage?.Invoke(this, null);

            }
        }

        private async void btnRecalculateData_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Recalculate all data values?", "Question", MessageBoxButton.OKCancel,
     MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("Recalculating data", 0));


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
            List<Task> calculatingChannelMPTasks = _forecastData.Channels.Select(channel =>
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

        // reaching or creating mediaPlan
        private async Task<MediaPlanDTO> SchemaToMP(SchemaDTO schema, int version, bool shouldReplace = false)
        {
            MediaPlanDTO mediaPlan = null;
            // if already exist, fix conflicts
            if ((mediaPlan = await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid, version)) != null)
            {
                // if mediaPlan already exists, return that one
                if (AreEqualMediaPlanAndSchema(mediaPlan, schema) && !shouldReplace)
                    return mediaPlan;
                else
                {
                    if (!shouldReplace)
                    {
                        if (MessageBox.Show($"New program conflicts with existing:\n{mediaPlan.name}\n" +
                            $"This action will replace existing program with new one", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                            == MessageBoxResult.Cancel)
                        {
                            return mediaPlan;
                        }
                    }

                    await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlan.xmpid);
                    await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(mediaPlan.xmpid);
                    await _mediaPlanController.DeleteMediaPlanById(mediaPlan.xmpid);

                    var itemToRemove = _allMediaPlans.Where(item => item.MediaPlan.schid == schema.id).First();
                    _allMediaPlans.Remove(itemToRemove);

                }

            }
            if (schema.blocktime == null || schema.blocktime.Length == 0)
            {
                string position = schema.position.Trim();
                if (position == "INS" || position == "BET")
                {
                    schema.blocktime = _schemaController.CalculateBlocktime(position, schema.stime, schema.etime);
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
                   //plan1.blocktime == schema.blocktime && // becaus we sometimes change blocktime when inserting mediaPlan 
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

        private async Task FillMPList(int version)
        {
            var allMediaPlans = await _forecastDataManipulation.MakeMediaPlanTuples(version);
            _allMediaPlans.ReplaceRange(allMediaPlans);

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
            cgGrid.SelectedChannelsChanged(_selectedChannels);

        }

        #region ContextMenu

        private void lvChannels_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ListViewItem clickedItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            var channels = lvChannels.SelectedItems.Cast<ChannelDTO>().ToList();
            //clickedItem != null
            if (channels.Count > 0)
            {
                // Do something with the clicked item
                //var channel = clickedItem.DataContext as ChannelDTO;
                // Call your function for the item
                ContextMenu menu = new ContextMenu();

                MenuItem trimAmrs = new MenuItem();
                trimAmrs.Header = "Trim Channel Amrs";
                trimAmrs.Click += async (obj, ea) =>
                {
                    //var chname = channel.chname;

                    var f = _factoryAmrTrim.Create();
                    if (channels.Count == 1)
                        f.Initialize("Trim Channel " + channels[0].chname.Trim(), 100);
                    else
                        f.Initialize("Trim Selected Channels ", 100);

                    f.ShowDialog();
                    if (f.changed)
                    {
                        foreach (var channel in channels)
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
                        

                    }
                };
                menu.Items.Add(trimAmrs);

                MenuItem recalculateChannel = new MenuItem();
                recalculateChannel.Header = "Recalculate channel values";
                recalculateChannel.Click += async (obj, ea) =>
                {
                    if (MessageBox.Show($"Recalculate channel values for selected channels?",
                        "Question:", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("Recalculating data", 0));
                        // When pricelists or calculating inside MediaPlans are changed 
                        foreach (var channel in channels)
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
            goalsTreeView.FillGoals(_allMediaPlans);
            var reach = await _reachController.GetFinalReachByCmpid(_campaign.cmpid);
            if (reach != null)
            {
                goalsTreeView.UpdateTotalReach(reach);
            }
        }
        #endregion

        #region dgMediaPlans

        private async void dgMediaPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _showMPHist.Clear();
            if (dgMediaPlans.Schema.SelectedItems.Count == 0)
            {
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

                goalsTreeView.SelectedTupleChanged(mediaPlanTuple);
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
                // For keeping information about already assigned spots
                List<Tuple<DateOnly, string>> assignedSpots = new List<Tuple<DateOnly, string>>();
                var assignedTermList = mediaPlanTerms.Where(term => term != null && 
                                              term.Spotcode != null && term.Spotcode.Trim().Length > 0);
                foreach (MediaPlanTerm assigned in assignedTermList)
                {
                    assignedSpots.Add(Tuple.Create(assigned.Date, assigned.Spotcode.Trim()));
                }

                await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlanDTO.xmpid);
                var newMediaPlanTerms = await MediaPlanToMPTerm(mediaPlanDTO);
                foreach (var tuple in assignedSpots)
                {
                    var term = newMediaPlanTerms.Where(term => term != null && term.Date == tuple.Item1).First();
                    term.Spotcode = tuple.Item2;
                    var termDTO = _mpTermConverter.ConvertToDTO(term);
                    await _mediaPlanTermController.UpdateMediaPlanTerm(new UpdateMediaPlanTermDTO(termDTO));
                }
                mediaPlanTerms = newMediaPlanTerms;
            }

            if (updateSchema)
            {
                var schema = await _schemaController.GetSchemaById(mediaPlan.schid);
                schema.stime = mediaPlan.stime;
                schema.etime = mediaPlan.etime;
                schema.blocktime = mediaPlan.blocktime;
                await _schemaController.UpdateSchema(new UpdateSchemaDTO(schema));
            }

            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlanDTO));
            await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);

            var newMediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(newMediaPlan)));
            var mpTupleNew = new MediaPlanTuple(newMediaPlan, mediaPlanTerms);

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

        private async void dgMediaPlans_ImportMediaPlanClicked(object? sender, EventArgs e)
        {
            ChannelDTO? selectedChannel = null;
            if (lvChannels.SelectedItems.Count == 1)
            {
                selectedChannel = lvChannels.SelectedItems[0] as ChannelDTO;
            }
            var factory = _factoryImportFromSchema.Create();
            factory.Initialize(_forecastData.Channels, selectedChannel);
            factory.ShowDialog();
            if (factory.success)
            {
                bool shouldReplace = factory.shouldReplace;
                var schemasToImport = factory.SchemasToImport;
                foreach (var schemaToImport in schemasToImport)
                {
                    bool duplicated = _allMediaPlans.Any(mpTuple => mpTuple.MediaPlan.schid == schemaToImport.id);
                    if (duplicated && !shouldReplace)
                    {
                        continue;
                    }
                    /*if (duplicated && shouldReplace)
                    {
                        var tupleToDelete = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.schid == schemaToImport.id).First();
                        _allMediaPlans.Remove(tupleToDelete);
                    }*/

                    var mediaPlanDTO = await SchemaToMP(schemaToImport, _cmpVersion, shouldReplace);
                    var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
                    var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlanDTO);
                    var mediaPlanTuple = new MediaPlanTuple(mediaPlan, mediaPlanTerms);
                    _allMediaPlans.Add(mediaPlanTuple);
                }
            }
        }

        private void DeleteTermValues(MediaPlanTuple mediaPlanTuple)
        {
            var mediaPlan = mediaPlanTuple.MediaPlan;
            var terms = mediaPlanTuple.Terms;
            foreach (var term in terms.Where(term => term != null && term.Spotcode != null 
            && term.Spotcode.Trim().Count() > 0))
            {
                foreach (char spotcode in term.Spotcode.Trim())
                    UpdatedTerm(mediaPlan, term, spotcode);
            }
        }

        private async void dgMediaPlans_DeleteMediaPlanClicked(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            if (mediaPlanTuple != null)
            {
                if (MessageBox.Show($"Delete program\n{mediaPlanTuple.MediaPlan.Name.Trim()}?", "Question",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    bool canBeDeleted = false;

                    var firstTerm = mediaPlanTuple.Terms.FirstOrDefault(term => term != null &&
                                                                term.Spotcode != null && term.Spotcode.Trim().Length > 0);
                    // If program has spots, check if they can be deleted, if user clear them, then delete program
                    if (firstTerm != null)
                    {
                        if (firstTerm.Date <= DateOnly.FromDateTime(DateTime.Today))
                        {
                            MessageBox.Show($"Cannot delete program because it has assigned spots", "Question",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        else
                        {
                            if (MessageBox.Show($"Program has assigned spots\nThis action will delete it's spots", "Question",
                                MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                            {
                                await _forecastDataManipulation.ClearMPTerms(mediaPlanTuple);
                                canBeDeleted = true;
                            }
                        }
                    }
                    else
                    {
                        canBeDeleted = true;
                    }

                    if (canBeDeleted)
                    {
                        DeleteTermValues(mediaPlanTuple);
                        _allMediaPlans.Remove(mediaPlanTuple);
                        var mediaPlan = mediaPlanTuple.MediaPlan;
                        await DeleteMPById(mediaPlan.xmpid);
                    }
                    
                }
            }
        }

        private async void dgMediaPlans_RecalculateMediaPlan(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            if (mediaPlanTuple != null)
            {
                if (MessageBox.Show($"Recalculate values for program\n{mediaPlanTuple.MediaPlan.Name.Trim()}?", "Question", 
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

        private async void dgMediaPlans_ClearMpTerms(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            if (mediaPlanTuple != null)
            {
                if (MessageBox.Show($"Clear spots for program\n{mediaPlanTuple.MediaPlan.Name.Trim()}?", "Question",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    await _forecastDataManipulation.ClearMPTerms(mediaPlanTuple);
                }

            }
        }

        private void dgMediaPlans_UpdatedTerm(object? sender, UpdatedTermEventArgs e)
        {
            MediaPlanTerm term = e.Term;
            char? spotcode = e.Spotcode;
            if (!spotcode.HasValue)
                return;
            var mediaPlan = _allMediaPlans.First(mpt => mpt.Terms.Any(t => t != null && t.Xmptermid == term.Xmptermid)).MediaPlan;
            UpdatedTerm(mediaPlan, term, spotcode.Value);
        }
        private void dgMediaPlans_VisibleTuplesChanged(object? sender, EventArgs e)
        {
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(dgMediaPlans.dgMediaPlans.ItemsSource);

            IEnumerable<MediaPlanTuple> visibleTuples = collectionView.OfType<MediaPlanTuple>();
            swgGrid.VisibleTuplesChanged(visibleTuples);
            sdgGrid.VisibleTuplesChanged(visibleTuples);
            cgGrid.VisibleTuplesChanged(visibleTuples);
        }


        private void UpdatedTerm(MediaPlan mediaPlan, MediaPlanTerm term, char spotcode)
        {

            var channel = _forecastData.Channels.First(ch => ch.chid == mediaPlan.chid);
            var date = term.Date;
            var spot = _forecastData.SpotcodeSpotDict[spotcode];

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

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            _factoryPrintForecast.ShowDialog();
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

        public void ClosePage()
        {          
            _factoryPrintForecast.shouldClose = true;
            _factoryPrintForecast.Close();
            UnsubscribeEvents();
        }

        private void UnsubscribeEvents()
        {
            UnsibscribeDataGridEvents();
            reachGrid.UpdateReach -= ReachGrid_UpdateReach;

        }

        private void UnsibscribeDataGridEvents()
        {
            dgMediaPlans.AddMediaPlanClicked -= dgMediaPlans_AddMediaPlanClicked;
            dgMediaPlans.ImportMediaPlanClicked -= dgMediaPlans_ImportMediaPlanClicked;
            dgMediaPlans.DeleteMediaPlanClicked -= dgMediaPlans_DeleteMediaPlanClicked;
            dgMediaPlans.UpdateMediaPlanClicked -= dgMediaPlans_UpdateMediaPlanClicked;
            dgMediaPlans.UpdatedMediaPlan -= dgMediaPlans_UpdatedMediaPlan;
            dgMediaPlans.RecalculateMediaPlan -= dgMediaPlans_RecalculateMediaPlan;
            dgMediaPlans.ClearMpTerms -= dgMediaPlans_ClearMpTerms;
            dgMediaPlans.UpdatedTerm -= dgMediaPlans_UpdatedTerm;
            dgMediaPlans.VisibleTuplesChanged -= dgMediaPlans_VisibleTuplesChanged;
        }


    }
   
}
            
