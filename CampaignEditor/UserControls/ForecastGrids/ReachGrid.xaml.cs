using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ReachDTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// Interaction logic for ReachGrid.xaml
    /// </summary>
    public partial class ReachGrid : UserControl
    {
        public ReachGrid()
        {
            InitializeComponent();
            ApplyCellStyle(dgGridFirst);
            ApplyCellStyle(dgGridSecond);
        }

        public void SetReach(ReachDTO reach)
        {
            List<ReachDTO> reachList = new List<ReachDTO>
            {
                reach
            };
            dgGridFirst.ItemsSource = reachList;
            dgGridSecond.ItemsSource = reachList;
        }

        private void DataGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up ||
                e.Key == System.Windows.Input.Key.Down)
            {
                ChangeFocusedRow();
                e.Handled = true;
            }
        }

        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender == dgGridFirst)
            {
                dgGridSecond.SelectedItem = null;
            }
            else
            {
                dgGridFirst.SelectedItem = null;
            }
        }

        private void ChangeFocusedRow()
        {
            if (dgGridFirst.SelectedItems.Count > 0)
            {
                dgGridFirst.SelectedItem = null;
                dgGridSecond.SelectedIndex = 0;
                dgGridSecond.Focus();
            }
            else if (dgGridSecond.SelectedItems.Count > 0)
            {
                dgGridFirst.SelectedItem = null;
                dgGridFirst.SelectedIndex = 0;
                dgGridFirst.Focus();

            }
        }


        private void ApplyCellStyle(DataGrid dataGrid)
        {
            var cellStyle = new System.Windows.Style(typeof(DataGridCell));

            Trigger trigger = new Trigger
            {
                Property = DataGridCell.IsSelectedProperty,
                Value = true
            };

            Setter backgroundSetter = new Setter(DataGridCell.BackgroundProperty, System.Windows.Media.Brushes.LightBlue);
            Setter foregroundSetter = new Setter(DataGridCell.ForegroundProperty, System.Windows.Media.Brushes.Black);

            trigger.Setters.Add(backgroundSetter);
            trigger.Setters.Add(foregroundSetter);

            cellStyle.Triggers.Add(trigger);

            dataGrid.Resources.Add(typeof(DataGridCell), cellStyle);
        }

    }
}
