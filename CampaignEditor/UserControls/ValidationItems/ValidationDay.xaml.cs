﻿using Database.Entities;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using CampaignEditor.Controllers;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using CampaignEditor.Helpers;
using System.Windows.Input;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Data;
using System.Data;
using System.Windows.Controls.Primitives;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Interaction logic for ValidationDay.xaml
    /// </summary>
    public partial class ValidationDay : UserControl
    {
        public List<TermTuple?> _termTuples;
        public List<MediaPlanRealized?> _mpRealizedTuples;
        private DateOnly date;

        public MediaPlanRealizedController _mediaPlanRealizedController;
        public DataGrid ExpectedGrid { get { return dgExpected; } }
        public DataGrid RealizedGrid { get { return dgRealized; } }

        public event EventHandler<IndexEventArgs> InvertedExpectedColumnVisibility;
        public event EventHandler<IndexEventArgs> InvertedRealizedColumnVisibility;
        public event EventHandler<CheckDateEventArgs> CheckNewDataDay;
        public event EventHandler GridOpened;
        public event EventHandler GridClosed;

        public event EventHandler<UpdateMediaPlanRealizedEventArgs> UpdatedMediaPlanRealized;
        public event EventHandler<UpdateMediaPlanRealizedEventArgs> ProgcoefChangedMediaPlanRealized;
        public event EventHandler<SizeChangedEventArgs> DgExpectedSizeChanged;
        public event EventHandler<DoubleValueEventArgs> DgExpectedWidthChanged;
        public event EventHandler<ColumnWidthChangedEventArgs> DgExpectedColumnWidthChanged;
        public event EventHandler<ColumnWidthChangedEventArgs> DgRealizedColumnWidthChanged;

        bool hideExpected = false;

        string columnToEdit = string.Empty;
        decimal oldCellValue;
        bool skipCellUpdate = true;
        bool canEdit = true;

        // For filtering
        ICollectionView dataViewExpected;
        ICollectionView dataViewRealized;

        public List<TermTuple?> GetViewExpected { get { return dataViewExpected.Cast<TermTuple?>().ToList(); } }
        public List<MediaPlanRealized?> GetViewRealized { get { return dataViewRealized.Cast<MediaPlanRealized?>().ToList(); } }

        List<int> _selectedChids = new List<int>();
        List<int> _selectedChrdsids = new List<int>();

        public ValidationDay(DateOnly date,
            List<TermTuple> termTuples,
            List<MediaPlanRealized> mpRealizedTuples,
            string expectedGridMask, string realizedGridMask,
            bool isCompleted)
        {
            InitializeComponent();

            dgExpected.ItemsSource = null;
            dgRealized.ItemsSource = null;

            _termTuples = termTuples;
            _mpRealizedTuples = mpRealizedTuples;

            if (isCompleted)
            {
                SetCompleted(true, true);
            }
            else
                SetCompleted(false, true);

            this.date = date;
            if (_termTuples.Count > 0)
            {
                dataViewExpected = CollectionViewSource.GetDefaultView(_termTuples);
                dgExpected.ItemsSource = dataViewExpected;
                dataViewExpected.Filter = d =>
                {
                    bool result = true;

                    if (_selectedChids.Count > 0)
                    {
                        var mediaPlan = ((TermTuple)d).MediaPlan;
                        result = _selectedChids.Any(c => mediaPlan != null && c == mediaPlan.chid);
                    }

                    return result;
                };
                //dgExpected.ItemsSource = _termTuples;

            }
            if (_mpRealizedTuples.Count > 0)
            {
                dataViewRealized = CollectionViewSource.GetDefaultView(_mpRealizedTuples);
                dgRealized.ItemsSource = dataViewRealized;
                dataViewRealized.Filter = d =>
                {
                    bool result = true;
                    if (_selectedChrdsids.Count > 0)
                    {
                        var chid = ((MediaPlanRealized)d).chid;
                        result = _selectedChrdsids.Any(c => chid.HasValue && c == chid);
                    }

                    return result;
                };
                //dgRealized.ItemsSource = _mpRealizedTuples;

            }
            SetUserControl();
            SetGridMasks(expectedGridMask, realizedGridMask);



        }

        public void RefreshDate(List<TermTuple> termTuples,
            List<MediaPlanRealized> mpRealizedTuples)
        {
            dgExpected.ItemsSource = null;
            dgRealized.ItemsSource = null;

            _termTuples = termTuples;
            _mpRealizedTuples = mpRealizedTuples;

            if (_termTuples.Count > 0)
            {
                dataViewExpected = CollectionViewSource.GetDefaultView(_termTuples);
                dgExpected.ItemsSource = dataViewExpected;
                dataViewExpected.Filter = d =>
                {
                    bool result = true;

                    if (_selectedChids.Count > 0)
                    {
                        var mediaPlan = ((TermTuple)d).MediaPlan;
                        result = _selectedChids.Any(c => mediaPlan != null && c == mediaPlan.chid);
                    }

                    return result;
                };
                //dgExpected.ItemsSource = _termTuples;

            }
            if (_mpRealizedTuples.Count > 0)
            {
                dataViewRealized = CollectionViewSource.GetDefaultView(_mpRealizedTuples);
                dgRealized.ItemsSource = dataViewRealized;
                dataViewRealized.Filter = d =>
                {
                    bool result = true;
                    if (_selectedChrdsids.Count > 0)
                    {
                        var chid = ((MediaPlanRealized)d).chid;
                        result = _selectedChrdsids.Any(c => chid.HasValue && c == chid);
                    }

                    return result;
                };
                //dgRealized.ItemsSource = _mpRealizedTuples;

            }
            SetUserControl();
        }




        private void ValidationDay_Loaded(object sender, RoutedEventArgs e)
        {
            /*dgExpected.MaxHeight = SystemParameters.PrimaryScreenHeight * 0.6;
            dgRealized.MaxHeight = SystemParameters.PrimaryScreenHeight * 0.6;*/
            gridItems.MaxHeight = SystemParameters.PrimaryScreenHeight * 0.6;
        }

        private void SetUserControl()
        {
            /*int exCount = _termTuples.Where(tt => tt != null && tt.Status != -1).Count();
            int realCount = _mpRealizedTuples.Where(rt => rt != null && rt.status != -1).Count();*/
            int exCount = 0;
            int realCount = 0;

            if (_termTuples.Count > 0)
                exCount = GetViewExpected.Where(tt => tt != null && tt.Status != -1 && tt.StatusAD != 2).Count();
            if (_mpRealizedTuples.Count > 0)
                realCount = GetViewRealized.Where(rt => rt != null && rt.status != -1).Count(); 
            
            lblExCount.Content = exCount;
            lblRealCount.Content = realCount;
            lblDate.Content = date.ToShortDateString();
            
        }


        private void SetGridMasks(string expectedGridMask, string realizedGridMask)
        {
            for (int i=0; i<expectedGridMask.Count(); i++)
            {
                dgExpected.Columns[i].Visibility = expectedGridMask[i] == '1' ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }

            for (int i = 0; i < realizedGridMask.Count(); i++)
            {
                dgRealized.Columns[i].Visibility = realizedGridMask[i] == '1' ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
        }

        public void InvertExpectedColumnVisibility(int index)
        {
            dgExpected.Columns[index].Visibility = dgExpected.Columns[index].Visibility == System.Windows.Visibility.Visible ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }
        public void InvertRealizedColumnVisibility(int index)
        {
            dgRealized.Columns[index].Visibility = dgRealized.Columns[index].Visibility == System.Windows.Visibility.Visible ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        public void ChannelsChanged(IEnumerable<int> chids, IEnumerable<int> chrdsids)
        {
            _selectedChids = chids.ToList();
            _selectedChrdsids = chrdsids.ToList();
            if (_termTuples.Count > 0)
            {
                dataViewExpected.Refresh();
            }
            if (_mpRealizedTuples.Count > 0)
            {
                dataViewRealized.Refresh();
            }

            SetUserControl();
        }

        private void DgExpected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // for selecting both grids
            //int index = dgExpected.SelectedIndex;
            //dgRealized.SelectedIndex = index;
        }
        private void DgRealized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // for selecting both grids
            //int index = dgRealized.SelectedIndex;
            //dgExpected.SelectedIndex = index;
        }

        private void dgExpected_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // check if it's clicked on header
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;

            // For setting visibility of columns
            if (IsCellInDataGridHeader(dependencyObject))
            {
                ShowHeadersMenu(dgExpected);
            }
            
        }

        private void dgRealized_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // check if it's clicked on header
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;

            // For setting visibility of columns
            if (IsCellInDataGridHeader(dependencyObject))
            {
                ShowHeadersMenu(dgRealized);
            }
        }

        private void ShowHeadersMenu(DataGrid dataGrid)
        {
            ContextMenu menu = new ContextMenu();
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                var column = dataGrid.Columns[i];
                int index = i;
                MenuItem item = new MenuItem();
                item.Header = column.Header.ToString().Trim();
                item.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                item.Click += (obj, ea) =>
                {
                    if (dataGrid.Name == "dgExpected")
                    {
                        InvertedExpectedColumnVisibility?.Invoke(this, new IndexEventArgs(index));
                    }
                    else if (dataGrid.Name == "dgRealized")
                    {
                        InvertedRealizedColumnVisibility?.Invoke(this, new IndexEventArgs(index));
                    }
                    item.IsChecked = item.IsChecked ? false : true;
                };

                item.StaysOpenOnClick = true;

                menu.Items.Add(item);
            }
            menu.StaysOpen = true;
            dataGrid.ContextMenu = menu;
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

        #region Change dataGrid focus

        private async void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Handle changing between grids
            var dataGrid = sender as DataGrid;
            if (e.Key == Key.Right && dataGrid == dgExpected && IsLastVisibleColumn(dataGrid))
            {
                dgRealized.Focus();
                MoveFocusToFirstCell(dgRealized);
                e.Handled = true;
            }
            else if (e.Key == Key.Left && dataGrid == dgRealized && IsFirstVisibleColumn(dataGrid))
            {
                dgExpected.Focus();
                MoveFocusToLastCell(dgExpected);
                e.Handled = true;
            }

            // Stop editing 
            if (!canEdit || dgRealized.SelectedItems.Count == 0)
                return;

            // Handle changing status or accepted
            int status = 2;
            bool changeAccept = false;
            bool acceptValue = false;
            switch (e.Key)
            {
                case Key.F1: status = 1; break;
                case Key.F2: status = 2; break;
                case Key.F3: status = 3; break;
                case Key.F4: status = 4; break;
                case Key.F5: status = 5; break;
                case Key.F6: status = 6; break;
                case Key.F7: status = 7; break;
                case Key.F8: status = 8; break;
                case Key.X: changeAccept = true; acceptValue = false; break;
                case Key.Z:
                case Key.Y:    
                    changeAccept = true; acceptValue = true; break;
                default: return;
            }


            if (changeAccept)
            {
                foreach (MediaPlanRealized mpRealized in dgRealized.SelectedItems)
                {
                    if (mpRealized.status == -1)
                        continue;

                    RealizedAcceptChanged(mpRealized, acceptValue);

                }
            }
            else
            {
                foreach (MediaPlanRealized mpRealized in dgRealized.SelectedItems)
                {
                    if (mpRealized.status == -1 || 
                       (mpRealized.Accept != null && mpRealized.Accept.Value))
                        continue;

                    bool recalculatePrice = false;
                    bool gratisStatus = status == 3 || status == 4;
                    bool gratisMPRStatus = mpRealized.status == 3 || mpRealized.status == 4;
                    if (gratisStatus != gratisMPRStatus)
                        recalculatePrice = true;
                    mpRealized.status = status;

                    UpdatedMediaPlanRealized?.Invoke(this, new UpdateMediaPlanRealizedEventArgs(mpRealized, false, recalculatePrice));
                }
            }
            e.Handled = true;


        }


        private bool IsLastVisibleColumn(DataGrid dataGrid)
        {
            if (dataGrid.CurrentCell != null)
            {
                DataGridColumn focusedColumn = dataGrid.CurrentCell.Column;

                if (focusedColumn != null)
                {
                    // Get the index of the focused column
                    int columnIndex = dataGrid.Columns.IndexOf(focusedColumn);
                    for (int i=columnIndex+1; i<dataGrid.Columns.Count; i++)
                    {
                        if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                            return false;
                    }
                }
            }

            return true; // There are no more visible columns
        }

        private bool IsFirstVisibleColumn(DataGrid dataGrid)
        {
            if (dataGrid.CurrentCell != null)
            {
                DataGridColumn focusedColumn = dataGrid.CurrentCell.Column;

                if (focusedColumn != null)
                {
                    // Get the index of the focused column
                    int columnIndex = dataGrid.Columns.IndexOf(focusedColumn);
                    for (int i = 0; i < columnIndex; i++)
                    {
                        if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                            return false;
                    }
                }
            }

            return true; // There are no visible columns before current
        }       

        private void MoveFocusToFirstCell(DataGrid dataGrid)
        {
            if (dataGrid != null && dataGrid.SelectedItem != null)
            {
                // Move to the most left cell in the same row
                dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.SelectedItem, dataGrid.Columns[0]);
                int firstVisible = dataGrid.Columns.First(c => c.Visibility == Visibility.Visible).DisplayIndex;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem, dataGrid.Columns[firstVisible]);
            }
        }

        private void MoveFocusToLastCell(DataGrid dataGrid)
        {
            if (dataGrid != null && dataGrid.SelectedItem != null)
            {
                // Move to the most right cell in the same row
                dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.SelectedItem, dataGrid.Columns[dataGrid.Columns.Count - 1]);
                int lastVisible = dataGrid.Columns.Last(c => c.Visibility == Visibility.Visible).DisplayIndex;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem, dataGrid.Columns[lastVisible]);
            }
        }

        public void HideExpected()
        {
            hideExpected = true;
            dgExpected.Visibility = Visibility.Collapsed;
            lblExCount.Visibility = Visibility.Collapsed;
            lblDate.HorizontalAlignment = HorizontalAlignment.Left;
            lblRealCount.HorizontalAlignment = HorizontalAlignment.Center;
            dgRealized.HorizontalAlignment = HorizontalAlignment.Stretch;
            var scrollViewer = GetScrollViewer(dgExpected);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= dgRealized_ScrollChanged;
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer)
                return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        #endregion

        private void btnRecalculateDay_Click(object sender, RoutedEventArgs e)
        {
            CheckNewDataDay?.Invoke(this, new CheckDateEventArgs(date));
        }

        private void tglButton_Click(object sender, RoutedEventArgs e)
        {
            if (tglButton.IsChecked == false)
            {
                HideGrid();
            }
            else
            {
                ShowGrid();
            }
        }

        public void HideGrid()
        {
            if (gridItems.Visibility == Visibility.Collapsed)
                return;
            GridClosed?.Invoke(this, null);
            gridItems.Visibility = Visibility.Collapsed;
            tglButton.Content = "Show";
        }

        public void ShowGrid()
        {
            if (gridItems.Visibility == Visibility.Visible)
                return;
            GridOpened?.Invoke(this, null);
            gridItems.Visibility = Visibility.Visible;
            tglButton.Content = "Hide";

        }

        private void dgExpected_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0.0f && !hideExpected)
            {
                ScrollViewer sv = null;
                Type t = dgExpected.GetType();
                try
                {
                    sv = t.InvokeMember("InternalScrollHost", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, dgRealized, null) as ScrollViewer;
                    sv.ScrollToVerticalOffset(e.VerticalOffset);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void dgRealized_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0.0f && !hideExpected)
            {
                ScrollViewer sv = null;
                Type t = dgRealized.GetType();
                try
                {
                    sv = t.InvokeMember("InternalScrollHost", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, dgExpected, null) as ScrollViewer;
                    sv.ScrollToVerticalOffset(e.VerticalOffset);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void gridItems_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Get the current vertical offset and scroll viewer
            var scrollViewer = GetScrollViewer(dgExpected);
            if (scrollViewer == null)
                return;

            double initialVerticalOffset = scrollViewer.VerticalOffset;

            // Check if we can scroll up or down in myDataGrid
            bool canScrollUp = initialVerticalOffset > 0;
            bool canScrollDown = initialVerticalOffset < scrollViewer.ScrollableHeight;

            if ((e.Delta > 0 && !canScrollUp) || (e.Delta < 0 && !canScrollDown))
            {
                var parentScrollViewer = FindParentScrollViewer(this); // Method to find parent stack pannel
                var v = parentScrollViewer.Name;

                // If we can't scroll, trigger scrolling in the parent DataGrid
                if (e.Delta > 0)
                {
                    // Scroll up in the parent
                    parentScrollViewer.ScrollToVerticalOffset(parentScrollViewer.VerticalOffset - 20); // Adjust scroll amount as needed
                }
                else
                {
                    // Scroll down in the parent
                    parentScrollViewer.ScrollToVerticalOffset(parentScrollViewer.VerticalOffset + 20); // Adjust scroll amount as needed

                }
            }
        }

        private ScrollViewer FindParentScrollViewer(DependencyObject child)
        {
            while (child != null)
            {
                if (child is ScrollViewer parentPannel)
                {
                    return parentPannel;
                }
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        #region Edit cells
        // for getting name of column that will be edited
        private void dgRealized_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the DataGrid
            DataGrid dataGrid = dgRealized;

            // Get the clicked cell
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // Traverse the visual tree to find the DataGridCell
            while (dep != null && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            DataGridCell cell = dep as DataGridCell;

            // Get the column of the cell
            DataGridColumn column = cell.Column;


            // For editing row on click
            var originalSource = e.OriginalSource as DependencyObject;

            // Find the DataGridRow that was clicked
            var row = FindParent<DataGridRow>(originalSource);

            if (row != null)
            {
                // Check if the clicked row is already selected
                if (dataGrid.SelectedItems.Contains(row.Item))
                {
                    // Prevent the default behavior
                    //e.Handled = true;

                    // Set the cell to edit mode
                    dataGrid.CurrentCell = new DataGridCellInfo(row.Item, column);

                    dataGrid.BeginEdit();
                }
            }

            if (column is DataGridTextColumn textColumn)
            {
                // Get the column header
                string columnHeader = textColumn.Header.ToString();
                this.columnToEdit = columnHeader;
            }

        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            var parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }



        #endregion

        private void dgRealized_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!canEdit)
                return;


            // Get cell value before editing
            var dataGrid = sender as DataGrid;
            var row = e.Row.Item;
            var mediaPlanRealized = row as MediaPlanRealized;

            var column = e.Column;
            oldCellValue = -1.0M;

            var cellInfo = new DataGridCellInfo(row, column);
            var cellContent = column.GetCellContent(row) as TextBlock;

            if (cellContent != null)
            {
                var originalValue = cellContent.Text;
                if (!decimal.TryParse(originalValue, out oldCellValue))
                {
                    return;
                }
            }
        }

        private void dgRealized_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var row = e.Row.Item;
            var column = e.Column;

            var cellInfo = new DataGridCellInfo(row, column);
            var editingElement = e.EditingElement as TextBox;           

            var mediaPlanRealized = e.Row.Item as MediaPlanRealized;

            if (editingElement != null && mediaPlanRealized != null)
            {
                var newValue = editingElement.Text;
                if (!decimal.TryParse(newValue, out decimal result) || 
                    mediaPlanRealized.Accept.Value ||
                    Math.Abs(result - oldCellValue) < 0.001M)
                {
                    // Returning old value to cell
                    editingElement.Text = oldCellValue.ToString();
                    
                }
                else
                {
                    UpdateSelectedCells(result);
                }
            }
        }

        private void UpdateSelectedCells(decimal newValue)
        {

            foreach (MediaPlanRealized mpR in dgRealized.SelectedItems)
            {
                if (mpR.status == -1 ||
                    (mpR.Accept != null && mpR.Accept.Value))
                    continue;

                bool isChangedCoef = false;
                bool progcoefChanged = false;
                /*switch (columnToEdit)
                {
                isChangedCoef = true;
                    case "Ch coef": mpR.Chcoef = newValue; break;
                    case "Dp coef": mpR.Dpcoef = newValue; break;
                    case "Prog coef": mpR.Progcoef = newValue; progcoefChanged = true;  break;
                    case "Seas coef": mpR.Seascoef = newValue; break;
                    case "Sec coef": mpR.Seccoef = newValue; break;
                    case "Coef A": mpR.CoefA = newValue; break;
                    case "Coef B": mpR.CoefB = newValue; break;
                    case "Cbr coef": mpR.Cbrcoef = newValue; break;
                    default: isChangedCoef = false; break;
                }*/

                if (string.Compare(columnToEdit, "Prog coef") == 0)
                {
                    mpR.Progcoef = newValue;
                    progcoefChanged = true;
                }
                else if (string.Compare(columnToEdit, "Coef A") == 0)
                {
                    mpR.CoefA = newValue;
                    isChangedCoef = true;

                }
                else if (string.Compare(columnToEdit, "Coef B") == 0)
                {
                    mpR.CoefB = newValue;
                    isChangedCoef = true;

                }
                /*else if (string.Compare(columnToEdit, "Ch coef") == 0)
                {
                    mpR.Chcoef = newValue;
                    isChangedCoef = true;
                }
                else if (string.Compare(columnToEdit, "Dp coef") == 0)
                {
                    mpR.Dpcoef = newValue;
                    isChangedCoef = true;
                }
                else if (string.Compare(columnToEdit, "Seas coef") == 0)
                {
                    mpR.Seascoef = newValue;
                    isChangedCoef = true;

                }
                else if (string.Compare(columnToEdit, "Sec coef") == 0)
                {
                    mpR.Seccoef = newValue;
                    isChangedCoef = true;

                }
                else if (string.Compare(columnToEdit, "Cbr coef") == 0)
                {
                    mpR.Cbrcoef = newValue;
                    isChangedCoef = true;
                }*/

                if (isChangedCoef)
                    UpdatedMediaPlanRealized?.Invoke(this, new UpdateMediaPlanRealizedEventArgs(mpR, true, false));

                if (progcoefChanged)
                    ProgcoefChangedMediaPlanRealized?.Invoke(this, new UpdateMediaPlanRealizedEventArgs(mpR, true, false));
            }

        }

        #region Edit date

        public event EventHandler<CompletedValidationEventArgs> CompletedValidationChanged;

        private void chbCompleted_Checked(object sender, RoutedEventArgs e)
        {
            string dateString = TimeFormat.DateOnlyToYMDString(date);
            CompletedValidationChanged?.Invoke(this, new CompletedValidationEventArgs(dateString, true));
            SetCompleted(true);
        }

        private void chbCompleted_Unchecked(object sender, RoutedEventArgs e)
        {
            string dateString = TimeFormat.DateOnlyToYMDString(date);
            CompletedValidationChanged?.Invoke(this, new CompletedValidationEventArgs(dateString, false));
            SetCompleted(false);
        }

        private void SetCompleted(bool isCompleted, bool setCheckbox = false)
        {
            if (isCompleted)
            {
                canEdit = false;
                dgRealized.IsManipulationEnabled = false;
                dgRealized.PreviewTextInput += DgRealized_PreviewTextInput;
                // Change setter
                SetHitTestVisible(false);
            }
            else
            {
                canEdit = true;
                dgRealized.IsManipulationEnabled = true;
                dgRealized.PreviewTextInput -= DgRealized_PreviewTextInput;

                // Change setter
                SetHitTestVisible(true);


            }

            if (setCheckbox)
            {
                chbCompleted.Checked -= chbCompleted_Checked;
                chbCompleted.Unchecked -= chbCompleted_Unchecked;

                chbCompleted.IsChecked = isCompleted;

                chbCompleted.Checked += chbCompleted_Checked;
                chbCompleted.Unchecked += chbCompleted_Unchecked;
            }
        }

        private void SetHitTestVisible(bool isVisible)
        {
            var checkBoxColumn = dgRealized.Columns
                .OfType<DataGridCheckBoxColumn>()
                .FirstOrDefault(col => col.Header.ToString() == "Accept");

            if (checkBoxColumn != null)
            {
                // Create a new style based on the current ElementStyle
                var newStyle = new Style(typeof(CheckBox), checkBoxColumn.ElementStyle);

                // Find the IsHitTestVisible setter if it exists
                var existingSetter = newStyle.Setters
                    .OfType<Setter>()
                    .FirstOrDefault(s => s.Property == UIElement.IsHitTestVisibleProperty);

                if (existingSetter != null)
                {
                    // Update the value of the existing setter
                    existingSetter.Value = isVisible;
                }
                else
                {
                    // Add the IsHitTestVisible setter if it does not exist
                    newStyle.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, isVisible));
                }

                // Apply the new style to the column
                checkBoxColumn.ElementStyle = newStyle;
            }
        }

        // Function to prevent editing cells
        private void DgRealized_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // To prevent copy-paste mechanism
        private void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == ApplicationCommands.Cut ||
                 e.Command == ApplicationCommands.Copy ||
                 e.Command == ApplicationCommands.Paste)
            {

                e.CanExecute = false;
                e.Handled = true;

            }

        }



        #endregion

        private void dgExpected_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = (SharedColumnsWidthViewModel)DataContext;
            // Ensure ColumnWidths is initialized and has the correct number of columns
            if (viewModel.ExpectedColumnWidthViewModel.ColumnWidths == null || viewModel.ExpectedColumnWidthViewModel.ColumnWidths.Count() != dgExpected.Columns.Count)
            {
                viewModel.ExpectedColumnWidthViewModel.ColumnWidths = new double[dgExpected.Columns.Count];
            }

            bool update = false;
            for (int i=0; i<dgExpected.Columns.Count; i++)
            {
                var column = dgExpected.Columns[i];
                if (column.Width.DisplayValue > viewModel.ExpectedColumnWidthViewModel.ColumnWidths[i])
                {
                    viewModel.ExpectedColumnWidthViewModel.ColumnWidths[i] = column.Width.DisplayValue;
                    update = true;
                    DgExpectedColumnWidthChanged?.Invoke(this, new ColumnWidthChangedEventArgs(column.Width.DisplayValue, i));
                }
            }
            if (update)
            {
                viewModel.ExpectedColumnWidthViewModel.OnPropertyChanged(nameof(viewModel.ExpectedColumnWidthViewModel.ColumnWidths));
            }
        }

        private void dgRealized_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = (SharedColumnsWidthViewModel)DataContext;
            if (viewModel.RealizedColumnWidthViewModel.ColumnWidths == null || viewModel.RealizedColumnWidthViewModel.ColumnWidths.Count() != dgRealized.Columns.Count)
            {
                viewModel.RealizedColumnWidthViewModel.ColumnWidths = new double[dgRealized.Columns.Count];
            }
            bool update = false;
            for (int i = 0; i < dgRealized.Columns.Count; i++)
            {
                var column = dgRealized.Columns[i];
                if (column.Width.DisplayValue > viewModel.RealizedColumnWidthViewModel.ColumnWidths[i])
                {
                    viewModel.RealizedColumnWidthViewModel.ColumnWidths[i] = column.Width.DisplayValue;
                    update = true;
                    DgRealizedColumnWidthChanged?.Invoke(this, new ColumnWidthChangedEventArgs(column.Width.DisplayValue, i));

                }
            }

            if (update)
            {
                viewModel.RealizedColumnWidthViewModel.OnPropertyChanged(nameof(viewModel.RealizedColumnWidthViewModel.ColumnWidths));
            }
        }

        public void SetRealizedColumnWidth(double width, int index)
        {
            dgRealized.Columns[index].Width = width;
        }
        public void SetExpectedColumnWidth(double width, int index)
        {
            dgExpected.Columns[index].Width = width;
        }

        public void SetWidthExpected(double width)
        {
            dgExpected.SizeChanged -= dgExpected_SizeChanged;
            dgExpected.Width = width;
            dgExpected.SizeChanged += dgExpected_SizeChanged;

        }

        private void tcRAccept_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;

            var mpRealized = checkBox.DataContext as MediaPlanRealized;
            if (mpRealized != null)
            {
                RealizedAcceptChanged(mpRealized, true);
            }

        }

        private void tcRAccept_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;

            var mpRealized = checkBox.DataContext as MediaPlanRealized;
            if (mpRealized != null)
            {
                RealizedAcceptChanged(mpRealized, false);
            }
        }

        private void RealizedAcceptChanged(MediaPlanRealized mpR, bool value)
        {
            // Handle x and y key press
            mpR.Accept = value;
            UpdatedMediaPlanRealized?.Invoke(this, new UpdateMediaPlanRealizedEventArgs(mpR, false, false));
        }


    }
}
