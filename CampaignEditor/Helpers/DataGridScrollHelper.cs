using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace CampaignEditor.Helpers
{
    public static class DataGridScrollHelper
    {
        public static readonly DependencyProperty SlowScrollProperty =
        DependencyProperty.RegisterAttached("SlowScroll", typeof(bool), typeof(DataGridScrollHelper), new PropertyMetadata(false, OnSlowScrollChanged));

        public static bool GetSlowScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(SlowScrollProperty);
        }

        public static void SetSlowScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(SlowScrollProperty, value);
        }

        private static void OnSlowScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.PreviewMouseWheel += DataGrid_PreviewMouseWheel;
                }
                else
                {
                    dataGrid.PreviewMouseWheel -= DataGrid_PreviewMouseWheel;
                }
            }
        }

        private static void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            var scrollViewer = FindVisualChild<ScrollViewer>(dataGrid);
            if (scrollViewer == null)
                return;

            // Adjust the scroll offset
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta * 0.01);
            e.Handled = true;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }
                else
                {
                    var descendant = FindVisualChild<T>(child);
                    if (descendant != null)
                    {
                        return descendant;
                    }
                }
            }
            return null;
        }
    }
}
