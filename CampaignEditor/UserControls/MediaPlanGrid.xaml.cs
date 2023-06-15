using CampaignEditor.Controllers;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SchemaDTO;
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

        // for checking if certain character can be written in spot cells
        HashSet<char> spotCodes = new HashSet<char>();
        // for mouse click event
        string lastSpotCell = "";

        // number of frozen columns
        int mediaPlanColumns = 25;

        private int frozenColumnsNum
        {
            get { return (int)GetValue(frozenColumnsNumProperty); }
            set { SetValue(frozenColumnsNumProperty, value); }
        }

        public static readonly DependencyProperty frozenColumnsNumProperty =
            DependencyProperty.Register(nameof(frozenColumnsNum), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public ObservableCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableCollection<MediaPlanTuple>();

        public ObservableCollection<ChannelDTO> _selectedChannels = new ObservableCollection<ChannelDTO>();

        public MediaPlanConverter _converter { get; set; }
        CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        public MediaPlanGrid()
        {
            InitializeComponent();
        }

        
        public DataGrid Schema
        {
            get { return dgMediaPlans; }
        }

        public async Task Initialize(CampaignDTO campaign)
        {

            this.frozenColumnsNum = mediaPlanColumns;
            this.Schema.FrozenColumnCount = frozenColumnsNum;

            _campaign = campaign;
            //this._allMediaPlans = _allMediaPlans;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            InitializeDateColumns();


            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            for (int i = 0; i < spots.Count(); i++)
            {
                spotCodes.Add((char)('A' + i));
            }
            
            ICollectionView myDataView = CollectionViewSource.GetDefaultView(_allMediaPlans);
            dgMediaPlans.ItemsSource = myDataView;

            myDataView.SortDescriptions.Add(new SortDescription("MediaPlan.name", ListSortDirection.Ascending));
            myDataView.Filter = d =>
            {
                var mediaPlan = ((MediaPlanTuple)d).MediaPlan;

                return mediaPlan.active && _selectedChannels.Any(c => c.chid == mediaPlan.chid);
            };

            _allMediaPlans.CollectionChanged += OnCollectionChanged;
            _selectedChannels.CollectionChanged += OnCollectionChanged;

        }

        // Method to handle the CollectionChanged event
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Call Refresh() on the view to update it
            ICollectionView view = CollectionViewSource.GetDefaultView(_allMediaPlans);
            view.Refresh();
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise the SelectionChanged event
            SelectionChanged?.Invoke(sender, e);
        }

        /*public event EventHandler<MouseButtonEventArgs> MouseRightButtonDown;

        private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Raise the SelectionChanged event
            MouseRightButtonDown?.Invoke(sender, e);
        }*/


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


                var binding = new Binding($"Terms[{dates.IndexOf(date)}].spotcode");
                //binding.ValidationRules.Add(new CharLengthValidationRule(1)); // add validation rule to restrict input to a single character
                column.Binding = binding;

                var cellStyle = new Style(typeof(DataGridCell));

                // Adding setters to cells
                //var keyDownEventSetter = new EventSetter(DataGridCell.PreviewTextInputEvent, new TextCompositionEventHandler(OnCellPreviewTextInput));
                var textInputEventSetter = new EventSetter(PreviewTextInputEvent, new TextCompositionEventHandler(OnCellPreviewTextInput));
                var keyDownEventSetter = new EventSetter(PreviewKeyDownEvent, new KeyEventHandler(OnCellPreviewKeyDown));
                var mouseLeftButtonDownEventSetter = new EventSetter(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));

                cellStyle.Setters.Add(textInputEventSetter);
                cellStyle.Setters.Add(keyDownEventSetter);
                cellStyle.Setters.Add(mouseLeftButtonDownEventSetter);
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
            SimulateTextInput(lastSpotCell);
        }

        private async void OnCellPreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            DataGridCell cell = sender as DataGridCell;
            TextBlock textBlock = cell.Content as TextBlock;

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

                    var mpTerm = GetSelectedMediaPlanTermDTO(cell);
                    await _mediaPlanTermController.UpdateMediaPlanTerm(
                    new UpdateMediaPlanTermDTO(mpTerm.xmptermid, mpTerm.xmpid, mpTerm.date, cell.Content.ToString()));

                    var mediaPlanTuple = dgMediaPlans.SelectedItem as MediaPlanTuple;
                    if (mediaPlanTuple != null)
                    {
                        var mediaPlan = mediaPlanTuple.MediaPlan;
                        await _converter.ComputeExtraProperties(mediaPlan);
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                    }


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

                        var mediaPlanTuple = dgMediaPlans.SelectedItem as MediaPlanTuple;
                        if (mediaPlanTuple != null)
                        {
                            var mediaPlan = mediaPlanTuple.MediaPlan;
                            await _converter.ComputeExtraProperties(mediaPlan);
                            await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                        }

                        mpTerm.spotcode = cell.Content.ToString().Trim();
                        lastSpotCell = mpTerm.spotcode;

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
                if (spotcode != null)
                    spotcode = spotcode.Trim();
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

                var mediaPlanTuple = dgMediaPlans.SelectedItem as MediaPlanTuple;
                if (mediaPlanTuple != null)
                {
                    var mediaPlan = mediaPlanTuple.MediaPlan;
                    await _converter.ComputeExtraProperties(mediaPlan);
                    await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
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
            MediaPlanTermDTO mpTermDTO = mpTerms[columnIndex - frozenColumnsNum];

            return mpTermDTO;
        }

        #endregion

        #region MediaPlan columns  

        private async void TextBoxAMRTrim_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tuple = dgMediaPlans.SelectedItems[0] as MediaPlanTuple;
            var textBox = sender as TextBox;

            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            string propertyName = bindingExpr?.ResolvedSourcePropertyName;
            if (tuple != null)
            {
                var mediaPlan = tuple.MediaPlan;
                int value = 0;

                if (propertyName == "Amr1trim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amr1trim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amr1trim.ToString();
                    }
                }
                else if (propertyName == "Amr2trim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amr2trim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amr2trim.ToString();
                    }
                }
                else if (propertyName == "Amr3trim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amr3trim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
                    }
                    else
                    {
                        textBox.Text = mediaPlan.amr3trim.ToString();
                    }
                }
                else if (propertyName == "Amrsaletrim")
                {
                    if (textBox != null && (textBox.Text.Trim() == "" || Int32.TryParse(textBox.Text.Trim(), out value)))
                    {
                        value = textBox.Text.Trim() == "" ? 0 : value;
                        mediaPlan.amrsaletrim = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));
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
            var tuple = dgMediaPlans.SelectedItems[0] as MediaPlanTuple;
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
                        mediaPlan.Progcoef = value;
                        await _mediaPlanController.UpdateMediaPlan(new UpdateMediaPlanDTO(_converter.ConvertToDTO(mediaPlan)));

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
                deleteItem.Header = "Delete MediaPlan";
                deleteItem.Click += async (obj, ea) =>
                {
                    OnDeleteMediaPlanClicked();
                };
                menu.Items.Add(deleteItem);

                MenuItem addMediaPlanItem = new MenuItem();
                addMediaPlanItem.Header = "Add MediaPlan";
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

                if (dependencyObject == null)
                {
                    return;
                }

                DataGridCell cell = dependencyObject as DataGridCell;

                var mediaPlanTuple = Schema.SelectedItem as MediaPlanTuple;
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

        private async Task<RoutedEventHandler> TrimAmrAsync(MediaPlan mediaPlan, string message, string attr, int? trimValue)
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


        public event EventHandler AddMediaPlanClicked;

        private void OnAddMediaPlanClicked()
        {
            AddMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DeleteMediaPlanClicked;

        private void OnDeleteMediaPlanClicked()
        {
            DeleteMediaPlanClicked?.Invoke(this, EventArgs.Empty);
        }

    }
}
