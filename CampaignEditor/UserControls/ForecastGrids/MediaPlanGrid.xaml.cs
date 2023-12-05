﻿using CampaignEditor.Controllers;
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
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using System.ComponentModel;
using System.Collections.Specialized;
using Database.DTOs.ChannelDTO;
using OfficeOpenXml;
using System.Windows.Threading;
using CampaignEditor.Converters;
using OfficeOpenXml.Style;
using Border = System.Windows.Controls.Border;
using CampaignEditor.Helpers;
using System.Collections;
using System.Diagnostics;

namespace CampaignEditor.UserControls
{
    public partial class MediaPlanGrid : UserControl
    {

        public IAbstractFactory<AddSchema> _factoryAddSchema { get; set; }
        public IAbstractFactory<AMRTrim> _factoryAmrTrim { get; set; }


        public SchemaController _schemaController { get; set; }
        public MediaPlanController _mediaPlanController { get; set; }
        public MediaPlanTermController _mediaPlanTermController { get; set; }
        public SpotController _spotController { get; set; }
        public Dictionary<int, string> chidChannelDictionary { get; set; }

        // for checking if certain character can be written in spot cells
        HashSet<char> spotCodes = new HashSet<char>();
        // for mouse click event
        string lastSpotCell = "";

        // number of frozen columns
        int mediaPlanColumns = 29;

        // for saving old values of MediaPlan when I want to change it
        private MediaPlan _mediaPlanOldValues;
        private MediaPlan _mediaPlanToUpdate;
        bool isEditingEnded = false;

        public bool filterByIns = false;

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
        public MediaPlanConverter _converter { get; set; }
        CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        public ObservableBool CanUserEdit { get; set; }
        public DataGrid Grid
        {
            get { return dgMediaPlans; }
        }

        public MediaPlanGrid()
        {

            CanUserEdit = new ObservableBool(true);

            if (MainWindow.user.usrlevel == 2)
            {
                CanUserEdit.Value = false;
            }

            InitializeComponent();

            dgMediaPlans.IsManipulationEnabled = CanUserEdit.Value;

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
        public async Task Initialize(CampaignDTO campaign)
        {
            tcChannel.Binding = new Binding()
            {
                Path = new PropertyPath("MediaPlan.chid"),
                Converter = new ChidToChannelConverter(),
                ConverterParameter = chidChannelDictionary
            };

            this.frozenColumnsNum = mediaPlanColumns;
            this.Schema.FrozenColumnCount = frozenColumnsNum;

            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            DeleteDateColumns(frozenColumnsNum);
            InitializeDateColumns();

            await InitializeSpots(_campaign.cmpid);


            myDataView = CollectionViewSource.GetDefaultView(_allMediaPlans);
            dgMediaPlans.ItemsSource = myDataView;

            myDataView.SortDescriptions.Add(new SortDescription("MediaPlan.chid", ListSortDirection.Ascending));
            myDataView.SortDescriptions.Add(new SortDescription("MediaPlan.stime", ListSortDirection.Ascending));
            myDataView.Filter = d =>
            {
                var mediaPlan = ((MediaPlanTuple)d).MediaPlan;
                var mpTerms = ((MediaPlanTuple)d).Terms;

                if (filterByIns == true)
                {
                    return _selectedChannels.Any(c => c.chid == mediaPlan.chid) &&
                                           mediaPlan.Insertations > 0 &&
                                           mpTerms.Any(t => t != null && _filteredDays.Contains(t.Date.DayOfWeek));
                }
                
                return _selectedChannels.Any(c => c.chid == mediaPlan.chid) &&
                                           mpTerms.Any(t => t != null && _filteredDays.Contains(t.Date.DayOfWeek));
            };

            _allMediaPlans.CollectionChanged += OnCollectionChanged;
            _selectedChannels.CollectionChanged += OnCollectionChanged;

        }

        public async Task InitializeSpots(int cmpid)
        {
            spotCodes.Clear();
            var spots = await _spotController.GetSpotsByCmpid(cmpid);
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
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise the SelectionChanged event
            SelectionChanged?.Invoke(sender, e);
        }


        #region DgMediaPlans

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

                var disabledCellColor = Brushes.LightGoldenrodYellow;
                var enabledCellColor = Brushes.LightGreen;
                if (DateTime.TryParse(column.Header.ToString(), out DateTime columnHeaderDate))
                {
                    if (columnHeaderDate <= DateTime.Today)
                    {
                        // Apply the dimmed column style
                        disabledCellColor = Brushes.Goldenrod;
                        enabledCellColor = Brushes.Green;
                    }
                }

                var binding = new Binding($"Terms[{dates.IndexOf(date)}].Spotcode");
                //binding.ValidationRules.Add(new CharLengthValidationRule(1)); // add validation rule to restrict input to a single character
                column.Binding = binding;

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

        private void SimulateTextInput(string text)
        {

            foreach (char c in text)
            {
                var inputEvent = new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, this, c.ToString()));
                inputEvent.RoutedEvent = UIElement.PreviewTextInputEvent;
                InputManager.Current.ProcessInput(inputEvent);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell == null)
                return;
            var mpTerm = GetSelectedMediaPlanTerm(cell);
            if (mpTerm == null)
            {
                return;
            }
            SimulateTextInput(lastSpotCell);
        }

        private async void OnCellPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!CanUserEdit.Value) 
                return;

            DataGridCell cell = sender as DataGridCell;
            TextBlock textBlock = cell.Content as TextBlock;

            DateTime currentDate = DateTime.Now;
            var mpTerm = GetSelectedMediaPlanTerm(cell);
            if (mpTerm == null || mpTerm.Date <= DateOnly.FromDateTime(currentDate))
            {
                return;
            }

            char? spotcodeNull = e.Text.Trim()[0];
            if (spotcodeNull.HasValue)
            {
                char spotcode = Char.ToUpper(spotcodeNull.Value);
                lastSpotCell = spotcode.ToString();
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

                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.Xmptermid, mpTerm.Xmpid, mpTerm.Date, cell.Content.ToString()));

                    var mediaPlanTuple = dgMediaPlans.SelectedItem as MediaPlanTuple;
                    if (mediaPlanTuple != null)
                    {
                        var mediaPlan = mediaPlanTuple.MediaPlan;
                        await _converter.ComputeExtraProperties(mediaPlan, true);
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                    }


                    mpTerm.Spotcode = cell.Content.ToString().Trim();

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

                        await _mediaPlanTermController.UpdateMediaPlanTerm(
                        new UpdateMediaPlanTermDTO(mpTerm.Xmptermid, mpTerm.Xmpid, mpTerm.Date, cell.Content.ToString().Trim()));

                        var mediaPlanTuple = dgMediaPlans.SelectedItem as MediaPlanTuple;
                        if (mediaPlanTuple != null)
                        {
                            var mediaPlan = mediaPlanTuple.MediaPlan;
                            await _converter.ComputeExtraProperties(mediaPlan, true);
                            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                        }

                        mpTerm.Spotcode = cell.Content.ToString().Trim();
                        lastSpotCell = mpTerm.Spotcode;

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

            // Disable usual mechanisms
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
            }


            // if cell is not binded to mediaPlanTerm, or if user don't have privileges disable editing
            var tuple = (MediaPlanTuple)cell.DataContext;
            var mpTerms = tuple.Terms;
            var index = cell.Column.DisplayIndex - mediaPlanColumns;
            MediaPlanTerm mpTerm;
            try
            {
                mpTerm = mpTerms[index];
            }
            catch
            {
                return;
            }
            DateTime currentDate = DateTime.Now;

            if (!CanUserEdit.Value || mpTerm == null || mpTerm.Date <= DateOnly.FromDateTime(currentDate))
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

                string? spotcode = mpTerm.Spotcode;
                if (spotcode != null)
                    spotcode = spotcode.Trim();
                if (spotcode == null || spotcode.Length == 1)
                {
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.Xmptermid, mpTerm.Xmpid, mpTerm.Date, null));

                    mpTerm.Spotcode = null;
                    cell.Content = "";
                }
                else if (spotcode.Length == 2)
                {
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.Xmptermid, mpTerm.Xmpid, mpTerm.Date, spotcode[0].ToString().Trim()));
                    mpTerm.Spotcode = spotcode[0].ToString();
                    cell.Content = spotcode[0];
                }

                var mediaPlanTuple = dgMediaPlans.SelectedItem as MediaPlanTuple;
                if (mediaPlanTuple != null)
                {
                    var mediaPlan = mediaPlanTuple.MediaPlan;
                    await _converter.ComputeExtraProperties(mediaPlan, true);
                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                }

            }

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

        private MediaPlanTerm GetSelectedMediaPlanTerm(DataGridCell cell)
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
            ObservableCollection<MediaPlanTerm> mpTerms = tuple.Terms;

            // Get the MediaPlanTerm for the selected cell
            int rowIndex = row.GetIndex();
            MediaPlanTerm mpTerm = null;
            try
            {
                mpTerm = mpTerms[columnIndex - frozenColumnsNum];
            
            }
            catch
            {
                return null;
            }

            return mpTerm;
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
                _mediaPlanOldValues = _converter.CopyMP(_mediaPlanToUpdate);

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
                // Compare with the original value and revert if needed
                if (_mediaPlanToUpdate != null && !_converter.SameMPValues(_mediaPlanToUpdate, _mediaPlanOldValues))
                {
                    // If coefs are changed, we need to recalculate price
                    double eps = 0.0001;
                    if (Math.Abs(_mediaPlanToUpdate.Progcoef - _mediaPlanOldValues.Progcoef) > eps ||
                        Math.Abs(_mediaPlanToUpdate.Dpcoef - _mediaPlanOldValues.Dpcoef) > eps ||
                        Math.Abs(_mediaPlanToUpdate.Seccoef - _mediaPlanOldValues.Seccoef) > eps ||
                        Math.Abs(_mediaPlanToUpdate.Seascoef - _mediaPlanOldValues.Seascoef) > eps)
                    {
                        await _converter.CoefsChanged(_mediaPlanToUpdate);
                    }
                    else if(_mediaPlanToUpdate.Stime != _mediaPlanOldValues.Stime ||
                            _mediaPlanToUpdate.Etime != _mediaPlanOldValues.Etime ||
                            _mediaPlanToUpdate.Blocktime != _mediaPlanOldValues.Blocktime)
                    {
                        try
                        {
                            _mediaPlanToUpdate.Stime = TimeFormat.ReturnGoodTimeFormat(_mediaPlanToUpdate.Stime.Trim());
                            _mediaPlanToUpdate.Etime = TimeFormat.ReturnGoodTimeFormat(_mediaPlanToUpdate.Etime.Trim());
                            if (_mediaPlanToUpdate.Blocktime != null)
                                _mediaPlanToUpdate.Blocktime = TimeFormat.ReturnGoodTimeFormat(_mediaPlanToUpdate.Blocktime.Trim());
                        }
                        catch
                        {
                            MessageBox.Show("Wrong Time Format", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
                            isEditingEnded = false;
                            _mediaPlanToUpdate.Stime = _mediaPlanOldValues.Stime;
                            _mediaPlanToUpdate.Etime = _mediaPlanOldValues.Etime;
                            _mediaPlanToUpdate.Blocktime = _mediaPlanOldValues.Blocktime;
                            return;
                        }
                    }
                    try
                    {
                        var mpDTO = _converter.ConvertToDTO(_mediaPlanToUpdate);
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(mpDTO));
                        _converter.CopyValues(_mediaPlanToUpdate, _mediaPlanToUpdate);
                    }
                    catch
                    {
                        _converter.CopyValues(_mediaPlanToUpdate, _mediaPlanOldValues);
                    }
                }
                isEditingEnded = false;
            }

        }

        #endregion

        #endregion

        #region ContextMenu
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
                    };

                    menu.Items.Add(item);
                }

                Schema.ContextMenu = menu;
            }
            else
            {
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
                        // Check if the clicked cell is in the "AMR" columns
                        /*if (cell.Column.Header.ToString() == "AMR 1" || cell.Column.Header.ToString() == "AMR% 1")
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
                        }*/
                        menu.Items.Add(trimAmr);
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

        private async Task<RoutedEventHandler> UpdateMediaPlan(MediaPlanTuple mediaPlanTuple)
        {
            async void handler(object sender, RoutedEventArgs e)
            {
                var f = _factoryAddSchema.Create();
                
            }

            return handler;
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


        public event EventHandler AddMediaPlanClicked;

        private void OnAddMediaPlanClicked()
        {
            AddMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<UpdateMediaPlanEventArgs> UpdateMediaPlanClicked;

        private void OnUpdateMediaPlanClicked(MediaPlanTuple mediaPlanTuple)
        {
            UpdateMediaPlanClicked?.Invoke(this, new UpdateMediaPlanEventArgs(mediaPlanTuple));
        }

        public event EventHandler DeleteMediaPlanClicked;

        private void OnDeleteMediaPlanClicked()
        {
            DeleteMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

        public void PopulateWorksheet(ExcelWorksheet worksheet, int rowOff = 0, int colOff = 0)
        {

            var dataGrid = dgMediaPlans;

            // Disable row virtualization
            dataGrid.EnableRowVirtualization = false;
            dataGrid.EnableColumnVirtualization = false;

            //Unselect all rows
            dataGrid.SelectedItem = null;

            // Wait for the dispatcher to finish processing pending messages
            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                // Get the visible columns from the DataGrid
                var visibleColumns = dataGrid.Columns.Where(c => c.Visibility == Visibility.Visible).ToList();

                // Set the column headers in Excel
                for (int columnIndex = 0; columnIndex < visibleColumns.Count; columnIndex++)
                {
                    var column = visibleColumns[columnIndex];
                    worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Value = column.Header;
                    worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1 + rowOff, columnIndex + 1 + colOff].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                }

                // Set the cell values and colors in Excel
                for (int i=0, rowIndex = 0; i < dataGrid.Items.Count; rowIndex++, i++)
                {
                    var dataItem = (MediaPlanTuple)dataGrid.Items[i];
                    /*if (dataItem.MediaPlan.Insertations == 0)
                    {
                        rowIndex --;
                        continue;
                    }*/
                    for (int columnIndex = 0; columnIndex < visibleColumns.Count; columnIndex++)
                    {
                        var column = visibleColumns[columnIndex];
                        var cellContent = column.GetCellContent(dataItem);
                        if (cellContent is TextBlock textBlock)
                        {
                            // Try to parse the text to a double
                            if (double.TryParse(textBlock.Text, out double numericValue))
                            {
                                // If successful, write the numeric value to the Excel cell
                                worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff].Value = numericValue;
                            }
                            else
                            {
                                // If not a valid number, write the text as it is
                                worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff].Value = textBlock.Text;
                            }
                        }
                        else if (cellContent is CheckBox checkbox)
                        {
                            var cellValue = checkbox.IsChecked == true ? "yes" : "no" ;
                            worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff].Value = cellValue;

                        }
                        else if (cellContent is ComboBox combobox)
                        {
                            var cellValue = combobox.Text.Trim();
                            worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff].Value = cellValue;

                        }
                        else
                        {
                            worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff].Value = "";
                        }

                        // Set the cell color
                        var cell = FindParentDataGridCell(cellContent) as DataGridCell;
                        if (cell != null)
                        {
                            var cellColor = cell.Background;
                            if (cellColor == Brushes.Green)
                            {
                                cellColor = Brushes.LightGreen;
                            }
                            else if(cellColor == Brushes.Goldenrod)
                            {
                                cellColor = Brushes.LightGoldenrodYellow;
                            }
                            var cellBorderColor = cell.BorderBrush;

                            var drawingColor = System.Drawing.Color.Black;
                            var drawingThickness = ExcelBorderStyle.Thin;

                            if (cellBorderColor == Brushes.OrangeRed)
                            {
                                drawingColor = System.Drawing.Color.OrangeRed;
                                drawingThickness = ExcelBorderStyle.Thick;
                            }


                            if (cellColor != null)
                            {
                                var excelColor = System.Drawing.ColorTranslator.FromHtml(cellColor.ToString());
                                var excelCell = worksheet.Cells[rowIndex + 2 + rowOff, columnIndex + 1 + colOff];
                                excelCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                excelCell.Style.Fill.BackgroundColor.SetColor(excelColor);

                                excelCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                excelCell.Style.Border.Left.Style = drawingThickness;
                                excelCell.Style.Border.Left.Color.SetColor(drawingColor);
                                excelCell.Style.Border.Right.Style = drawingThickness;
                                excelCell.Style.Border.Right.Color.SetColor(drawingColor);
                                excelCell.Style.Border.Top.Style = drawingThickness;
                                excelCell.Style.Border.Top.Color.SetColor(drawingColor);
                                excelCell.Style.Border.Bottom.Style = drawingThickness;
                                excelCell.Style.Border.Bottom.Color.SetColor(drawingColor);
                            }
                            double cellHeight = cell.ActualHeight;
                            double cellWidth = cell.ActualWidth / 7;

                            // Set the size of the Excel cell
                            worksheet.Row(rowIndex + 2 + rowOff).Height = cellHeight;
                            worksheet.Column(columnIndex + 1 + colOff).Width = cellWidth;
                            //worksheet.Row(rowIndex + 2 + rowOff).OutlineLevel = 2;

                        }

                    }
                }
            }));

            // Disable row virtualization
            dataGrid.EnableRowVirtualization = true;
            dataGrid.EnableColumnVirtualization = true;          

        }

        private DataGridCell FindParentDataGridCell(FrameworkElement element)
        {
            // Traverse up the visual tree to find the DataGridCell
            FrameworkElement parent = element;
            while (parent != null && !(parent is DataGridCell))
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            return parent as DataGridCell;
        }

        private void dgMediaPlans_Sorting(object sender, DataGridSortingEventArgs e)
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
        }
       
    }
}
