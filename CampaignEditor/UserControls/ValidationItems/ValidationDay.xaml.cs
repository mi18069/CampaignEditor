using Database.Entities;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using CampaignEditor.Controllers;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using CampaignEditor.Helpers;

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

        public ValidationDay(DateOnly date,
            List<TermTuple> termTuples,
            List<MediaPlanRealized> mpRealizedTuples,
            bool[] expectedGridMask, bool[] realizedGridMask)
        {
            InitializeComponent();

            dgExpected.ItemsSource = null;
            dgRealized.ItemsSource = null;

            _termTuples = termTuples;
            _mpRealizedTuples = mpRealizedTuples;

            this.date = date;
            if (_termTuples.Count > 0)
                dgExpected.ItemsSource = _termTuples;
            if (_mpRealizedTuples.Count > 0)
                dgRealized.ItemsSource = _mpRealizedTuples;
            SetUserControl();
            SetGridMasks(expectedGridMask, realizedGridMask);
        }




        private void SetUserControl()
        {
            int exCount = _termTuples.Where(tt => tt != null && tt.Status != -1).Count();
            int realCount = _mpRealizedTuples.Where(rt => rt != null && rt.status != -1).Count();

            lblExCount.Content = exCount;
            lblRealCount.Content = realCount;
            lblDate.Content = date.ToShortDateString();

            if (exCount == 0 && realCount == 0)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }
            
        }

        private void SetGridMasks(bool[] expectedGridMask, bool[] realizedGridMask)
        {
            for (int i=0; i<expectedGridMask.Count(); i++)
            {
                dgExpected.Columns[i].Visibility = expectedGridMask[i] ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }

            for (int i = 0; i < realizedGridMask.Count(); i++)
            {
                dgRealized.Columns[i].Visibility = realizedGridMask[i] ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
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

        private void DgExpected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = dgExpected.SelectedIndex;
            dgRealized.SelectedIndex = index;
        }
        private void DgRealized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = dgRealized.SelectedIndex;
            dgExpected.SelectedIndex = index;
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


    }
}
