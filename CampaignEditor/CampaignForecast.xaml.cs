﻿using CampaignEditor.Controllers;
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
using Database.DTOs.DayPartDTO;
using Database.DTOs.PricelistDTO;
using System.Drawing;
using Database.DTOs.CobrandDTO;
using Database.DTOs.SpotDTO;
using CampaignEditor.UserControls.ForecastGrids;

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
        private ClientCoefsController _clientCoefsController;
        private DGConfigController _dgConfigController;

        private DatabaseFunctionsController _databaseFunctionsController;

        private readonly IAbstractFactory<AddSchema> _factoryAddSchema;
        private readonly IAbstractFactory<AMRTrim> _factoryAmrTrim;
        private readonly IAbstractFactory<ImportFromSchema> _factoryImportFromSchema;
        private readonly PrintCampaignInfo _factoryPrintCmpInfo;
        private readonly Listing _factoryListing;
        private readonly PrintForecast _factoryPrintForecast;
        private ForecastDataManipulation _forecastDataManipulation;

        private bool canUserEdit = true;
        private bool isEditableVersion = true;

        private CampaignDTO _campaign;
        private int _cmpVersion = 1;
        private int _maxVersion = 1;

        List<DayOfWeek> filteredDays;
        List<DayPartDTO> filteredDayParts = new List<DayPartDTO>();

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;


        /*private ObservableRangeCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableRangeCollection<MediaPlanTuple>();*/
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;

        private ObservableRangeCollection<ChannelDTO> _allChannels = new ObservableRangeCollection<ChannelDTO>();
        private ObservableRangeCollection<ChannelDTO> _selectedChannels = new ObservableRangeCollection<ChannelDTO>();
        private ObservableRangeCollection<ChannelDTO> _gridDisplayChannels = new ObservableRangeCollection<ChannelDTO>();

        private ObservableCollection<MediaPlanHist> _showMPHist = new ObservableCollection<MediaPlanHist>();

        // For delegating changes to forecast when realizations are changed
        // selecting different chids and date 
        private HashSet<Tuple<DateOnly, int, SpotDTO>> _updatedRealizations = new HashSet<Tuple<DateOnly, int, SpotDTO>>();

        public MediaPlanForecastData _forecastData;
        public MediaPlanConverter _mpConverter;

        MediaPlanTermConverter _mpTermConverter;
        private bool gridShowSelected = true;
        private bool gridFirstOpening = true;

        private GridsDataManipulation gridsDataManipulation = new GridsDataManipulation();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<UpdatedTermDateAndChannelEventArgs> UpdatedTermDateAndChannel;
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
            //IAbstractFactory<MediaPlanConverter> factoryMpConverter,
            IAbstractFactory<MediaPlanTermConverter> factoryMpTermConverter,
            IAbstractFactory<PrintCampaignInfo> factoryPrintCmpInfo,
            IAbstractFactory<Listing> factoryListing,
            IAbstractFactory<PrintForecast> factoryPrintForecast,
            IAbstractFactory<ImportFromSchema> factoryImportFromSchema,
            //IAbstractFactory<MediaPlanForecastData> factoryForecastData,
            IReachRepository reachRepository,
            IAbstractFactory<ForecastDataManipulation> factoryForecastDataManipulation,
            IClientCoefsRepository clientCoefsRepository,
            IDGConfigRepository dgConfigRepository)
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
            _clientCoefsController = new ClientCoefsController(clientCoefsRepository);
            _dgConfigController = new DGConfigController(dgConfigRepository);
            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);

            _factoryAddSchema = factoryAddSchema;
            _factoryAmrTrim = factoryAmrTrim;
            _factoryPrintCmpInfo = factoryPrintCmpInfo.Create();
            _factoryListing = factoryListing.Create();
            _factoryPrintForecast = factoryPrintForecast.Create();

            //_mpConverter = factoryMpConverter.Create();
            _mpTermConverter = factoryMpTermConverter.Create();
            //_forecastData = factoryForecastData.Create();

            _forecastDataManipulation = factoryForecastDataManipulation.Create();

            InitializeComponent();
            _factoryImportFromSchema = factoryImportFromSchema;

        }

        #region Triggers

        public async Task CampaignChanged(CampaignDTO campaign)
        {
            SetLoadingPage?.Invoke(this, null);

            try
            {
                _forecastDataManipulation.Initialize(campaign, _forecastData, _mpConverter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating campaign\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _campaign = campaign;

            SetContentPage?.Invoke(this, null);
        }
        public async Task GoalsChanged()
        {
            SetLoadingPage?.Invoke(this, null);
            try
            {
                await goalsTreeView.GoalsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating goals\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            SetContentPage?.Invoke(this, null);

        }

        public async Task TargetsChanged()
        {
            SetLoadingPage?.Invoke(this, null);

            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            //await _forecastData.InitializeTargets();
            goalsTreeView._mpConverter = _mpConverter;
            _forecastDataManipulation.UpdateProgressBar += _forecastDataManipulation_UpdateProgressBar;
            try
            {
                await _forecastDataManipulation.RecalculateMediaPlans(mpVer.version);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while recalculating media plan values\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _forecastDataManipulation.UpdateProgressBar -= _forecastDataManipulation_UpdateProgressBar;
            await LoadData(mpVer.version);

            SetContentPage?.Invoke(this, null);
        }

        public async Task SpotsChanged()
        {
            SetLoadingPage?.Invoke(this, null);

            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);
            //await _forecastData.InitializeSpots();
            //_mpConverter.Initialize(_forecastData);
            goalsTreeView._mpConverter = _mpConverter;
            try
            {
                await CalculateMPValuesForCampaign(_campaign.cmpid, _maxVersion);
                await LoadData(mpVer.version);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating spots\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
  

            SetContentPage?.Invoke(this, null);
        }

        public async Task DayPartsChanged()
        {
            SetLoadingPage?.Invoke(this, null);

            //await _forecastData.InitializeDayParts();
            //_mpConverter.Initialize(_forecastData);
            try
            {
                FillLvFilterDayParts();
                foreach (var mediaPlan in _allMediaPlans.Select(mpt => mpt.MediaPlan))
                {
                    _mpConverter.SetDayPart(mediaPlan);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating day parts\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }


            lvChannels.UnselectAll();

            SetContentPage?.Invoke(this, null);
        }

        public async Task ChannelsChanged(List<int> channelsToDelete, List<int> channelsToAdd)
        {
            SetLoadingPage?.Invoke(this, null);

            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);

            //await _forecastData.InitializeChannels(true);
            //_mpConverter.Initialize(_forecastData);
            _forecastDataManipulation.Initialize(_campaign, _forecastData, _mpConverter);

            try
            {
                foreach (var chid in channelsToDelete)
                {
                    SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("DELETING CHANNELS...", 0));
                    await _forecastDataManipulation.DeleteChannelFromCampaign(_campaign.cmpid, chid);
                }
                foreach (var chid in channelsToAdd)
                {
                    _forecastDataManipulation.UpdateProgressBar += _forecastDataManipulation_UpdateProgressBar;
                    await _forecastDataManipulation.AddChannelInCampaign(_campaign.cmpid, chid, mpVer.version);
                    _forecastDataManipulation.UpdateProgressBar -= _forecastDataManipulation_UpdateProgressBar;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating channels\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            

            SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("LOADING...", 0));

            await LoadData(mpVer.version);

            SetContentPage?.Invoke(this, null);

        }

        public void ChannelsOrderChanged()
        {
            SetLoadingPage?.Invoke(this, null);

            _allChannels.ReplaceRange(_forecastData.Channels);

            ChangeChannelsOrderInGrids();

            SetContentPage?.Invoke(this, null);

        }

        public async Task PricelistChanged(PricelistDTO pricelist)
        {
            SetLoadingPage?.Invoke(this, null);

            //await _forecastData.InitializePricelists();
            //_mpConverter.Initialize(_forecastData);
            try
            {
                _forecastDataManipulation.Initialize(_campaign, _forecastData, _mpConverter);

                var channelIds = new List<int>();
                foreach (var keyValue in _forecastData.ChidPricelistDict)
                {
                    if (keyValue.Value.plid == pricelist.plid)
                    {
                        channelIds.Add(keyValue.Key);
                    }
                }
                foreach (var channelId in channelIds)
                {
                    /*var mpTuples = _allMediaPlans.Where(mpt => mpt.MediaPlan.chid == channelId);
                    await _forecastDataManipulation.RecalculatePricelistMediaPlans(mpTuples);*/
                    await CalculateMPValuesForChannel(_campaign.cmpid, channelId, _maxVersion);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating program values\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            SetLoadingPage?.Invoke(this, new LoadingPageEventArgs("LOADING...", 0));

            await LoadData(_maxVersion);

            SetContentPage?.Invoke(this, null);

        }

        public async Task CobrandsChanged(IEnumerable<CobrandDTO> cobrands)
        {
            SetLoadingPage?.Invoke(this, null);

            var mpVer = await _mediaPlanVersionController.GetLatestMediaPlanVersion(_campaign.cmpid);

            goalsTreeView._mpConverter = _mpConverter;
            try
            {
                var chids = cobrands.Select(cb => cb.chid).Distinct().ToList();
                foreach (var chid in chids)
                    await CalculateMPValuesForChannel(_campaign.cmpid, chid, mpVer.version);
                await LoadData(mpVer.version);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating spots\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }


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

            /*await _forecastData.Initialize(_campaign);
            _mpConverter.Initialize(_forecastData);*/
            _forecastDataManipulation.Initialize(_campaign, _forecastData, _mpConverter);
            await InitializeVersions();
            await InitializeGoals();
            SubscribeControllers();

            BindLists();

            InitializeListViews();

        }

        private void InitializeListViews()
        {
            FillVersions(_maxVersion);
            FillLvFilterDays();
            FillLvFilterDayParts();
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
            goalsTreeView._mpConverter = _mpConverter;
            await goalsTreeView.Initialize(_campaign);
        }

        private void SubscribeControllers()
        {
            SubscribeDataGridControllers();
            SubscribeGridManipulationControllers();
            //SubscribeSWGGridControllers();
            //SubscribeSDGGridControllers();
            SubscribePrintForecastControllers();
            SubscribeReachTabItemControllers();
        }

        private void SubscribeGridManipulationControllers()
        {
            gridsDataManipulation._allMediaPlans = _allMediaPlans;
            gridsDataManipulation.cgGrid = cgGrid;
            gridsDataManipulation.swgGrid = swgGrid;
            gridsDataManipulation.sdgGrid = sdgGrid;
            gridsDataManipulation.sgGrid = sgGrid;
            gridsDataManipulation.swgGrid.spotGoalsGrid = sgGrid;

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
            dgMediaPlans._clientCoefsController = _clientCoefsController;
            dgMediaPlans._dgConfigController = _dgConfigController;
            dgMediaPlans.AddMediaPlanClicked += dgMediaPlans_AddMediaPlanClicked;
            dgMediaPlans.ImportMediaPlanClicked += dgMediaPlans_ImportMediaPlanClicked;
            dgMediaPlans.CopyNameClicked += dgMediaPlans_CopyNameClicked;
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
            swgGrid._allMediaPlans = _allMediaPlans;

            swgGrid.spotGoalsGrid = sgGrid;
        }

        private void SubscribeSDGGridControllers()
        {
            sdgGrid._allMediaPlans = _allMediaPlans;
        }

        private void SubscribePrintForecastControllers()
        {
            _factoryPrintForecast._selectedChannels = _gridDisplayChannels;
            _factoryPrintForecast.cgGrid = cgGrid;
            _factoryPrintForecast.mpGrid = dgMediaPlans;
            _factoryPrintForecast.sdgGrid = sdgGrid;
            _factoryPrintForecast.sgGrid = sgGrid;
            _factoryPrintForecast.swgGrid = swgGrid;
            _factoryPrintForecast.factoryListing = _factoryListing;
            _factoryPrintForecast.factoryPrintCmpInfo = _factoryPrintCmpInfo;
            _factoryPrintForecast._allMediaPlans = _allMediaPlans;
            _factoryPrintForecast._campaign = _campaign;
            _factoryPrintForecast.SetDates(_campaign);
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
            filteredDays = new List<DayOfWeek>
            { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday};

            foreach (var day in filteredDays)
            {
                lvFilterDays.Items.Add(day);
            }
        }

        private void FillLvFilterDayParts()
        {
            var filterDayParts = _forecastData.DayPartsDict.Select(kv => kv.Key).ToList();
            lvFilterDayParts.ItemsSource = filterDayParts;
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

            dgMediaPlans.filterByDays = !(filteredDays.Count() == 0 || filteredDays.Count() == lvFilterDays.Items.Count);
            dgMediaPlans._filteredDays = filteredDays;

            var selectedChannels = lvChannels.SelectedItems.Cast<ChannelDTO>();
            _selectedChannels.ReplaceRange(selectedChannels);
            ChangeGridDisplayData(_selectedChannels);

        }

        private void lvFilterDayParts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filteredDayParts.Clear();
            if (lvFilterDayParts.SelectedItems.Count == 0)
            {
                foreach (DayPartDTO dayPart in lvFilterDayParts.Items)
                    filteredDayParts.Add(dayPart);
            }
            else
            {
                foreach (DayPartDTO day in lvFilterDayParts.SelectedItems)
                    filteredDayParts.Add(day);
            }

            dgMediaPlans.filterByDP = !(filteredDayParts.Count == 0 ||
                                      filteredDayParts.Count() == _forecastData.DayPartsDict.Count());

            dgMediaPlans._filteredDayParts = filteredDayParts;

            var selectedChannels = lvChannels.SelectedItems.Cast<ChannelDTO>();
            _selectedChannels.ReplaceRange(selectedChannels);
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

            await _forecastData.Initialize(_campaign);
            _mpConverter.Initialize(_forecastData);

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

            // Make new MediaPlan and MediaPlanTerms in database
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

                await InitializeGrids();

                // Filling lvChannels and dictionary
                await FillMPList(version);
                await FillLoadedDateRanges();
                await InitializeDataGrid();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading data!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            await FillGoals();

        }

        private void CheckManipulationButtons(bool canUserEdit, bool isEditableVersion)
        {
            // btnNewVersion is always enabled
            if (!canUserEdit || !isEditableVersion)
            {
                //btnNewVersion.IsEnabled = false;
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

        public async Task InitializeGrids()
        {
            lvChannels.SelectedItems.Clear();

            InitializeCGGrid();
            /*swgGrid.Initialize(_campaign, _forecastData.Channels, _forecastData.Spots, _cmpVersion);
            swgGrid._forecastData = _forecastData;
            sdgGrid.Initialize(_campaign, _forecastData.Channels, _forecastData.Spots, _cmpVersion);
            sdgGrid._forecastData = _forecastData;*/
            gridsDataManipulation.Initialize(_campaign, _forecastData, _cmpVersion);

            _factoryListing.Initialize(_campaign, _forecastData.Channels,
                _forecastData.SpotcodeSpotDict, _mpConverter);
            // should be awaitable, but it takes too long now
            reachGrid.Initialize(_campaign, _forecastData.Targets);
        }

        private async Task InitializeDataGrid()
        {
            dgMediaPlans.CanUserEdit = canUserEdit && isEditableVersion;
            dgMediaPlans._selectedChannels = _selectedChannels;
            dgMediaPlans._allMediaPlans = _allMediaPlans;
            dgMediaPlans._filteredDays = filteredDays;
            await dgMediaPlans.Initialize(_campaign, _forecastData.Channels, _forecastData.Spots);

        }

        private void InitializeCGGrid()
        {

            /*ObservableCollection<MediaPlan> mediaPlans = new ObservableCollection<MediaPlan>(_allMediaPlans.Select(mp => mp.MediaPlan));
            cgGrid.Initialize(mediaPlans, _forecastData.Channels);
            cgGrid._forecastData = _forecastData;
            cgGrid.startDate = DateOnly.FromDateTime(startDate);*/
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

            ChangeChannelsOrderInGrids();

        }

        private void ChangeChannelsOrderInGrids()
        {
            lvChannels.SelectedItems.Clear();
            lvChannels.ItemsSource = null;
            lvChannels.ItemsSource = _allChannels;
            dgMediaPlans.ChannelsOrderChanged(_allChannels.ToList());

            /*sgGrid.UpdateUgChannelOrder(_forecastData.Channels, true);
            swgGrid.UpdateUgChannelOrder(_forecastData.Channels, true);
            sdgGrid.UpdateUgChannelOrder(_forecastData.Channels, true);*/
            gridsDataManipulation.UpdateChannelOrder(_forecastData.Channels);
            //cgGrid.UpdateOrder(_forecastData.Channels);
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


        public async void CbVersions_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private List<ChannelDTO> GetSelectedChannelsInOrder()
        {
            List<ChannelDTO> channels = new List<ChannelDTO>();

            /*for (int i = 0; i < lvChannels.Items.Count; i++)
            {
                ListViewItem item = lvChannels.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;

                if (item != null && item.IsSelected)
                {
                    channels.Add(lvChannels.Items[i] as ChannelDTO);
                }
            }*/

            foreach (ChannelDTO channel in lvChannels.SelectedItems)
            {
                channels.Add(channel);
            }
            return channels;
        }
        private void lvChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedChannels.ReplaceRange(GetSelectedChannelsInOrder());

            ChangeGridDisplayData(_selectedChannels);          
        }

        private void ChangeGridDisplayData(IEnumerable<ChannelDTO> displayChannels)
        {
            if (gridFirstOpening == true)
            {
                gridFirstOpening = false;
                return;
            }
            if (gridShowSelected == true)
            {
                _gridDisplayChannels.ReplaceRange(displayChannels);
                gridsDataManipulation.SelectedChannelsChanged(displayChannels);
                /*sdgGrid.SelectedChannelsChanged(displayChannels);
                swgGrid.SelectedChannelsChanged(displayChannels);*/
               // cgGrid.SelectedChannelsChanged(displayChannels);
            }
            
           

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
                            var mediaPlanTuples = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.chid == channel.chid);
                            foreach (MediaPlanTuple mediaPlanTuple in mediaPlanTuples)
                            {
                                if (f.attributesToTrim[0])
                                    mediaPlanTuple.MediaPlan.Amr1trim = f.newValue;
                                if (f.attributesToTrim[1])
                                    mediaPlanTuple.MediaPlan.Amr2trim = f.newValue;
                                if (f.attributesToTrim[2])
                                    mediaPlanTuple.MediaPlan.Amr3trim = f.newValue;
                                if (f.attributesToTrim[3])
                                    mediaPlanTuple.MediaPlan.Amrsaletrim = f.newValue;

                                var termsDTO = _mpTermConverter.ConvertToEnumerableDTO(mediaPlanTuple.Terms);
                                _mpConverter.ComputeExtraProperties(mediaPlanTuple.MediaPlan, termsDTO, true);
                                var mpDTO = _mpConverter.ConvertToDTO(mediaPlanTuple.MediaPlan);
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
                int xmpid = mediaPlan.xmpid;

                if (_cmpVersion > 1)
                {
                    var oldestVersionMediaPlan = await _mediaPlanController.GetOldestVersionMediaPlanBySchemaAndCmpId(mediaPlan.schid, _campaign.cmpid);
                    if (oldestVersionMediaPlan == null)
                        return;

                    xmpid = oldestVersionMediaPlan.xmpid;
                }
                var mediaPlanHists = await _mediaPlanHistController.GetAllMediaPlanHistsByXmpid(xmpid);
                mediaPlanHists = mediaPlanHists.OrderBy(m => m.date).ThenBy(m => m.stime);
                foreach (var mediaPlanHist in mediaPlanHists)
                    _showMPHist.Add(mediaPlanHist);

                goalsTreeView.SelectedTupleChanged(mediaPlanTuple);
            }
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
                        MediaPlanDTO mediaPlanDTO = await _forecastDataManipulation.SchemaToMP(schema, _cmpVersion);

                        if (_allMediaPlans.Any(mp => mp.MediaPlan.xmpid == mediaPlanDTO.xmpid))
                        {
                            return;
                        }
                        await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);
                        var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
                        var mediaPlanTerms = await _forecastDataManipulation.MediaPlanToMPTerm(mediaPlanDTO);
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
            
            await UpdatedMediaPlan(mediaPlan, false);
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
                var newMediaPlanTerms = await _forecastDataManipulation.MediaPlanToMPTerm(mediaPlanDTO);
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
                schema.days = mediaPlan.days;
                schema.position = mediaPlan.position;
                schema.name = mediaPlan.name.Trim();
                schema.type = mediaPlan.type;

                try
                {
                    await _schemaController.UpdateSchema(new UpdateSchemaDTO(schema));
                }
                catch
                {

                }
            }

            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlanDTO));
            await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);

            var newMediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(newMediaPlan)));
            var mpTupleNew = new MediaPlanTuple(newMediaPlan, mediaPlanTerms);

            var isRemoved = _allMediaPlans.Remove(mediaPlanTuple);
            if (isRemoved)
                _allMediaPlans.Add(mpTupleNew);
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

                    var mediaPlanDTO = await _forecastDataManipulation.SchemaToMP(schemaToImport, _cmpVersion, shouldReplace);
                    var mediaPlan = await _mpConverter.ConvertFirstFromDTO(mediaPlanDTO);
                    var mediaPlanTerms = await _forecastDataManipulation.MediaPlanToMPTerm(mediaPlanDTO);
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

        private void dgMediaPlans_CopyNameClicked(object? sender, EventArgs e)
        {
            var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
            // String value to copy to the clipboard
            string textToCopy = mediaPlanTuple.MediaPlan.Name.Trim();

            // Copy the string value to the clipboard
            Clipboard.SetText(textToCopy);
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
                        await _forecastDataManipulation.DeleteMediaPlan(mediaPlan.xmpid, mediaPlan.schid);
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

            if (gridShowSelected == true)
            {
                IEnumerable<MediaPlanTuple> visibleTuples = collectionView.OfType<MediaPlanTuple>();
                /*swgGrid.VisibleTuplesChanged(visibleTuples);
                sdgGrid.VisibleTuplesChanged(visibleTuples);*/
                gridsDataManipulation.VisibleTuplesChanged(visibleTuples);
                //cgGrid.VisibleTuplesChanged(visibleTuples);
            }

        }


        private void UpdatedTerm(MediaPlan mediaPlan, MediaPlanTerm term, char spotcode)
        {

            var channel = _forecastData.Channels.First(ch => ch.chid == mediaPlan.chid);
            var date = term.Date;
            var spot = _forecastData.SpotcodeSpotDict[spotcode];

            /*sdgGrid.RecalculateGoals(channel, date, spot, true);
            swgGrid.RecalculateGoals(channel, date, spot, true);*/
            gridsDataManipulation.RecalculateGoals(channel, date, spot, true);
            //cgGrid.RecalculateGoalsExpected(channel.chid);

            // For propagating changes in validation
            UpdatedTermDateAndChannel?.Invoke(this, new UpdatedTermDateAndChannelEventArgs(date, channel));
        }

        #endregion

        #region Page

        // to prevent event from closing this page on backspace 
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
                    var termsDTO = _mpTermConverter.ConvertToEnumerableDTO(mediaPlanTuple.Terms);
                    _mpConverter.ComputeExtraProperties(mediaPlan, termsDTO, true);
                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
                }
            }
        }


        #endregion

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (gridShowSelected == true)
                _factoryPrintForecast.visibleTuples = dgMediaPlans.GetVisibleMediaPlanTuples();
            else
                _factoryPrintForecast.visibleTuples = _allMediaPlans.ToList();

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
            UnsibscribeReachGridEvents();

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

        private void UnsibscribeReachGridEvents()
        {
            reachGrid.UpdateReach -= ReachGrid_UpdateReach;
        }

        private void rbGridShowSelected_Checked(object sender, RoutedEventArgs e)
        {
            gridShowSelected = true;
            _gridDisplayChannels.ReplaceRange(_selectedChannels);
            ChangeGridDisplayData(_selectedChannels);
        }

        private void rbGridShowAll_Checked(object sender, RoutedEventArgs e)
        {
            _gridDisplayChannels.ReplaceRange(_forecastData.Channels);
            ChangeGridDisplayData(_gridDisplayChannels);
            IEnumerable<MediaPlanTuple> visibleTuples = _allMediaPlans;
            /*swgGrid.VisibleTuplesChanged(visibleTuples);
            sdgGrid.VisibleTuplesChanged(visibleTuples);*/
            gridsDataManipulation.VisibleTuplesChanged(visibleTuples);
            //cgGrid.VisibleTuplesChanged(visibleTuples);
            gridShowSelected = false;
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            // Get the ScrollViewer containing the DataGrid
            ScrollViewer scrollViewer = svBorder;
            // Calculate the new vertical offset based on the mouse wheel delta
            double newVerticalOffset = scrollViewer.VerticalOffset - e.Delta*0.3;

            // Ensure the new vertical offset is within bounds
            if (newVerticalOffset < 0)
                newVerticalOffset = 0;
            else if (newVerticalOffset > scrollViewer.ScrollableHeight)
                newVerticalOffset = scrollViewer.ScrollableHeight;

            // Set the new vertical offset
            scrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }

        #region Report radio buttons

        public void AddRealizations(ObservableRangeCollection<MediaPlanRealized> mpRealized, DateOnly lastDateImport)
        {
            rbRealized.IsEnabled = true;
            rbExpectedRealized.IsEnabled = true;
            SetLastDataImportDate(lastDateImport);

            gridsDataManipulation.AddRealizations(mpRealized);
            /*cgGrid._mpRealized = mpRealized;
            sdgGrid._mpRealized = mpRealized;
            swgGrid._mpRealized = mpRealized;*/
        }
        private void SetLastDataImportDate(DateOnly lastDateImport)
        {
            var dateImportString = "-";
            //var separationDate = DateOnly.FromDateTime(startDate);
            if (lastDateImport > DateOnly.FromDateTime(endDate))
            {
                dateImportString = DateOnly.FromDateTime(endDate).ToShortDateString();
                //separationDate = DateOnly.FromDateTime(endDate);
            }
            else if (lastDateImport > DateOnly.FromDateTime(startDate))
            {
                dateImportString = DateOnly.FromDateTime(startDate).ToShortDateString();
                //separationDate = lastDateImport;
            }

            lblLastDataDate.Content = "Last data gathered on: " + dateImportString;

            gridsDataManipulation.SeparationDate = lastDateImport;
            /*cgGrid.SeparationDate = lastDateImport;
            sdgGrid.SeparationDate = lastDateImport;
            swgGrid.SetSeparationDate(lastDateImport);*/
        }
        private void rbExpected_Checked(object sender, RoutedEventArgs e)
        {
            /*if (cgGrid != null)
                cgGrid.ChangeDataForShowing("expected");

            if (sdgGrid != null)
                sdgGrid.ChangeDataForShowing("expected");

            if (swgGrid != null)
                swgGrid.ChangeDataForShowing("expected");*/
            gridsDataManipulation.ChangeDataForShowing("expected");
        }

        private void rbRealized_Checked(object sender, RoutedEventArgs e)
        {
            /* if (cgGrid != null)
                 cgGrid.ChangeDataForShowing("realized");

             if (sdgGrid != null)
                 sdgGrid.ChangeDataForShowing("realized");

             if (swgGrid != null)
                 swgGrid.ChangeDataForShowing("realized");*/
            gridsDataManipulation.ChangeDataForShowing("realized");

        }

        private void rbExpectedRealized_Checked(object sender, RoutedEventArgs e)
        {
            /*if (cgGrid != null)
                cgGrid.ChangeDataForShowing("expectedrealized");

            if (sdgGrid != null)
                sdgGrid.ChangeDataForShowing("expectedrealized");

            if (swgGrid != null)
                swgGrid.ChangeDataForShowing("expectedrealized");*/
            gridsDataManipulation.ChangeDataForShowing("expectedrealized");

        }

        public void AddIntoUpdatedRealizations(DateOnly date, int chrdsid, SpotDTO spot)
        {
            _updatedRealizations.Add(Tuple.Create(date, chrdsid, spot));
        }

        public void CheckUpdatedRealizations()
        {
            if (_updatedRealizations.Count == 0)
                return;

            /*foreach(var chrdsid in _updatedRealizations.Select(t => t.Item2))
            {
                cgGrid.RecalculateGoalsRealized(chrdsid);
            }

            foreach (var updatedRealization in _updatedRealizations)
            {
                var spot = updatedRealization.Item3;
                if (spot == null)
                    continue; // Undedicated spot is ignoring
                int chrdsid = updatedRealization.Item2;
                var date = updatedRealization.Item1;
                if (!_forecastData.ChrdsidChidDict.ContainsKey(chrdsid))
                    continue;
                int chid = _forecastData.ChrdsidChidDict[chrdsid];
                var channel = _forecastData.Channels.FirstOrDefault(c => c.chid == chid);
                if (channel == null)
                    continue;
                sdgGrid.RecalculateGoals(channel, date, spot, true);

                swgGrid.RecalculateGoals(channel, date, spot, true);
            }*/

            foreach (var updatedRealization in _updatedRealizations)
            {
                var spot = updatedRealization.Item3;
                if (spot == null)
                    continue; // Undedicated spot is ignoring
                int chrdsid = updatedRealization.Item2;
                var date = updatedRealization.Item1;
                if (!_forecastData.ChrdsidChidDict.ContainsKey(chrdsid))
                    continue;
                int chid = _forecastData.ChrdsidChidDict[chrdsid];
                var channel = _forecastData.Channels.FirstOrDefault(c => c.chid == chid);
                if (channel == null)
                    continue;
                gridsDataManipulation.RecalculateGoals(channel, date, spot, true);
            }
            

        }


        #endregion


    }

}
            
