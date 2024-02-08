using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.Entities;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ForecastGrids
{
    /// <summary>
    /// User control under TabItem for calculating reach
    /// </summary>
    public partial class ReachTabItem : UserControl
    {

        public DatabaseFunctionsController _databaseFunctionsController;
        public ReachController _reachController;

        private CampaignDTO _campaign = null;
        public ReachTabItem()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            await GetReach();
        }

        private async Task GetReach()
        {
            var reach = await _reachController.GetFinalReachByCmpid(_campaign.cmpid);
            if (reach == null)
            {
                await RecalculateReach();
            }
            else
            {
                reachGrid.SetReach(reach);
            }
        }

        private async Task RecalculateReach()
        {
            visibleGrid.Visibility = Visibility.Collapsed;
            loadingGrid.Visibility = Visibility.Visible;

            await _databaseFunctionsController.StartReachCalculation(_campaign.cmpid, 20, 60, true, true, null);
            var reach = await _reachController.GetFinalReachByCmpid(_campaign.cmpid);

            loadingGrid.Visibility = Visibility.Collapsed;
            visibleGrid.Visibility = Visibility.Visible;

            reachGrid.SetReach(reach);
        }

        private async void btnRecalculateReach_Click(object sender, RoutedEventArgs e)
        {
            await RecalculateReach();
        }

        private async void btnExportReach_Click(object sender, RoutedEventArgs e)
        {
            int segint;
            int segbet;

            if (!(int.TryParse(tbSegins.Text, out segint) && int.TryParse(tbSegbet.Text, out segbet)))
            {
                MessageBox.Show("Invalid values for segint or segbet!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            /*var folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog().GetValueOrDefault())
            {
                int cmpid = _campaign.cmpid;
                bool delete = true;
                bool expr = true;
                string folderPath = folderDialog.SelectedPath.ToString();
                string? path = CreateFile();

                if (path == null)
                    return;

                try
                {
                    await _databaseFunctionsController.StartReachCalculation(cmpid, segint, segbet, delete, expr, path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot calculate reach!\n" + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }*/

            string? path = CreateAndSaveFile();
            if (path == null)
                return;

            int cmpid = _campaign.cmpid;
            bool delete = true;
            bool expr = true;

            try
            {
                await _databaseFunctionsController.StartReachCalculation(cmpid, segint, segbet, delete, expr, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot calculate reach!\n" + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private string CreateAndSaveFile()
        {
            // Create SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMdd_HHmm");
            saveFileDialog.DefaultExt = ".pln";
            saveFileDialog.Filter = "PLN files (*.pln)|*.pln|All files (*.*)|*.*";

            // Show the dialog
            bool? result = saveFileDialog.ShowDialog();

            // Check if a location was selected
            if (result == true)
            {
                // Get the selected file path
                string filePath = saveFileDialog.FileName;

                // Create the file
                //File.Create(filePath).Close();

                return filePath;
            }
            else
            {
                return null;
            }
        }

        private string? CreateFile(string folderPath)
        {
            string currentTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Combine the current time with the desired file extension (e.g., .pln)
            string fileName = $"{currentTime}.pln";

            // Specify the path where you want to create the file
            string filePath = Path.Combine(folderPath, fileName);

            // Create the file
            try
            {
                File.Create(filePath).Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create .pln file!\n" + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return filePath;
        }
    }
}
