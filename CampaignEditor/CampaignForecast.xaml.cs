using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.GoalsDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanRef;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
        private PricelistController _pricelistController;
        private SeasonalityController _seasonalityController;
        private SectableController _sectableController;

        private readonly IAbstractFactory<AddSchema> _factoryAddSchema;
        private readonly IAbstractFactory<AMRTrim> _factoryAmrTrim;


        private ClientDTO _client;
        private CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        // for checking if certain character can be written in spot cells
        HashSet<char> spotCodes = new HashSet<char>();

        // number of frozen columns
        int mediaPlanColumns = 24;
        bool resetRefData = false;

        public int FrozenColumnsNum
        {
            get { return (int)GetValue(FrozenColumnsNumProperty); }
            set { SetValue(FrozenColumnsNumProperty, value); }
        }

        public static readonly DependencyProperty FrozenColumnsNumProperty =
            DependencyProperty.Register(nameof(FrozenColumnsNum), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        private Dictionary<ChannelDTO, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>> _channelMPDict =
            new Dictionary<ChannelDTO, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>>();
        private ObservableCollection<MediaPlanTuple> _showMP 
            = new ObservableCollection<MediaPlanTuple>();

        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository, 
            IChannelCmpRepository channelCmpRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanHistRepository mediaPlanHistRepository,
            IMediaPlanRefRepository mediaPlanRefRepository,
            ISpotRepository spotRepository,
            IGoalsRepository goalsRepository,
            IPricelistRepository pricelistRepository,
            ISeasonalityRepository seasonalityRepository,
            ISectableRepository sectableRepository,
            IAbstractFactory<AddSchema> factoryAddSchema,
            IAbstractFactory<AMRTrim> factoryAmrTrim)
        {
            this.DataContext = this;
            this.FrozenColumnsNum = mediaPlanColumns;

            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanHistController = new MediaPlanHistController(mediaPlanHistRepository);
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _spotController = new SpotController(spotRepository);
            _goalsController = new GoalsController(goalsRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _sectableController = new SectableController(sectableRepository);

            _factoryAddSchema = factoryAddSchema;
            _factoryAmrTrim = factoryAmrTrim;

            InitializeComponent();

            if (MainWindow.user.usrlevel == 2)
            {
                this.IsEnabled = false;
            }
        }


        #region Initialization
        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            for (int i = 0; i < spots.Count(); i++)
            {
                spotCodes.Add((char)('A' + i));
            }

            var exists = (await _mediaPlanRefController.GetMediaPlanRef(_campaign.cmpid) != null);
            if ( exists )
            {
                await LoadData();
            }
            else
            {
                SetGrid(gridInit);

                lbDateRanges.Initialize(new DateRangeItem());
                // waiting for function Init_Click to activate LoadData function
            }

        }

        private async Task LoadData()
        {
            SetGrid(gridLoading);

            // Filling lvChannels and dictionary

            await FillLvChannels();
            await FillDictionary();
            await FillGoals();
            await FillLoadedDateRanges();

            InitializeDateColumns();

            ICollectionView myDataView = CollectionViewSource.GetDefaultView(_showMP);
            dgSchema.ItemsSource = myDataView;

            myDataView.SortDescriptions.Add(new SortDescription("MediaPlan.name", ListSortDirection.Ascending));
            myDataView.Filter = d => ((MediaPlanTuple)d).MediaPlan.active == true;

            _showMP.CollectionChanged += OnCollectionChanged;

            SetGrid(gridForecast);
        }

        // Method to handle the CollectionChanged event
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Call Refresh() on the view to update it
            ICollectionView view = CollectionViewSource.GetDefaultView(_showMP);
            view.Refresh();
        }

        #region GridInit
        // When we initialize forecast, we need to set dates for search
        private async void Init_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            bool validRanges = CheckDateRanges();
            if (!validRanges)
            {
                return;
            }

            if (resetRefData)
            {
                await DeleteData();
            }

            for (int i = 0; i < lbDateRanges.Items.Count - 1; i++)
            {
                DateRangeItem dri = lbDateRanges.Items[i] as DateRangeItem;

                int? ymdFrom = TimeFormat.DPToYMDInt(dri.dpFrom);
                int? ymdTo = TimeFormat.DPToYMDInt(dri.dpTo);

                if (ymdFrom.HasValue && ymdTo.HasValue)
                {
                    await _mediaPlanRefController.CreateMediaPlanRef(
                    new MediaPlanRefDTO(_campaign.cmpid, ymdFrom.Value, ymdTo.Value));
                }
            }

            SetGrid(gridLoading);

            await InsertAndLoadData();

            SetGrid(gridForecast);

        }

        private async Task DeleteData()
        {
            await _mediaPlanRefController.DeleteMediaPlanRefById(_campaign.cmpid);
            var mediaPlans = await _mediaPlanController.GetAllMediaPlansByCmpid(_campaign.cmpid);
            
            foreach (var mediaPlan in mediaPlans)
            {
                await _mediaPlanHistController.DeleteMediaPlanHistByXmpid(mediaPlan.xmpid);
                await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlan.xmpid);
                await _mediaPlanController.DeleteMediaPlanById(mediaPlan.xmpid);
            }
            
        }

        private bool CheckDateRanges()
        {
            // no range entered
            if (lbDateRanges.Items.Count == 1)
            {
                MessageBox.Show("Enter date range");
                return false;
            }

            // check intercepting or invalid date values
            for (int i = 0; i < lbDateRanges.Items.Count - 1; i++)
            {
                DateRangeItem dri = lbDateRanges.Items[i] as DateRangeItem;
                if (!dri.CheckValidity())
                {
                    MessageBox.Show("Invalid dates");
                    return false;
                }
                for (int j = i + 1; j < lbDateRanges.Items.Count - 1; j++)
                {
                    DateRangeItem dri2 = lbDateRanges.Items[j] as DateRangeItem;
                    if (dri.checkIntercepting(dri2))
                    {
                        MessageBox.Show("Dates are intercepting");
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        private async Task InsertAndLoadData()
        {

            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);

            List<Task> tasks = new List<Task>();

            foreach (var channelCmp in channelCmps)
            {
                Task task = Task.Run(() => InsertInDatabase(channelCmp.chid));
                tasks.Add(task);
            }

            // waiting for all tasks to finish
            await Task.WhenAll(tasks);

            await LoadData();

        }

        #region Inserting in database
        private async Task InsertInDatabase(int chid)
        {
            var schemas = await _schemaController.GetAllChannelSchemasWithinDateAndTime(
                chid, DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate),
                _campaign.cmpstime, _campaign.cmpetime);

            foreach (var schema in schemas)
            {
                MediaPlanDTO mediaPlan = await SchemaToMP(schema);
                var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlan);
            }
        }

        // reaching or creating mediaPlan
        private async Task<MediaPlanDTO> SchemaToMP(SchemaDTO schema)
        {
            MediaPlanDTO mediaPlan = null;
            if ((mediaPlan = await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid)) != null)
            {
                if (AreEqualMediaPlanAndSchema(mediaPlan, schema))
                    return mediaPlan;
                else
                {
                    for (int i = 0; i < _showMP.Count(); i++)
                    {
                        if (AreMediaPlansEqual(_showMP[i].MediaPlan, mediaPlan))
                        {
                            _showMP.RemoveAt(i);
                        }
                    }

                    foreach (var channel in _channelMPDict.Keys)
                    {
                        if (channel.chid == mediaPlan.chid)
                        {
                            for (int i = 0; i < _channelMPDict[channel].Count(); i++)
                            {
                                if (AreMediaPlansEqual(_channelMPDict[channel][i].Item1, mediaPlan))
                                {
                                    _channelMPDict[channel].RemoveAt(i);
                                }
                            }

                            await _mediaPlanTermController.DeleteMediaPlanTermByXmpId(mediaPlan.xmpid);
                            await _mediaPlanController.DeleteMediaPlanById(mediaPlan.xmpid);
                        }
                    }
                    
                    
                }
                    
            }
            /*var channelCmp = await _channelCmpController.GetChannelCmpByIds(_campaign.cmpid, schema.chid);
                var pricelist = await _pricelistController.GetPricelistById(channelCmp.plid);
                var seasonality = await _seasonalityController.GetSeasonalityById(pricelist.seastbid);
                var sectable = await _sectableController.GetSectableById(pricelist.sectbid);*/

            CreateMediaPlanDTO createMediaPlan = new CreateMediaPlanDTO(schema.id, _campaign.cmpid, schema.chid,
            schema.name.Trim(), 1, schema.position, schema.stime, schema.etime, schema.blocktime,
            schema.days, schema.type, schema.special, schema.sdate, schema.edate, schema.progcoef,
            schema.created, schema.modified, 0, 100, 0, 100, 0, 100, 0, 100, 0, 0, 0, 0, 1, 1, 0, true);

            return await _mediaPlanController.CreateMediaPlan(createMediaPlan);
              

        }

        private async Task<List<MediaPlanTermDTO>> MediaPlanToMPTerm(MediaPlanDTO mediaPlan)
        {

            List<DateTime> availableDates = GetAvailableDates(mediaPlan);
            DateTime started = startDate;

            int n = (int)(endDate - startDate).TotalDays;
            var mediaPlanDates = new List<MediaPlanTermDTO>();

            List<DateTime> sorted = availableDates.OrderBy(d => d).ToList();

            for (int i = 0, j = 0; i <= n && j < sorted.Count(); i++)
            {
                if (started.AddDays(i).Date == sorted[j].Date)
                {
                    CreateMediaPlanTermDTO mpTerm = new CreateMediaPlanTermDTO(mediaPlan.xmpid, DateOnly.FromDateTime(sorted[j]), null);
                    mediaPlanDates.Add(await _mediaPlanTermController.CreateMediaPlanTerm(mpTerm));
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

        #endregion

        #region Filling lists
        private async Task FillLvChannels()
        {
            lvChannels.Items.Clear();
            
            var channelIds = await _mediaPlanController.GetAllChannelsByCmpid(_campaign.cmpid);
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (var chid in channelIds)
            {
                ChannelDTO channel = await _channelController.GetChannelById(chid);
                channels.Add(channel);
            }

            channels = channels.OrderBy(c => c.chname).ToList();

            foreach (ChannelDTO channel in channels)
            {
                lvChannels.Items.Add(channel);
            }

        }

        private async Task FillDictionary()
        {
            _channelMPDict.Clear();

            foreach (ChannelDTO channel in lvChannels.Items)
            {
                List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>> emptyList = new List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>();
                _channelMPDict.Add(channel, emptyList);
            }

            int n = (int)(endDate - startDate).TotalDays;

            foreach (ChannelDTO channel in _channelMPDict.Keys)
            {
                var mediaPlans = await _mediaPlanController.GetAllChannelMediaPlans(channel.chid);
                foreach (MediaPlanDTO mediaPlan in mediaPlans)
                {
                    List<MediaPlanTermDTO> mediaPlanTerms = (List<MediaPlanTermDTO>)await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
                    var mediaPlanDates = new List<MediaPlanTermDTO>();
                    for (int i = 0, j = 0; i <= n && j < mediaPlanTerms.Count(); i++)
                    {
                        if (DateOnly.FromDateTime(startDate.AddDays(i)) == mediaPlanTerms[j].date)
                        {
                            mediaPlanDates.Add(mediaPlanTerms[j]);
                            j++;
                        }
                        else
                        {
                            mediaPlanDates.Add(null);
                        }
                    }

                    Tuple<MediaPlanDTO, List<MediaPlanTermDTO>> row = Tuple.Create(mediaPlan, mediaPlanDates);
                    _channelMPDict[channel].Add(row);
                }
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
                label.FontSize = 18;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;

                spLoadedDateRanges.Children.Add(label);

            }
        }

        private void btnResetDates_Click(object sender, RoutedEventArgs e)
        {
            resetRefData = true;
            SetGrid(gridInit);
            LoadGridInit();
        }

        private async void LoadGridInit()
        {
            lbDateRanges.Items.Clear();
            lbDateRanges.Initialize(new DateRangeItem());
            var dateRanges = await _mediaPlanRefController.GetAllMediaPlanRefsByCmpid(_campaign.cmpid);

            bool first = true;
            foreach (var dateRange in dateRanges)
            {
                DateTime start = TimeFormat.YMDStringToDateTime(dateRange.datestart.ToString());
                DateTime end = TimeFormat.YMDStringToDateTime(dateRange.dateend.ToString());

                if (first)
                {
                    DateRangeItem dri = lbDateRanges.Items[0] as DateRangeItem;
                    dri.SetDates(start, end);
                    first = false;
                }
                else
                {
                    DateRangeItem dri = new DateRangeItem();
                    dri.SetDates(start, end);
                    lbDateRanges.Items.Insert(lbDateRanges.Items.Count - 1, dri);
                }                                              
            }

            lbDateRanges.ResizeItems(lbDateRanges.Items);
            
            btnInitCancel.Visibility = Visibility.Visible;
        }

        private void btnInitCancel_Click(object sender, RoutedEventArgs e)
        {
            SetGrid(gridForecast);
        }

        #endregion

        #region lvChannels
        private void lvChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selectedItems = e.AddedItems;
            var deselectedItems = e.RemovedItems;

            
            if (selectedItems.Count>0) 
            {
                ChannelDTO selectedItem = selectedItems[0]! as ChannelDTO;

                for (int k = 0; k < _channelMPDict[selectedItem].Count; k++)
                {
                    MediaPlanDTO mediaPlanDTO = _channelMPDict[selectedItem][k].Item1;
                    ObservableCollection<MediaPlanTermDTO> mediaPlanTerms = new ObservableCollection<MediaPlanTermDTO>();
                    foreach (MediaPlanTermDTO mpTerm in _channelMPDict[selectedItem][k].Item2)
                        mediaPlanTerms.Add(mpTerm);

                    _showMP.Add(new MediaPlanTuple(mediaPlanDTO, mediaPlanTerms));
                }
            }

            if (deselectedItems.Count>0)
            {
                ChannelDTO deselectedItem = deselectedItems[0]! as ChannelDTO;

                for (int i=0; i < _showMP.Count(); i++)
                {
                    var tuple = _showMP[i];
                    foreach (var channelTuple in _channelMPDict[deselectedItem])
                        if (channelTuple.Item1 == tuple.MediaPlan)
                        {
                            _showMP.Remove(tuple);
                            i--;
                        }
                }
            }
        }

        #endregion

        #region Goals

        private async Task FillGoals()
        {
            GoalsDTO goals = await _goalsController.GetGoalsByCmpid(_campaign.cmpid);

            if (goals != null)
            {
                if (goals.budget == 0)
                {
                    bBudget.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bBudget.Visibility = Visibility.Visible;
                    lblBudgetTarget.Content = "/" + goals.budget.ToString();
                }

                if (goals.grp == 0)
                {
                    bGRP.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bGRP.Visibility = Visibility.Visible;
                    lblGRPTarget.Content = "/" + goals.grp.ToString();
                }

                if (goals.ins == 0)
                {
                    bInsertations.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bInsertations.Visibility = Visibility.Visible;
                    lblInsertationsTarget.Content = "/" + goals.ins.ToString();
                }

                if (goals.rch == 0)
                {
                    bReach.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bReach.Visibility = Visibility.Visible;
                    lblReachTarget.Content = "/" + "from " + goals.rch_f1.ToString().Trim() + " to " + goals.rch_f2.ToString().Trim() +
                    " , " + goals.rch.ToString().Trim() + " %";
                }
            }
        }

        #endregion

        #region Date Columns
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
                column.Header = date.ToString("dd.MM.yy");
                    

                var binding = new Binding($"Terms[{dates.IndexOf(date)}].spotcode");
                //binding.ValidationRules.Add(new CharLengthValidationRule(1)); // add validation rule to restrict input to a single character
                column.Binding = binding;

                var cellStyle = new Style(typeof(DataGridCell));

                // Adding setters to cells
                //var keyDownEventSetter = new EventSetter(DataGridCell.PreviewTextInputEvent, new TextCompositionEventHandler(OnCellPreviewTextInput));
                var textInputEventSetter = new EventSetter(PreviewTextInputEvent, new TextCompositionEventHandler(OnCellPreviewTextInput));
                var keyDownEventSetter = new EventSetter(PreviewKeyDownEvent, new KeyEventHandler(OnCellPreviewKeyDown));

                cellStyle.Setters.Add(textInputEventSetter);
                cellStyle.Setters.Add(keyDownEventSetter);
                column.CellStyle = cellStyle;

                var trigger = new DataTrigger();
                trigger.Binding = new Binding($"Terms[{dates.IndexOf(date)}]");
                trigger.Value = null;
                trigger.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGoldenrodYellow)); // Set background to yellow if value is null

                //set background to yellow if column is weekend day
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    // Set the background of the column header to yellow
                    column.HeaderStyle = new Style(typeof(DataGridColumnHeader));
                    column.HeaderStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(3, 3, 1.5, 3)));
                    column.HeaderStyle.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));
                  

                    trigger.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(3, 1, 1, 1)));
                    trigger.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));

                    column.CellStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(3, 1, 1, 1)));
                    column.CellStyle.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));

                    
                }
                else if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Set the background of the column header to yellow
                    column.HeaderStyle = new Style(typeof(DataGridColumnHeader));
                    column.HeaderStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1.5, 3, 3, 3)));
                    column.HeaderStyle.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));

                    trigger.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1, 1, 3, 1)));
                    trigger.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));

                    column.CellStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1, 1, 3, 1)));
                    column.CellStyle.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));
                }

                column.CellStyle.Triggers.Add(trigger);

                column.CellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGreen)); // Set background to green if value is not null
                column.CanUserSort = false;
                column.CanUserResize = false;
                column.CanUserReorder = false;
                column.IsReadOnly = true;

                // when cell is focused, set background to orange
                Trigger focusTrigger = new Trigger();
                focusTrigger.Property = DataGridCell.IsFocusedProperty;
                focusTrigger.Value = true;
                focusTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.Orange));
                column.CellStyle.Triggers.Add(focusTrigger);

                // Add the column to the DataGrid
                dgSchema.Columns.Add(column);
            }
        }

        private async void OnCellPreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            DataGridCell cell = sender as DataGridCell;
            TextBlock textBlock = cell.Content as TextBlock;

            char? spotcodeNull = e.Text.Trim()[0];

            if (spotcodeNull.HasValue)
            {
                char spotcode = Char.ToUpper(spotcodeNull.Value);
                if (spotCodes.Contains(spotcode))
                {
                    // if cell already have 2 spots, delete them and write only one, else add spotcode
                    if (cell.Content.ToString().Length == 2 || 
                        (textBlock != null && textBlock.Text.Trim().Length == 2))
                    {
                        cell.Content = spotcode;
                    }
                    else if (textBlock != null)
                    {
                        cell.Content = textBlock.Text.Trim() + spotcode.ToString();
                    }
                    else if (textBlock == null)
                    {
                        cell.Content += spotcode.ToString();
                    }
                      

                    var mpTerm = GetSelectedMediaPlanTermDTO(cell);
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                        new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, cell.Content.ToString()));

                    mpTerm.spotcode = cell.Content.ToString().Trim();

                }
                // For entering numbers
                else if (Char.IsDigit(spotcode))
                {
                    int scNum = spotcode - '0';
                    if (scNum > 0 && scNum <= spotCodes.Count())
                    {
                        char spCode = (char)('A' + scNum - 1);
                        // if cell already have 2 spots, delete them and write only one, else add spotcode
                        if (cell.Content.ToString().Length == 2 ||
                            (textBlock != null && textBlock.Text.Trim().Length == 2))
                        {
                            cell.Content = spCode;
                        }
                        else if (textBlock != null)
                        {
                            cell.Content = textBlock.Text.Trim() + spCode.ToString();
                        }
                        else if (textBlock == null)
                        {
                            cell.Content += spCode.ToString();
                        }

                        var mpTerm = GetSelectedMediaPlanTermDTO(cell);
                        await _mediaPlanTermController.UpdateMediaPlanTerm(
                            new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, cell.Content.ToString().Trim()));

                        mpTerm.spotcode = cell.Content.ToString().Trim();

                        //cell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                    }

                }
            }
            

        }     

        private async void OnCellPreviewKeyDown(object sender, KeyEventArgs e)
        {

            DataGridCell cell = sender as DataGridCell;
            
            // Allow navigation with arrows
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                return;
            }

            // if cell is not binded to mediaPlanTerm, disable editing
            var tuple = (MediaPlanTuple)cell.DataContext;
            var mpTerms = tuple.Terms;
            var index = cell.Column.DisplayIndex - mediaPlanColumns;
            var mpTerm = mpTerms[index];
            if (mpTerm == null)
            {
                e.Handled = true;
                return;
            }

            // edit cell
            var textBlock = cell.Content;
            var tb2 = cell.Content as TextBlock;
            string text = "";
            if (tb2 != null)
            {
                text = tb2.Text;
            }
            else if (textBlock != null)
            {
                text = textBlock.ToString();
            }

            // move focus to the next available cell in a row
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                e.Handled = true;
                cell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            if ((e.Key == Key.Delete || e.Key == Key.Back) && text != null)
            {
                e.Handled = true;

                string? spotcode = mpTerm.spotcode;
                if (spotcode == null || spotcode.Length == 1)
                {
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, null));

                    mpTerm.spotcode = null;
                    cell.Content = "";
                }
                else if (spotcode.Length == 2)
                {
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, spotcode[0].ToString().Trim()));

                    mpTerm.spotcode = spotcode[0].ToString();
                    cell.Content = spotcode[0];
                }

            }

        }
        

        public static DataGridCell GetCell(DataGrid dataGrid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dataGrid, row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual visual = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = visual as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(visual);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        private MediaPlanTermDTO GetSelectedMediaPlanTermDTO(DataGridCell cell)
        {
            // Traverse the visual tree to find the DataGridRow and DataGridCell that contain the selected cell
            DependencyObject parent = VisualTreeHelper.GetParent(cell);
            while (parent != null && !(parent is DataGridRow))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent == null)
            {
                return null; // selected cell is not in a DataGridRow
            }
            DataGridRow row = parent as DataGridRow;

            parent = VisualTreeHelper.GetParent(cell);

            var selectedCell = cell;
            // Get the index of the selected cell in the row
            int columnIndex = selectedCell.Column.DisplayIndex;

            // Get the bound item for the selected row
            var tuple = row.Item as MediaPlanTuple;
            if (tuple == null)
            {
                return null; // row is not bound to a tuple
            }
            ObservableCollection<MediaPlanTermDTO> mpTerms = tuple.Terms;

            // Get the MediaPlanTerm for the selected cell
            int rowIndex = row.GetIndex();
            MediaPlanTermDTO mpTermDTO = mpTerms[columnIndex - FrozenColumnsNum]; // we have n freezed columns

            return mpTermDTO;
        }

        #endregion

        #region MediaPlan columns

            #region ContextMenu
        private async void dgSchema_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // check if it's clicked on header
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;

            if (IsCellInDataGridHeader(dependencyObject))
            {
                ContextMenu menu = new ContextMenu();
                for (int i = 0; i < mediaPlanColumns; i++)
                {
                    var column = dgSchema.Columns[i];

                    MenuItem item = new MenuItem();
                    item.Header = column.Header.ToString().Trim();
                    item.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                    item.Click += (obj, ea) =>
                    {
                        column.Visibility = item.IsChecked ? Visibility.Hidden : Visibility.Visible;
                        item.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                    };

                    menu.Items.Add(item);
                }

                dgSchema.ContextMenu = menu;
            }
            else 
            {
                ContextMenu menu = new ContextMenu();
                MenuItem deleteItem = new MenuItem();
                deleteItem.Header = "Delete MediaPlan";
                deleteItem.Click += async (obj, ea) =>
                {
                    var mediaPlanTuple = dgSchema.SelectedItem as MediaPlanTuple;
                    if (mediaPlanTuple != null)
                    {
                        mediaPlanTuple.MediaPlan.active = false;
                        _showMP.Remove(mediaPlanTuple);
                        var mediaPlan = mediaPlanTuple.MediaPlan;                        
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));
                    }
                };
                menu.Items.Add(deleteItem);

                MenuItem addMediaPlanItem = new MenuItem();
                addMediaPlanItem.Header = "Add MediaPlan";
                addMediaPlanItem.Click += async (obj, ea) =>
                {
                    var f = _factoryAddSchema.Create();
                    await f.Initialize(_campaign);
                    f.ShowDialog();
                    if (f._schema != null)
                    {
                        var schema = await _schemaController.CreateGetSchema(f._schema);
                        MediaPlanDTO mediaPlan = await SchemaToMP(schema);
                        var channel = _channelMPDict.Keys.First(ch => ch.chid == mediaPlan.chid);

                        if (!MPInList(mediaPlan, _channelMPDict[channel]))
                        {
                            var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlan);
                            var tuple = Tuple.Create(mediaPlan, mediaPlanTerms);
                            _channelMPDict[channel].Add(tuple);
                            _showMP.Add(new MediaPlanTuple(mediaPlan, new ObservableCollection<MediaPlanTermDTO>(mediaPlanTerms)));
                        }
                    }
                };
                menu.Items.Add(addMediaPlanItem);

                // Traverse the visual tree to get the clicked DataGridCell object
                while ((dependencyObject != null) && !(dependencyObject is DataGridCell))
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }

                if (dependencyObject == null)
                {
                    return;
                }

                DataGridCell cell = dependencyObject as DataGridCell;

                var mediaPlanTuple = dgSchema.SelectedItem as MediaPlanTuple;
                if (mediaPlanTuple == null)
                {
                    return;
                }
                var mediaPlan = mediaPlanTuple.MediaPlan;

                MenuItem trimAmr = new MenuItem();
                // Check if the clicked cell is in the "AMR" columns
                if (cell.Column.Header.ToString() == "AMR 1")
                {           
                    trimAmr.Header = "Trim Amr1";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR 1", "amr1trim", mediaPlan.amr1trim);
                }
                else if (cell.Column.Header.ToString() == "AMR 2")
                {
                    trimAmr.Header = "Trim Amr2";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR 2", "amr2trim", mediaPlan.amr2trim);
                }
                else if (cell.Column.Header.ToString() == "AMR 3")
                {
                    trimAmr.Header = "Trim Amr3";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR 3", "amr3trim", mediaPlan.amr3trim);
                }
                else if (cell.Column.Header.ToString() == "AMR Sale")
                {
                    trimAmr.Header = "Trim Amr Sale";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR Sale", "amrsaletrim", mediaPlan.amrsaletrim);
                }
                else
                {
                    trimAmr.Header = "Trim All Amrs";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMRs", "amrtrimall", null);
                }
                menu.Items.Add(trimAmr);
                dgSchema.ContextMenu = menu;
            }
        }

        private bool MPInList(MediaPlanDTO mediaPlan, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>> list)
        {
            foreach (var mpTuple in list) 
            {
              
                var mp = mpTuple.Item1;
               
                if (AreMediaPlansEqual(mediaPlan, mp))
                {
                    return true;
                }
            }
            return false;
        }
        public bool AreMediaPlansEqual(MediaPlanDTO plan1, MediaPlanDTO plan2)
        {
            return plan1.xmpid == plan2.xmpid &&
                   plan1.schid == plan2.schid &&
                   plan1.cmpid == plan2.cmpid &&
                   plan1.chid == plan2.chid &&
                   plan1.name == plan2.name &&
                   plan1.position == plan2.position &&
                   plan1.stime == plan2.stime &&
                   plan1.etime == plan2.etime &&
                   plan1.blocktime == plan2.blocktime &&
                   plan1.days == plan2.days &&
                   plan1.sdate == plan2.sdate &&
                   plan1.edate == plan2.edate;                  
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

        private async Task<RoutedEventHandler> TrimAmrAsync(MediaPlanDTO mediaPlan, string message, string attr,  int? trimValue)
        {
            async void handler(object sender, RoutedEventArgs e)
            {
                var f = _factoryAmrTrim.Create();
                f.Initialize(message, trimValue);
                f.ShowDialog();
                if (f.changed)
                {
                    switch (attr)
                    {
                        case "amr1trim":
                            mediaPlan.amr1trim = f.newValue;
                            break;
                        case "amr2trim":
                            mediaPlan.amr2trim = f.newValue;
                            break;
                        case "amr3trim":
                            mediaPlan.amr3trim = f.newValue;
                            break;
                        case "amrsaletrim":
                            mediaPlan.amrsaletrim = f.newValue;
                            break;
                        case "amrtrimall":
                            mediaPlan.amr1trim = f.newValue;
                            mediaPlan.amr2trim = f.newValue;
                            mediaPlan.amr3trim = f.newValue;
                            mediaPlan.amrsaletrim = f.newValue;
                            break;
                        default:
                            break;
                    }
                    
                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));
                }
            }

            return handler;
        }

        private bool IsCellInDataGridHeader(DependencyObject obj)
        {
            var header = obj;
            while (header != null && header.DependencyObjectType.Name != "DataGridHeaderBorder")
            {
                header = VisualTreeHelper.GetParent(header);
            }
            return header != null;
        }




        #endregion

        private async void TextBoxAMRTrim_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tuple = dgSchema.SelectedItems[0] as MediaPlanTuple;
            var textBox = sender as TextBox;

            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            string propertyName = bindingExpr?.ResolvedSourcePropertyName;

            if (tuple != null)
            {
                var mediaPlan = tuple.MediaPlan;
                int value = 0;

                if (propertyName == "amr1trim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amr1trim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amr1trim.ToString();
                    }
                }
                else if (propertyName == "amr2trim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amr2trim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amr2trim.ToString();
                    }
                }
                else if (propertyName == "amr3trim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amr3trim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amr3trim.ToString();
                    }
                }
                else if (propertyName == "amrsaletrim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amrsaletrim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amrsaletrim.ToString();
                    }
                }
            }
        }

        private async void TextBoxCoef_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tuple = dgSchema.SelectedItems[0] as MediaPlanTuple;
            var textBox = sender as TextBox;

            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            string propertyName = bindingExpr?.ResolvedSourcePropertyName;

            if (tuple != null)
            {
                var mediaPlan = tuple.MediaPlan;
                float value = 0f;

                if (propertyName == "progcoef")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || float.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.progcoef = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mediaPlan));

                        // also should update value in progschema
                        var schema = await _schemaController.GetSchemaById(mediaPlan.schid);
                        schema.progcoef = mediaPlan.progcoef;
                        await _schemaController.UpdateSchema(new UpdateSchemaDTO(schema));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.progcoef.ToString();
                    }
                }
            }
        }

        #endregion

        #region Page

        // setting one visible Grid

        private void SetGrid(Grid grid)
        {
            gridInit.Visibility = Visibility.Hidden;
            gridLoading.Visibility = Visibility.Hidden;
            gridForecast.Visibility = Visibility.Hidden;

            if (grid.Name == "gridInit")
                gridInit.Visibility = Visibility.Visible;
            if (grid.Name == "gridLoading")
                gridLoading.Visibility = Visibility.Visible;
            if (grid.Name == "gridForecast")
                gridForecast.Visibility = Visibility.Visible;

        }

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

        private async void dgSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgHist.Items.Clear();
            if (dgSchema.SelectedItems.Count == 0)
                return;
            else
            {
                var mediaPlanTuple = dgSchema.SelectedItems[0] as MediaPlanTuple;
                var mediaPlan = mediaPlanTuple.MediaPlan;

                var mediaPlanHists = await _mediaPlanHistController.GetAllMediaPlanHistsBySchid(mediaPlan.schid);
                foreach (var mediaPlanHist in mediaPlanHists)
                    dgHist.Items.Add(mediaPlanHist);
            }
        }
    }
}
