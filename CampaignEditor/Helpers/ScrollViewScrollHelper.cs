using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace CampaignEditor.Helpers
{
    public static class ScrollViewScrollHelper
    {
        public static readonly DependencyProperty SlowScrollProperty =
        DependencyProperty.RegisterAttached("SlowScroll", typeof(bool), typeof(ScrollViewScrollHelper), new PropertyMetadata(false, OnSlowScrollChanged));

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
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += ScrollView_PreviewMouseWheel;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollView_PreviewMouseWheel;
                }
            }
        }

        private static void ScrollView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null)
                return;

            // Adjust the scroll offset
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta * 0.15);
            e.Handled = true;
        }
    }
}
