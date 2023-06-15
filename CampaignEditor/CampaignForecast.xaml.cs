using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.GoalsDTO;
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private DatabaseFunctionsController _databaseFunctionsController;

        private readonly IAbstractFactory<AddSchema> _factoryAddSchema;
        private readonly IAbstractFactory<AMRTrim> _factoryAmrTrim;
        private readonly IAbstractFactory<MediaPlanGrid> _factoryMediaPlanGrid;

        private SelectedMPGoals SelectedMediaPlan = new SelectedMPGoals();

        private CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;
        
        int mediaPlanColumns = 25;

        private ObservableCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableCollection<MediaPlanTuple>();

        private ObservableCollection<ChannelDTO> _selectedChannels = new ObservableCollection<ChannelDTO>();

        private ObservableCollection<MediaPlanHist> _showMPHist = new ObservableCollection<MediaPlanHist>();

        private MPGoals mpGoals = new MPGoals();

        MediaPlanConverter _converter;

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
            IDatabaseFunctionsRepository databaseFunctionsRepository,
            IAbstractFactory<AddSchema> factoryAddSchema,
            IAbstractFactory<AMRTrim> factoryAmrTrim,
            IAbstractFactory<MediaPlanConverter> factoryConverter,
            IAbstractFactory<MediaPlanGrid> mediaPlanGrid)
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

            _databaseFunctionsController = new DatabaseFunctionsController(databaseFunctionsRepository);

            _factoryAddSchema = factoryAddSchema;
            _factoryAmrTrim = factoryAmrTrim;
            _factoryMediaPlanGrid = mediaPlanGrid;

            _converter = factoryConverter.Create();

            InitializeComponent();

            if (MainWindow.user.usrlevel == 2)
            {
                this.IsEnabled = false;
            }
        }


        #region Initialization
        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            dgHist.ItemsSource = _showMPHist;

        }

        public async Task InsertAndLoadData()
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

            await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40);
            await CalculateMPValues();

            await LoadData();

        }

        public async Task LoadData()
        {
            // Filling lvChannels and dictionary

            await FillLvChannels();
            await FillMPList();
            await FillGoals();
            await FillLoadedDateRanges();
            // For dgMediaPlans
            await InitializeDataGrid();

        }

        private async Task InitializeDataGrid()
        {
            dgMediaPlans._converter = _converter;
            dgMediaPlans._factoryAddSchema = _factoryAddSchema;
            dgMediaPlans._factoryAmrTrim = _factoryAmrTrim;
            dgMediaPlans._schemaController = _schemaController;
            dgMediaPlans._mediaPlanController = _mediaPlanController;
            dgMediaPlans._mediaPlanTermController = _mediaPlanTermController;
            dgMediaPlans._spotController = _spotController;
            dgMediaPlans._selectedChannels = _selectedChannels;
            dgMediaPlans._allMediaPlans = _allMediaPlans;
            await dgMediaPlans.Initialize(_campaign);
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

        public async Task DeleteData()
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


        #endregion


        private async Task CalculateMPValues()
        {
            var mediaPlansDTO = await _mediaPlanController.GetAllMediaPlansByCmpid(_campaign.cmpid);

            foreach (var mediaPlanDTO in mediaPlansDTO)
            {
                var mediaPlan = await _converter.ConvertFirstFromDTO(mediaPlanDTO);
                await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
            }

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
            // if already exist, fix conflicts
            if ((mediaPlan = await _mediaPlanController.GetMediaPlanBySchemaAndCmpId(schema.id, _campaign.cmpid)) != null)
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
            schema.name.Trim(), 1, schema.position, schema.stime, schema.etime, schema.blocktime,
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

        private async Task FillMPList()
        {
            _allMediaPlans.Clear();

            int n = (int)(endDate - startDate).TotalDays;

            var mediaPlans = await _mediaPlanController.GetAllMediaPlansByCmpid(_campaign.cmpid);
            foreach (MediaPlanDTO mediaPlan in mediaPlans)
            {
                List<MediaPlanTermDTO> mediaPlanTerms = (List<MediaPlanTermDTO>)await _mediaPlanTermController.GetAllMediaPlanTermsByXmpid(mediaPlan.xmpid);
                var mediaPlanDates = new ObservableCollection<MediaPlanTermDTO>();
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

                MediaPlanTuple mpTuple = new MediaPlanTuple(await _converter.ConvertFromDTO(mediaPlan), mediaPlanDates);
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

            _selectedChannels.Clear();
            foreach (ChannelDTO channel in lvChannels.SelectedItems)
            {
                _selectedChannels.Add(channel);
            }
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
                            var mpDTO = _converter.ConvertToDTO(mediaPlan);
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
                    bBudget.Visibility = Visibility.Collapsed;
                    bpBudget.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bBudget.Visibility = Visibility.Visible;
                    lblBudgetTarget.Content = "/" + goals.budget.ToString();
                    lblBudgetValue.DataContext = mpGoals;

                    // for selectedMP values
                    lblpBudgetValue.DataContext = SelectedMediaPlan;
                }

                if (goals.grp == 0)
                {
                    bGRP.Visibility = Visibility.Collapsed;
                    bpGRP.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bGRP.Visibility = Visibility.Visible;
                    lblGRPTarget.Content = "/" + goals.grp.ToString();
                    lblGRPValue.DataContext = mpGoals;
                    lblpGRPValue.DataContext = SelectedMediaPlan;

                }

                if (goals.ins == 0)
                {
                    bInsertations.Visibility = Visibility.Collapsed;
                    bpInsertations.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bInsertations.Visibility = Visibility.Visible;
                    lblInsertationsTarget.Content = "/" + goals.ins.ToString();
                    lblInsertationsValue.DataContext = mpGoals;
                    lblpInsertationsValue.DataContext = SelectedMediaPlan;

                }

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

            #region ContextMenu
        private async void dgMediaPlans_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // check if it's clicked on header
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;

            DataGridRow row = FindParent<DataGridRow>(dependencyObject);
            if (row != null)
            {
                // Set the DataGrid's SelectedItem property to the right-clicked item
                dgMediaPlans.Schema.SelectedItem = row.DataContext;

            }

            if (IsCellInDataGridHeader(dependencyObject))
            {
                ContextMenu menu = new ContextMenu();
                for (int i = 0; i < mediaPlanColumns; i++)
                {
                    var column = dgMediaPlans.Schema.Columns[i];

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

                dgMediaPlans.Schema.ContextMenu = menu;
            }
            else 
            {
                ContextMenu menu = new ContextMenu();
                MenuItem deleteItem = new MenuItem();
                deleteItem.Header = "Delete MediaPlan";
                deleteItem.Click += async (obj, ea) =>
                {
                    var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
                    if (mediaPlanTuple != null)
                    {
                        mediaPlanTuple.MediaPlan.active = false;
                        _allMediaPlans.Remove(mediaPlanTuple);
                        var mediaPlan = mediaPlanTuple.MediaPlan;                        
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
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
                        MediaPlanDTO mediaPlanDTO = await SchemaToMP(schema);

                        await _databaseFunctionsController.StartAMRCalculation(_campaign.cmpid, 40, 40, mediaPlanDTO.xmpid);
                        var mediaPlan = await _converter.ConvertFirstFromDTO(mediaPlanDTO);

                        var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlanDTO);
                        var mpTuple = new MediaPlanTuple(mediaPlan, new ObservableCollection<MediaPlanTermDTO>(mediaPlanTerms));
                        _allMediaPlans.Add(mpTuple);

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

                var mediaPlanTuple = dgMediaPlans.Schema.SelectedItem as MediaPlanTuple;
                if (mediaPlanTuple == null)
                {
                    return;
                }
                var mediaPlan = mediaPlanTuple.MediaPlan;

                MenuItem trimAmr = new MenuItem();
                // Check if the clicked cell is in the "AMR" columns
                if (cell.Column.Header.ToString() == "AMR 1" || cell.Column.Header.ToString() == "AMR% 1")
                {           
                    trimAmr.Header = "Trim Amr1";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR 1", "amr1trim", mediaPlan.amr1trim);
                }
                else if (cell.Column.Header.ToString() == "AMR 2" || cell.Column.Header.ToString() == "AMR% 2")
                {
                    trimAmr.Header = "Trim Amr2";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR 2", "amr2trim", mediaPlan.amr2trim);
                }
                else if (cell.Column.Header.ToString() == "AMR 3" || cell.Column.Header.ToString() == "AMR% 3")
                {
                    trimAmr.Header = "Trim Amr3";
                    trimAmr.Click += await TrimAmrAsync(mediaPlan, "Trim AMR 3", "amr3trim", mediaPlan.amr3trim);
                }
                else if (cell.Column.Header.ToString() == "AMR Sale" || cell.Column.Header.ToString() == "AMR% Sale")
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
                dgMediaPlans.Schema.ContextMenu = menu;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
                return null;

            if (parent is T)
                return parent as T;
            else
                return FindParent<T>(parent);
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

        private async Task<RoutedEventHandler> TrimAmrAsync(MediaPlan mediaPlan, string message, string attr,  int? trimValue)
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
                            mediaPlan.Amr1trim = f.newValue;
                            break;
                        case "amr2trim":
                            mediaPlan.Amr2trim = f.newValue;
                            break;
                        case "amr3trim":
                            mediaPlan.Amr3trim = f.newValue;
                            break;
                        case "amrsaletrim":
                            mediaPlan.Amrsaletrim = f.newValue;
                            break;
                        case "amrtrimall":
                            mediaPlan.Amr1trim = f.newValue;
                            mediaPlan.Amr2trim = f.newValue;
                            mediaPlan.Amr3trim = f.newValue;
                            mediaPlan.Amrsaletrim = f.newValue;
                            break;
                        default:
                            break;
                    }

                    var mpDTO = _converter.ConvertToDTO(mediaPlan);

                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mpDTO));
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

                    await _converter.CalculateAMRs(mediaPlan);

                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                }
            }
        }


        #endregion
    }
}
            
