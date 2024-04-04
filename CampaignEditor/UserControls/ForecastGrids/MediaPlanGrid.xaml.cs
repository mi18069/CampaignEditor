using CampaignEditor.Controllers;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Database.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using System.ComponentModel;
using System.Collections.Specialized;
using Database.DTOs.ChannelDTO;
using OfficeOpenXml;
using System.Windows.Threading;
using CampaignEditor.Converters;
using Brushes = System.Windows.Media.Brushes;
using Border = System.Windows.Controls.Border;
using Style = System.Windows.Style;
using MenuItem = System.Windows.Controls.MenuItem;
using Action = System.Action;
using CampaignEditor.Helpers;
using OfficeOpenXml.Style;
using Database.DTOs.SpotDTO;
using Database.DTOs.DayPartDTO;
using Database.DTOs.ClientProgCoefDTO;

namespace CampaignEditor.UserControls
{
    public partial class MediaPlanGrid : UserControl
    {

        public IAbstractFactory<AddSchema> _factoryAddSchema { get; set; }
        public IAbstractFactory<AMRTrim> _factoryAmrTrim { get; set; }


        public SchemaController _schemaController { get; set; }
        public MediaPlanController _mediaPlanController { get; set; }
        public MediaPlanTermController _mediaPlanTermController { get; set; }
        public MediaPlanConverter _mpConverter { get; set; }
        public MediaPlanTermConverter _mpTermConverter { get; set; }
        public ClientProgCoefController _clientProgCoefController { get; set; }
        public DatabaseFunctionsController _databaseFunctionsController { get; set; }
        private Dictionary<int, string> chidChannelDictionary = new Dictionary<int, string>();


        private ObservableCollection<TotalItem> totals = new ObservableCollection<TotalItem>();

        // for checking if certain character can be written in spot cells
        HashSet<char> spotCodes = new HashSet<char>();
        // for mouse click event
        string lastSpotCell = "";

        // number of frozen columns
        public int mediaPlanColumns = 30;

        double dataColumnWidth = 51;


        // for saving old values of MediaPlan when I want to change it
        private MediaPlan _mediaPlanOldValues;
        private MediaPlan _mediaPlanToUpdate;
        bool isEditingEnded = false;

        public bool filterByIns = false;
        public bool filterByDP = false;
        public bool filterByDays = false;

        public void FilterByInsChanged()
        {
            if (myDataView != null)
            {
                myDataView.Refresh();
            }
        }

        private int frozenColumnsNum
        {
            get { return (int)GetValue(frozenColumnsNumProperty); }
            set { SetValue(frozenColumnsNumProperty, value); }
        }

        public static readonly DependencyProperty frozenColumnsNumProperty =
            DependencyProperty.Register(nameof(frozenColumnsNum), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableRangeCollection<MediaPlanTuple>();

        public ObservableCollection<ChannelDTO> _selectedChannels = new ObservableCollection<ChannelDTO>();
        public List<DayOfWeek> _filteredDays = new List<DayOfWeek>();
        public List<DayPartDTO> _filteredDayParts = new List<DayPartDTO>();
        CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        private bool _canUserEdit = false;
        public bool CanUserEdit
        {
            get { return _canUserEdit; }
            set
            {
                _canUserEdit = value;
                dgMediaPlans.IsManipulationEnabled = CanUserEdit;
            }
        }
        public DataGrid Grid
        {
            get { return dgMediaPlans; }
        }

        public MediaPlanGrid()
        {

            _canUserEdit = false;

            if (MainWindow.user.usrlevel == 2)
            {
                _canUserEdit = false;
            }

            InitializeComponent();

            dgMediaPlans.IsManipulationEnabled = true;

            // For combobox in Position column
            var positionColumn = dgMediaPlans.Columns.FirstOrDefault(c => c.Header.ToString() == "Position") as DataGridComboBoxColumn;
            if (positionColumn != null)
            {
                // Add the possible values
                positionColumn.ItemsSource = new[] { "INS", "BET" };
            }

        }


        public DataGrid Schema
        {
            get { return dgMediaPlans; }
        }
        ICollectionView myDataView;
        public void Initialize(CampaignDTO campaign, IEnumerable<ChannelDTO> channels, IEnumerable<SpotDTO> spots)
        {
            tcChannel.Binding = new Binding()
            {
                Path = new PropertyPath("MediaPlan.chid"),
                Converter = new ChidToChannelConverter(),
                ConverterParameter = chidChannelDictionary
            };

            chidChannelDictionary.Clear();
            foreach (ChannelDTO channel in channels)
            {
                chidChannelDictionary.Add(channel.chid, channel.chname.Trim());
            }

            this.frozenColumnsNum = mediaPlanColumns;
            this.Schema.FrozenColumnCount = frozenColumnsNum;

            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            DeleteDateColumns(frozenColumnsNum);
            InitializeDateColumns();

            InitializeSpots(spots);


            myDataView = CollectionViewSource.GetDefaultView(_allMediaPlans);
            dgMediaPlans.ItemsSource = myDataView;

            myDataView.SortDescriptions.Add(new SortDescription("MediaPlan.chid", ListSortDirection.Ascending));
            myDataView.SortDescriptions.Add(new SortDescription("MediaPlan.stime", ListSortDirection.Ascending));
            myDataView.Filter = d =>
            {
                var mediaPlan = ((MediaPlanTuple)d).MediaPlan;
                var mpTerms = ((MediaPlanTuple)d).Terms;

                var result = _selectedChannels.Any(c => c.chid == mediaPlan.chid);

                if (result && filterByDays)
                {
                    result = mpTerms.Any(t => t != null && _filteredDays.Contains(t.Date.DayOfWeek));
                }

                if (result && filterByIns)
                {
                    result = mediaPlan.Insertations > 0;
                }

                if (result && filterByDP)
                {
                    if (mediaPlan.DayPart == null)
                        result = false;
                    else
                        result = _filteredDayParts.Select(dp => dp.dpid).Contains(mediaPlan.DayPart.dpid);
                }

                return result;
            };

            _allMediaPlans.CollectionChanged += OnCollectionChanged;
            _selectedChannels.CollectionChanged += OnCollectionChanged;

            InitializeTotalGrid();


        }

        private IEnumerable<DateTime> GetCampaignDates(CampaignDTO campaign)
        {
            DateTime startDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
            DateTime endDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

            // Get a list of all dates between start and end date, inclusive
            var dates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
            .Select(offset => startDate.AddDays(offset));

            return dates;
        }

        private void InitializeTotalGrid()
        {
            TimeSpan duration = endDate - startDate;
            int numberOfDays = duration.Days + 1;

            totals.Clear();

            for (int i = 0; i < numberOfDays; i++)
            {
                var ti = new TotalItem();
                ti.Total = 0;
                totals.Add(ti);
            }

            icTotals.ItemsSource = totals;

            RefreshTotalGridValues();
        }

        private void SetTotalsWidth()
        {
            double width = 0;

            TimeSpan duration = endDate - startDate;
            int numberOfDays = duration.Days + 1;
            int numOfColumns = dgMediaPlans.Columns.Count() - numberOfDays;
            for (int i = 0; i < frozenColumnsNum; i++)
            {
                var column = dgMediaPlans.Columns[i] as DataGridColumn;

                if (column.Visibility == Visibility.Visible)
                {
                    width += column.ActualWidth;
                }

            }

            lblTotal.Width = width;

            double svWidth = dgMediaPlans.ActualWidth - lblTotal.Width;
            svTotals.Width = svWidth < 0 ? 0 : svWidth;
        }

        private void dgMediaPlans_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => SetTotalsWidth()), DispatcherPriority.ContextIdle);
        }

        private void DataGridColumnHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => SetTotalsWidth()), DispatcherPriority.ContextIdle);

        }

        private void RefreshTotalGridValues()
        {
            if (dgMediaPlans.Items.Count == 0)
            {
                bTotal.Visibility = Visibility.Hidden;
            }
            else
            {
                bTotal.Visibility = Visibility.Visible;
                SetTotalsWidth();
            }

            TimeSpan duration = endDate - startDate;
            int numberOfDays = duration.Days + 1;

            for (int dateIndex = 0; dateIndex < numberOfDays; dateIndex++)
            {
                var count = CountInsByDateIndex(dateIndex);
                totals[dateIndex].Total = count;
            }
        }

        private void RefreshTotalGridValues(int dateIndex)
        {
            var count = CountInsByDateIndex(dateIndex);
            totals[dateIndex].Total = count;
        }

        private int CountInsByDateIndex(int dateIndex)
        {
            int count = 0;

            foreach (MediaPlanTuple mpTuple in dgMediaPlans.Items)
            {
                try
                {
                    if (mpTuple.Terms.Count() > dateIndex &&
                        mpTuple.Terms[dateIndex] != null &&
                        mpTuple.Terms[dateIndex].Spotcode != null)
                    {
                        count += mpTuple.Terms[dateIndex].Spotcode.Trim().Count();

                    }
                }
                catch
                {

                }
            }

            return count;
        }

        public void InitializeSpots(IEnumerable<SpotDTO> spots)
        {
            spotCodes.Clear();
            for (int i = 0; i < spots.Count(); i++)
            {
                spotCodes.Add((char)('A' + i));
            }
        }

        private void DeleteDateColumns(int frozenColumnsNum)
        {
            // Get the columns after the frozenColumnsNum-th column
            var columnsToRemove = dgMediaPlans.Columns.Cast<DataGridColumn>().Skip(frozenColumnsNum).ToList();

            // Remove the columns from the DataGrid
            foreach (var column in columnsToRemove)
            {
                dgMediaPlans.Columns.Remove(column);
            }
        }

        // Method to handle the CollectionChanged event
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            myDataView.Refresh();
            // Use Dispatcher to run RefreshTotalGridValues after layout update
            Dispatcher.BeginInvoke(new Action(() => RefreshTotalGridValues()), DispatcherPriority.ContextIdle);
            OnVisibleTuplesChanged();
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise the SelectionChanged event
            SelectionChanged?.Invoke(sender, e);
        }

        public List<MediaPlanTuple> GetVisibleMediaPlanTuples()
        {
            List<MediaPlanTuple> visibleItems = new List<MediaPlanTuple>();

            // Iterate through the sorted and filtered collection view to get visible items
            foreach (MediaPlanTuple mediaPlanTuple in myDataView)
            {
                visibleItems.Add(mediaPlanTuple);
            }

            return visibleItems;
        }

        #region DgMediaPlans

        #region Date Columns
        private void InitializeDateColumns()
        {
            var dates = GetCampaignDates(_campaign).ToList();

            // Create a column for each date
            foreach (DateTime date in dates)
            {
                // Create a new DataGridTextColumn
                DataGridTextColumn column = new DataGridTextColumn();

                // Set the column header to the date
                column.Header = date.ToString("dd.MM.yy");

                var disabledCellColor = Brushes.LightGoldenrodYellow;
                var enabledCellColor = Brushes.LightGreen;
                if (DateTime.TryParse(column.Header.ToString(), out DateTime columnHeaderDate))
                {
                    if (columnHeaderDate.Date <= DateTime.Today.Date)
                    {
                        // Apply the dimmed column style
                        disabledCellColor = Brushes.Goldenrod;
                        enabledCellColor = Brushes.Green;
                    }
                }

                var binding = new Binding($"Terms[{dates.IndexOf(date)}].Spotcode");
                //binding.ValidationRules.Add(new CharLengthValidationRule(1)); // add validation rule to restrict input to a single character
                column.Binding = binding;
                column.Width = dataColumnWidth;

                var cellStyle = new Style(typeof(DataGridCell));

                // Adding setters to cells
                var textInputEventSetter = new EventSetter(PreviewTextInputEvent, new TextCompositionEventHandler(OnCellPreviewTextInput));
                var keyDownEventSetter = new EventSetter(PreviewKeyDownEvent, new KeyEventHandler(OnCellPreviewKeyDown));
                var mouseLeftButtonDownEventSetter = new EventSetter(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));

                cellStyle.Setters.Add(textInputEventSetter);
                cellStyle.Setters.Add(keyDownEventSetter);
                cellStyle.Setters.Add(mouseLeftButtonDownEventSetter);
                //cellStyle.Setters.Add(new Setter(DataGridCell.IsHitTestVisibleProperty, CanUserEdit.Value));
                column.CellStyle = cellStyle;

                var trigger = new DataTrigger();
                trigger.Binding = new Binding($"Terms[{dates.IndexOf(date)}]");
                trigger.Value = null;
                trigger.Setters.Add(new Setter(BackgroundProperty, disabledCellColor)); // Set background to yellow if value is null

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
                    trigger.Setters.Add(new Setter(System.Windows.Controls.Border.BorderBrushProperty, Brushes.OrangeRed));

                    column.CellStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1, 1, 3, 1)));
                    column.CellStyle.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.OrangeRed));
                }

                column.CellStyle.Triggers.Add(trigger);

                column.CellStyle.Setters.Add(new Setter(BackgroundProperty, enabledCellColor)); // Set background to green if value is not null
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
                dgMediaPlans.Columns.Add(column);
            }
        }

        #region Terms manipulation

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell == null)
                return;
            //SimulateTextInput(lastSpotCell);
            OnCellPreviewTextInput(sender, new TextCompositionEventArgs
                (Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, null, lastSpotCell.ToString())));
        }

        private async void OnCellPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!_canUserEdit)
                return;

            DataGridCell cell = sender as DataGridCell;

            // if cell is not binded to mediaPlanTerm, or if user don't have privileges disable editing
            var mediaPlanTuple = (MediaPlanTuple)cell.DataContext;
            var mpTerms = mediaPlanTuple.Terms;
            var index = cell.Column.DisplayIndex - mediaPlanColumns;
            MediaPlanTerm? mpTerm;
            if ((mpTerm = mpTerms[index]) == null)
                return;

            DateTime currentDate = DateTime.Now;
            if (mpTerm.Date <= DateOnly.FromDateTime(currentDate))
            {
                return;
            }

            char? spotcodeNull;
            try
            {
                spotcodeNull = e.Text == null ? null : e.Text[0];
            }
            catch
            {
                spotcodeNull = null;
            }
            if (!spotcodeNull.HasValue)
                return;

            char spotcode = ConvertSpotcodeToChar(spotcodeNull.Value);
            
            if (!spotCodes.Contains(spotcode))
            {
                e.Handled = true;
                return;
            }

            lastSpotCell = spotcode.ToString();

            await AddSpotcodeToMpTerm(mpTerm, spotcode);
            await UpdateMediaPlan(mediaPlanTuple);          
            TermUpdated(mpTerm, spotcode);

                       
        }


        private char ConvertSpotcodeToChar(char spotcodeChar)
        {
            if (Char.IsDigit(spotcodeChar))
            {
                int digitCharValue = spotcodeChar - '1';
                spotcodeChar = (char)('A' + digitCharValue);
            }
            return Char.ToUpper(spotcodeChar);
        }

        private async Task AddSpotcodeToMpTerm(MediaPlanTerm mpTerm, char spotcode)
        {
            if (mpTerm.Spotcode != null && mpTerm.Spotcode.Length >= 3)
                return;

            string newSpotcode = (mpTerm.Spotcode == null ? "" : mpTerm.Spotcode) + spotcode.ToString();

            await _mediaPlanTermController.UpdateMediaPlanTerm(
                new UpdateMediaPlanTermDTO(mpTerm.Xmptermid, mpTerm.Xmpid, mpTerm.Date, newSpotcode));

            mpTerm.Spotcode = newSpotcode;           

            int numberOfDays = mpTerm.Date.DayNumber - DateOnly.FromDateTime(startDate).DayNumber;
            totals[numberOfDays].Total += 1;

            // Refactor this
            //TermUpdated(mpTerm, spotcode);
        }       

        // For deleting spotcode from mediaPlanTerm
        private async void OnCellPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Only for deleting spotcode
            if (!(e.Key == Key.Delete || e.Key == Key.Back))
                return;
            
            DataGridCell cell = sender as DataGridCell;           

            // if cell is not binded to mediaPlanTerm, or if user don't have privileges disable editing
            var mediaPlanTuple = (MediaPlanTuple)cell.DataContext;
            var mpTerms = mediaPlanTuple.Terms;
            var index = cell.Column.DisplayIndex - mediaPlanColumns;
            MediaPlanTerm? mpTerm;
            if ((mpTerm = mpTerms[index]) == null)
                return;

            DateTime currentDate = DateTime.Now;
            if (!_canUserEdit || mpTerm.Date <= DateOnly.FromDateTime(currentDate))
            {
                return;
            }

            char? spotcode = await DeleteSpotcodeFromMpTerm(mpTerm);
            if (spotcode.HasValue)
            {
                await UpdateMediaPlan(mediaPlanTuple);
                TermUpdated(mpTerm, spotcode);
            }     
        }

        private async Task<char?> DeleteSpotcodeFromMpTerm(MediaPlanTerm mpTerm)
        {
            if (mpTerm.Spotcode != null && mpTerm.Spotcode.Length > 0)
            {
                // If old spotcode have only one character, then pass null as parameter, otherwise
                // pass substring without last spotcode
                string? newSpotcode = null;
                if (mpTerm.Spotcode.Length > 0)
                    newSpotcode = mpTerm.Spotcode.Substring(0, mpTerm.Spotcode.Length - 1);                

                char? deletedSpotcode = null;
                try
                {
                    deletedSpotcode = mpTerm.Spotcode[mpTerm.Spotcode.Length - 1];
                }
                catch
                {
                    return null;
                }

                int numberOfDays = mpTerm.Date.DayNumber - DateOnly.FromDateTime(startDate).DayNumber;


                await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.Xmptermid, mpTerm.Xmpid, mpTerm.Date, newSpotcode));

                mpTerm.Spotcode = newSpotcode;

                totals[numberOfDays].Total -= 1;

                return deletedSpotcode;
                // Refactor this
                //TermUpdated(mpTerm, deletedSpotcode);
            }

            return null;

        }

        private async Task UpdateMediaPlan(MediaPlanTuple mediaPlanTuple)
        {
            var mediaPlan = mediaPlanTuple.MediaPlan;
            var termsDTO = _mpTermConverter.ConvertToEnumerableDTO(mediaPlanTuple.Terms);
            _mpConverter.ComputeExtraProperties(mediaPlan, termsDTO, true);
            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(mediaPlan)));
        }

        #endregion


        public delegate void UpdatedTermEventHandler(object sender, UpdatedTermEventArgs e);
        public event UpdatedTermEventHandler UpdatedTerm;
        private void TermUpdated(MediaPlanTerm term, char? spotcode)
        {
            var updatedTerm = new UpdatedTermEventArgs(term, spotcode);
            UpdatedTerm?.Invoke(this, updatedTerm);
        }

        private void dgMediaPlans_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_canUserEdit)
            {
                e.Handled = true;
                return;
            }
        }       

        #endregion

        #region Editing MediaPlan

        private void dgMediaPlans_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

            isEditingEnded = false;

            // Access the DataContext (your MediaPlan object)
            var mediaPlanTuple = e.Row.DataContext as MediaPlanTuple;

            // Get values before editing
            if (mediaPlanTuple != null)
            {
                _mediaPlanToUpdate = mediaPlanTuple.MediaPlan;
                _mediaPlanOldValues = _mpConverter.CopyMP(_mediaPlanToUpdate);

            }

        }

        private void dgMediaPlans_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isEditingEnded = true;
        }

        private async void dgMediaPlans_CurrentCellChanged(object sender, EventArgs e)
        {
            if (isEditingEnded)
            {
                bool progCoefEdited = false;
                // Compare with the original value and revert if needed
                if (_mediaPlanToUpdate != null && !_mpConverter.SameMPValues(_mediaPlanToUpdate, _mediaPlanOldValues))
                {
                    // If coefs are changed, we need to recalculate price
                    double eps = 0.0001;
                    if (Math.Abs(_mediaPlanToUpdate.Progcoef - _mediaPlanOldValues.Progcoef) > eps)
                    {
                        progCoefEdited = true;
                        _mpConverter.CoefsChanged(_mediaPlanToUpdate);
                    }
                    else if (Math.Abs(_mediaPlanToUpdate.Dpcoef - _mediaPlanOldValues.Dpcoef) > eps ||
                        Math.Abs(_mediaPlanToUpdate.Seccoef - _mediaPlanOldValues.Seccoef) > eps ||
                        Math.Abs(_mediaPlanToUpdate.Seascoef - _mediaPlanOldValues.Seascoef) > eps)
                    {
                        _mpConverter.CoefsChanged(_mediaPlanToUpdate);
                    }
                    else if (_mediaPlanToUpdate.Stime != _mediaPlanOldValues.Stime ||
                            _mediaPlanToUpdate.Etime != _mediaPlanOldValues.Etime ||
                            _mediaPlanToUpdate.Blocktime != _mediaPlanOldValues.Blocktime)
                    {
                        try
                        {
                            _mediaPlanToUpdate.Stime = TimeFormat.ReturnGoodTimeFormat(_mediaPlanToUpdate.Stime.Trim());
                            _mediaPlanToUpdate.Etime = TimeFormat.ReturnGoodTimeFormat(_mediaPlanToUpdate.Etime.Trim());
                            if (_mediaPlanToUpdate.Blocktime != null)
                            {
                                if (_mediaPlanToUpdate.Blocktime != _mediaPlanOldValues.Blocktime)
                                {
                                    var timeFormat = TimeFormat.ReturnGoodTimeFormat(_mediaPlanToUpdate.Blocktime.Trim());
                                    _mediaPlanToUpdate.Blocktime = timeFormat == "" ? null : timeFormat;
                                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_mpConverter.ConvertToDTO(_mediaPlanToUpdate)));

                                    OnUpdatedMediaPlan(_mediaPlanToUpdate);
                                }

                            }
                        }
                        catch
                        {
                            MessageBox.Show("Wrong Time Format", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                            isEditingEnded = false;
                            _mediaPlanToUpdate.Stime = _mediaPlanOldValues.Stime;
                            _mediaPlanToUpdate.Etime = _mediaPlanOldValues.Etime;
                            _mediaPlanToUpdate.Blocktime = _mediaPlanOldValues.Blocktime;
                            return;
                        }
                    }
                    try
                    {
                        var mpDTO = _mpConverter.ConvertToDTO(_mediaPlanToUpdate);
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mpDTO));
                        _mpConverter.CopyValues(_mediaPlanToUpdate, _mediaPlanToUpdate);

                        if (progCoefEdited)
                        {
                            var clprogcoef = await _clientProgCoefController.GetClientProgCoef(_campaign.clid, mpDTO.schid);
                            if (clprogcoef == null)
                            {
                                var newClientProgCoef = new ClientProgCoefDTO(_campaign.clid, mpDTO.schid, mpDTO.progcoef);
                                try
                                {
                                    await _clientProgCoefController.CreateClientProgCoef(newClientProgCoef);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Unable to change prog coef! \n" + ex.Message, "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                clprogcoef.progcoef = mpDTO.progcoef;
                                try
                                {
                                    await _clientProgCoefController.UpdateClientProgCoef(clprogcoef);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Unable to change prog coef! \n" + ex.Message, "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                            }
                        }
                    }
                    catch
                    {
                        _mpConverter.CopyValues(_mediaPlanToUpdate, _mediaPlanOldValues);
                    }
                }
                isEditingEnded = false;
            }

        }

        #endregion

        #endregion

        #region  Context Menu
        private async void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // check if it's clicked on header
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;

            DataGridRow row = FindParent<DataGridRow>(dependencyObject);
            if (row != null)
            {
                // Set the DataGrid's SelectedItem property to the right-clicked item
                Schema.SelectedItem = row.DataContext;

            }

            // For setting visibility of columns
            if (IsCellInDataGridHeader(dependencyObject))
            {
                ContextMenu menu = new ContextMenu();
                for (int i = 0; i < mediaPlanColumns; i++)
                {
                    var column = Schema.Columns[i];

                    MenuItem item = new MenuItem();
                    item.Header = column.Header.ToString().Trim();
                    item.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                    item.Click += (obj, ea) =>
                    {
                        column.Visibility = item.IsChecked ? Visibility.Hidden : Visibility.Visible;
                        item.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                        Dispatcher.BeginInvoke(new Action(() => SetTotalsWidth()), DispatcherPriority.ContextIdle);
                    };

                    item.StaysOpenOnClick = true;

                    menu.Items.Add(item);
                }
                menu.StaysOpen = true;
                Schema.ContextMenu = menu;
            }
            else
            {
                if (!CanUserEdit)
                    return;

                ContextMenu menu = new ContextMenu();
                MenuItem deleteItem = new MenuItem();
                deleteItem.Header = "Delete Program";
                deleteItem.Click += async (obj, ea) =>
                {
                    OnDeleteMediaPlanClicked();
                };
                menu.Items.Add(deleteItem);

                MenuItem addMediaPlanItem = new MenuItem();
                addMediaPlanItem.Header = "Add Program";
                addMediaPlanItem.Click += async (obj, ea) =>
                {
                    OnAddMediaPlanClicked();
                };
                menu.Items.Add(addMediaPlanItem);

                MenuItem importMediaPlanItem = new MenuItem();
                importMediaPlanItem.Header = "Import Program from Schema";
                importMediaPlanItem.Click += async (obj, ea) =>
                {
                    OnImportMediaPlanClicked();
                };
                menu.Items.Add(importMediaPlanItem);

                // Traverse the visual tree to get the clicked DataGridCell object
                while ((dependencyObject != null) && !(dependencyObject is DataGridCell))
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }

                if (dependencyObject != null)
                {
                    DataGridCell cell = dependencyObject as DataGridCell;

                    var mediaPlanTuple = Schema.SelectedItem as MediaPlanTuple;
                    if (mediaPlanTuple != null)
                    {
                        var mediaPlan = mediaPlanTuple.MediaPlan;

                        MenuItem updateMediaPlanItem = new MenuItem();
                        updateMediaPlanItem.Header = "Update Program";
                        updateMediaPlanItem.Click += async (obj, ea) =>
                        {
                            OnUpdateMediaPlanClicked(mediaPlanTuple);
                        };
                        menu.Items.Add(updateMediaPlanItem);


                        MenuItem trimAmr = new MenuItem();
                        trimAmr.Header = "Trim Program Amrs";
                        trimAmr.Click += await TrimAmrs(mediaPlan);
                        menu.Items.Add(trimAmr);


                        MenuItem recalculateMediaPlan = new MenuItem();
                        recalculateMediaPlan.Header = "Recalculate program values";
                        recalculateMediaPlan.Click += async (obj, ea) =>
                        {
                            OnRecalculateMediaPlan();
                        };
                        menu.Items.Add(recalculateMediaPlan);

                        MenuItem clearMpTerms = new MenuItem();
                        clearMpTerms.Header = "Clear program spots";
                        clearMpTerms.Click += async (obj, ea) =>
                        {
                            OnClearMpTerms();
                        };
                        menu.Items.Add(clearMpTerms);

                    }

                }


                Schema.ContextMenu = menu;
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
        
        private async Task<RoutedEventHandler> TrimAmrs(MediaPlan mediaPlan)
        {
            async void handler(object sender, RoutedEventArgs e)
            {
                var f = _factoryAmrTrim.Create();
                f.Initialize("Trim Amrs for:\n" + mediaPlan.name, mediaPlan.Amr1trim);
                f.ShowDialog();
                if (f.changed)
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


        public event EventHandler AddMediaPlanClicked;

        private void OnAddMediaPlanClicked()
        {
            AddMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ImportMediaPlanClicked;

        private void OnImportMediaPlanClicked()
        {
            ImportMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<UpdateMediaPlanTupleEventArgs> UpdateMediaPlanClicked;

        private void OnUpdateMediaPlanClicked(MediaPlanTuple mediaPlanTuple)
        {
            UpdateMediaPlanClicked?.Invoke(this, new UpdateMediaPlanTupleEventArgs(mediaPlanTuple));
        }

        public event EventHandler<UpdateMediaPlanEventArgs> UpdatedMediaPlan;

        private void OnUpdatedMediaPlan(MediaPlan mediaPlan)
        {
            UpdatedMediaPlan?.Invoke(this, new UpdateMediaPlanEventArgs(mediaPlan));
        }

        public event EventHandler DeleteMediaPlanClicked;

        private void OnDeleteMediaPlanClicked()
        {
            DeleteMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RecalculateMediaPlan;

        private void OnRecalculateMediaPlan()
        {
            RecalculateMediaPlan?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ClearMpTerms;
        private void OnClearMpTerms()
        {
            ClearMpTerms?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler VisibleTuplesChanged;
        private void OnVisibleTuplesChanged()
        {
            VisibleTuplesChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Print

        Dictionary<string, System.Drawing.Color> colors = new Dictionary<string, System.Drawing.Color>();
        private void FillColorsDictionary()
        {
            var headerColor = System.Drawing.ColorTranslator.FromHtml("#DAA520");
            colors.Add("header", headerColor);

            var term = System.Drawing.ColorTranslator.FromHtml(Brushes.LightGreen.ToString());
            colors.Add("term", term);

            var nullTerm = System.Drawing.ColorTranslator.FromHtml(Brushes.LightGoldenrodYellow.ToString());
            colors.Add("null", nullTerm);

            var weekday = System.Drawing.Color.OrangeRed;
            colors.Add("weekday", weekday);

            var day = System.Drawing.Color.Black;
            colors.Add("day", day);
        }

        public void PopulateWorksheet(bool[] visibleColumns, ExcelWorksheet worksheet, int rowOff = 1, int colOff = 1)
        {
            var selectedChannels = _selectedChannels;
            var selectedChannelsChids = selectedChannels.Select(ch => ch.chid);
            //var mpTuples = _allMediaPlans.Where(mpTuple => selectedChannelsChids.Contains(mpTuple.MediaPlan.chid));
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(dgMediaPlans.ItemsSource);

            IEnumerable<MediaPlanTuple> mpTuples = collectionView.OfType<MediaPlanTuple>();
            mpTuples = mpTuples.OrderBy(mpt => mpt.MediaPlan.chid);
            if (colors.Count == 0)
            {
                FillColorsDictionary();
            }

            AddColumnHeaders(visibleColumns, worksheet, rowOff, colOff);
            int visibleColumnsNum = visibleColumns.Count(v => v);
            AddDateHeaders(worksheet, rowOff, colOff + visibleColumnsNum);

            int rowOffset = 1;
            foreach (var mpTuple in mpTuples)
            {
                AddMpTuple(mpTuple, visibleColumns, worksheet, rowOff + rowOffset, colOff);
                rowOffset += 1;
            }

            AddTotals(worksheet, rowOff + rowOffset, colOff + visibleColumnsNum);

        }

        private void AddTotals(ExcelWorksheet worksheet, int rowOff, int colOff)
        {

            int colOffset = 0;
            for (int i = 0; i < totals.Count; i++)
            {
                var excelCell = worksheet.Cells[rowOff, colOff + colOffset];
                excelCell.Value = totals[i].Total.ToString();
                SetBackgroundColor(excelCell, colors["header"]);
                colOffset += 1;
            }
        }       

        private void AddColumnHeaders(bool[] visibleColumns, ExcelWorksheet worksheet, int rowOff, int colOff)
        {
            string[] columnHeaders = new string[]{"Channel", "Program", "Day Part", "Position", "Start time",
            "End time", "Block time", "Type", "Special", "Amr1", "Amr% 1", "Amr1 Trim", "Amr2", "Amr% 2", "Amr2 Trim",
            "Amr3", "Amr% 3","Amr3 Trim", "Amr sale", "Amr% sale", "Amr sale Trim",
            "Affinity","Prog coef", "Dp coef", "Seas coef", "Sec coef", "CPP", "Ins", "CPSP", "Price"};

            int colOffset = 0;

            for (int i = 0; i < visibleColumns.Count(); i++)
            {
                if (visibleColumns[i])
                {
                    worksheet.Cells[rowOff, colOff + colOffset].Value = columnHeaders[i];
                    worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.BackgroundColor.SetColor(colors["header"]);
                    colOffset += 1;
                }
            }
        }
        private void AddDateHeaders(ExcelWorksheet worksheet, int rowOff, int colOff)
        {
            DateOnly startDate = DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpsdate));
            DateOnly endDate = DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpedate));

            int colOffset = 0;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var excelCell = worksheet.Cells[rowOff, colOff + colOffset];
                excelCell.Value = date.ToShortDateString();
                SetBackgroundColor(excelCell, colors["header"]);               
                colOffset += 1;
            }
        }

        private void AddMpTuple(MediaPlanTuple mpTuple, bool[] visibleColumns, ExcelWorksheet worksheet, int rowOff, int colOff)
        {
            int colOffset = 0;
            MediaPlan mediaPlan = mpTuple.MediaPlan;
            if (visibleColumns[0])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = chidChannelDictionary[mediaPlan.chid];
                colOffset += 1;
            }
            if (visibleColumns[1])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Name;
                colOffset += 1;
            }
            if (visibleColumns[2])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.DayPart == null ?
                    "" : mediaPlan.DayPart.name;
                colOffset += 1;
            }
            if (visibleColumns[3])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Position;
                colOffset += 1;
            }
            if (visibleColumns[4])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Stime;
                colOffset += 1;
            }
            if (visibleColumns[5])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Etime;
                colOffset += 1;
            }
            if (visibleColumns[6])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Blocktime;
                colOffset += 1;
            }
            if (visibleColumns[7])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Type;
                colOffset += 1;
            }
            if (visibleColumns[8])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Special;
                colOffset += 1;
            }
            if (visibleColumns[9])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr1;
                colOffset += 1;
            }
            if (visibleColumns[10])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrp1, 2);
                colOffset += 1;
            }
            if (visibleColumns[11])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr1trim;
                colOffset += 1;
            }
            if (visibleColumns[12])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr2;
                colOffset += 1;
            }
            if (visibleColumns[13])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrp2, 2);
                colOffset += 1;
            }
            if (visibleColumns[14])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr2trim;
                colOffset += 1;
            }
            if (visibleColumns[15])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr3;
                colOffset += 1;
            }
            if (visibleColumns[16])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrp3, 2);
                colOffset += 1;
            }
            if (visibleColumns[17])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amr3trim;
                colOffset += 1;
            }
            if (visibleColumns[18])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrsale;
                colOffset += 1;
            }
            if (visibleColumns[19])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Amrpsale, 2);
                colOffset += 1;
            }
            if (visibleColumns[20])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Amrsaletrim;
                colOffset += 1;
            }
            if (visibleColumns[21])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Affinity, 2);
                colOffset += 1;
            }
            if (visibleColumns[22])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Progcoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[23])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Dpcoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[24])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Seascoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[25])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Seccoef, 2);
                colOffset += 1;
            }
            if (visibleColumns[26])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Cpp, 2);
                colOffset += 1;
            }
            if (visibleColumns[27])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = mediaPlan.Insertations;
                colOffset += 1;
            }
            if (visibleColumns[28])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.PricePerSecond, 2);
                colOffset += 1;
            }
            if (visibleColumns[29])
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = Math.Round(mediaPlan.Price, 2);
                colOffset += 1;
            }

            AddMpTerms(mpTuple.Terms, worksheet, rowOff, colOff + colOffset);
        }

        private void AddMpTerms(IEnumerable<MediaPlanTerm> mpTerms, ExcelWorksheet worksheet, int rowOff, int colOff)
        {
            int colOffset = 0;

            DateOnly date = DateOnly.FromDateTime(TimeFormat.YMDStringToDateTime(_campaign.cmpsdate));

            foreach (var term in mpTerms)
            {
                var excelCell = worksheet.Cells[rowOff, colOff + colOffset];

                if (term == null)
                {
                    excelCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    SetBackgroundColor(excelCell, colors["null"]);
                }
                else
                {
                    excelCell.Value = term.Spotcode;
                    SetBackgroundColor(excelCell, colors["term"]);
                }

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    SetBorderColor(excelCell, ExcelBorderStyle.Thick, colors["weekday"]);
                }
                else
                {
                    SetBorderColor(excelCell, ExcelBorderStyle.Thin, colors["day"]);
                }

                date = date.AddDays(1);
                colOffset += 1;
            }
        }

        private void SetBackgroundColor(ExcelRange excelCell, System.Drawing.Color color)
        {
            excelCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            excelCell.Style.Fill.BackgroundColor.SetColor(color);
        }

        private void SetBorderColor(ExcelRange excelCell, ExcelBorderStyle thickness, System.Drawing.Color color)
        {
            excelCell.Style.Border.Left.Style = thickness;
            excelCell.Style.Border.Left.Color.SetColor(color);

            excelCell.Style.Border.Right.Style = thickness;
            excelCell.Style.Border.Right.Color.SetColor(color);

            excelCell.Style.Border.Top.Style = thickness;
            excelCell.Style.Border.Top.Color.SetColor(color);

            excelCell.Style.Border.Bottom.Style = thickness;
            excelCell.Style.Border.Bottom.Color.SetColor(color);
        }

        #endregion

        // To add default sorting by channel, then on selected, add line below in dgMediaPlans xaml file
        // and uncomment function below
        // Sorting="dgMediaPlans_Sorting"

        /*private void dgMediaPlans_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true; // Handle sorting manually

            var dataGrid = (DataGrid)sender;
            var collectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);

            if (collectionView != null)
            {
                // Clear existing sort descriptions
                collectionView.SortDescriptions.Clear();

                // Add "Channels" as the primary sort
                collectionView.SortDescriptions.Add(new SortDescription("MediaPlan.chid", ListSortDirection.Ascending));

                // Add the clicked column as a secondary sort
                var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;

                e.Column.SortDirection = direction;

                collectionView.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, direction));

                // Apply the updated sorting
                collectionView.Refresh();
            }
        }*/

        #region Synchronize scrolling between total and dgMediaPlan

        private void dgMediaPlans_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;

            // Find the ScrollViewer inside the DataGrid's template
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(dataGrid);

            // Subscribe to the ScrollChanged event
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            SetTotalsWidth();
        }


        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                {
                    return result;
                }

                T childOfChild = FindVisualChild<T>(child);

                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            var secondScrollViewer = svTotals;

            if (scrollViewer != null && secondScrollViewer != null)
            {
                secondScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        #endregion

 
    }
}
