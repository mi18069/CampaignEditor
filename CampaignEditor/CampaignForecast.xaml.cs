﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanRef;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private MediaPlanRefController _mediaPlanRefController;
        private SpotController _spotController;

        private ClientDTO _client;
        private CampaignDTO _campaign;
        private MediaPlanRefDTO _mediaPlanRef;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        // for checking if certain character can be written in spot cells
        HashSet<char> spotCodes = new HashSet<char>();

        // number of frozen columns
        int mediaPlanColumns = 23;
        public int FrozenColumnsNum
        {
            get { return (int)GetValue(FrozenColumnsNumProperty); }
            set { SetValue(FrozenColumnsNumProperty, value); }
        }

        public static readonly DependencyProperty FrozenColumnsNumProperty =
            DependencyProperty.Register(nameof(FrozenColumnsNum), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        private Dictionary<ChannelDTO, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>> _channelMPDict =
            new Dictionary<ChannelDTO, List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>>();
        private ObservableCollection<Tuple<MediaPlanDTO, ObservableCollection<MediaPlanTermDTO>>> _showMP 
            = new ObservableCollection<Tuple<MediaPlanDTO, ObservableCollection<MediaPlanTermDTO>>>();

        public CampaignForecast(ISchemaRepository schemaRepository,
            IChannelRepository channelRepository, 
            IChannelCmpRepository channelCmpRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            IMediaPlanRefRepository mediaPlanRefRepository,
            ISpotRepository spotRepository)
        {
            this.DataContext = this;
            this.FrozenColumnsNum = mediaPlanColumns;

            _schemaController = new SchemaController(schemaRepository);
            _channelController = new ChannelController(channelRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _spotController = new SpotController(spotRepository);



            InitializeComponent();
        }


        #region Initialization
        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            int latestVersion = await _mediaPlanRefController.GetLatestRefVersionById(_campaign.cmpid);
            if ( latestVersion == -1)
            {
                DateTime now = DateTime.Now;
                dpFrom.SelectedDate = now;
                dpTo.SelectedDate = now;

                gridForecast.Visibility = Visibility.Collapsed;
                gridLoading.Visibility = Visibility.Collapsed;
                gridInit.Visibility = Visibility.Visible;
            }
            else
            {
                gridForecast.Visibility = Visibility.Collapsed;
                gridLoading.Visibility = Visibility.Visible;
                gridInit.Visibility = Visibility.Collapsed;

                // Filling lvChannels and dictionary
                lvChannels.Items.Clear();
                _channelMPDict.Clear();

                var channelIds = await _mediaPlanController.GetAllChannelsByCmpidAndVersion(_campaign.cmpid, latestVersion);
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
                    List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>> emptyList = new List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>();
                    _channelMPDict.Add(channel, emptyList);
                }

                int n = (int)(endDate - startDate).TotalDays;


                foreach (ChannelDTO channel in _channelMPDict.Keys)
                {
                    var mediaPlans = await _mediaPlanController.GetAllChannelMediaPlansByVersion(channel.chid, latestVersion);
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

                InitializeDateColumns();
                dgSchema.ItemsSource = _showMP;

                gridForecast.Visibility = Visibility.Visible;
                gridLoading.Visibility = Visibility.Collapsed;
                gridInit.Visibility = Visibility.Collapsed;
            }

            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            for (int i = 0; i < spots.Count(); i++)
            {
                spotCodes.Add((char)('A' + i));
            }

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

                var schemas = await _schemaController.GetAllChannelSchemasWithinDateAndTime(
                    channel.chid, DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpsdate)), 
                    DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpedate)),
                    _campaign.cmpstime, _campaign.cmpetime);

                var mediaPlans = new List<Tuple<MediaPlanDTO, List<MediaPlanTermDTO>>>();
                foreach (var schema in schemas)
                {
                    MediaPlanDTO mediaPlan = await SchemaToMP(schema);
                    var mediaPlanTerms = await MediaPlanToMPTerm(mediaPlan); 
                    mediaPlans.Add(Tuple.Create(mediaPlan, mediaPlanTerms));
                }
                _channelMPDict.Add(channel, mediaPlans);

            }

            InitializeDateColumns();
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
                    schema.created, schema.modified, 0, 100, 0, 100, 0, 100, 0, 100, 0, 0, 0, 0, 1, 1, 0, true);

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

        // When we initialize forecast, we need to do set dates for search
        private async void Init_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if ((DateTime)dpFrom.SelectedDate! < (DateTime)dpTo.SelectedDate!)
            {
                gridInit.Visibility = Visibility.Hidden;
                gridForecast.Visibility = Visibility.Hidden;
                gridLoading.Visibility = Visibility.Visible;

                _mediaPlanRef = await _mediaPlanRefController.CreateMediaPlanRef(
                    new MediaPlanRefDTO(_campaign.cmpid, DateOnly.FromDateTime(dpFrom.SelectedDate.Value),
                    DateOnly.FromDateTime(dpTo.SelectedDate.Value), 1));

                await InitializeData();

                gridLoading.Visibility = Visibility.Hidden;
                gridInit.Visibility = Visibility.Hidden;
                gridForecast.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Invalid dates");
            }
        }


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

                    _showMP.Add(Tuple.Create(mediaPlanDTO, mediaPlanTerms));
                }
            }

            if (deselectedItems.Count>0)
            {
                ChannelDTO deselectedItem = deselectedItems[0]! as ChannelDTO;

                for (int i=0; i < _showMP.Count(); i++)
                {
                    var tuple = _showMP[i];
                    foreach (var channelTuple in _channelMPDict[deselectedItem])
                        if (channelTuple.Item1 == tuple.Item1)
                        {
                            _showMP.Remove(tuple);
                            i--;
                        }
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

                var binding = new Binding($"Item2[{dates.IndexOf(date)}].spotcode");
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
                trigger.Binding = new Binding($"Item2[{dates.IndexOf(date)}]");
                trigger.Value = null;
                trigger.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGoldenrodYellow)); // Set background to yellow if value is null
                trigger.Setters.Add(new Setter(FocusableProperty, false));
                column.CellStyle.Triggers.Add(trigger);
                column.CellStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Green)); // Set background to green if value is not null
                column.CanUserSort = false;
                column.CanUserResize = false;
                column.CanUserReorder = false;

                
                // Add the column to the DataGrid
                dgSchema.Columns.Add(column);
            }


        }

        private async void OnCellPreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            DataGridCell cell = sender as DataGridCell;
            TextBox textBox = cell.Content as TextBox;

            char? spotcodeNull = e.Text.Trim()[0];

            e.Handled = true;
            if (spotcodeNull.HasValue)
            {
                char spotcode = Char.ToUpper(spotcodeNull.Value);
                if (spotCodes.Contains(spotcode))
                {
                    //if (!(textBox == null) && textBox.Text.Trim().Length > 0)
                    cell.Content = spotcode;

                    var mpTerm = GetSelectedMediaPlanTermDTO(cell);
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                        new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, spotcode.ToString()));

                    mpTerm.spotcode = spotcode.ToString().Trim();
                    cell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                }
            }
            

        }

        private async void OnCellPreviewKeyDown(object sender, KeyEventArgs e)
        {

            DataGridCell cell = sender as DataGridCell;
            TextBox text = cell.Content as TextBox;

            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                e.Handled = true;
                cell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            if ((e.Key == Key.Delete || e.Key == Key.Back) && text != null)
            {
                var mpTerm = GetSelectedMediaPlanTermDTO(cell);

                await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, null));
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
            var tuple = row.Item as Tuple<MediaPlanDTO, ObservableCollection<MediaPlanTermDTO>>;
            if (tuple == null)
            {
                return null; // row is not bound to a tuple
            }
            ObservableCollection<MediaPlanTermDTO> mpTerms = tuple.Item2;

            // Get the MediaPlanTerm for the selected cell
            int rowIndex = row.GetIndex();
            MediaPlanTermDTO mpTermDTO = mpTerms[columnIndex - FrozenColumnsNum]; // we have n freezed columns

            return mpTermDTO;
        }

        #endregion

        #region MediaPlan columns

            #region ContextMenu
        private void dgSchema_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
                dgSchema.ContextMenu = null;
            }
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
            var tuple = dgSchema.SelectedItems[0] as Tuple<MediaPlanDTO, ObservableCollection<MediaPlanTermDTO>>;
            var textBox = sender as TextBox;

            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            string propertyName = bindingExpr?.ResolvedSourcePropertyName;

            if (tuple != null)
            {
                var mediaPlan = tuple.Item1;
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
        
        #endregion

        }
}
