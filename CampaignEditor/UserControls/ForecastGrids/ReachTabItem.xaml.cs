using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ReachDTO;
using Database.DTOs.TargetDTO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private int _segins = 20;
        private int _segbet = 60;

        private CampaignDTO _campaign = null;
        private List<TargetDTO> _targets = new List<TargetDTO>();

        public event EventHandler<UpdateReachEventArgs> UpdateReach;
        public ReachTabItem()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign, List<TargetDTO> targets)
        {
            _campaign = campaign;
            _targets = targets.ToList();
            reachGrid.SetTargets(targets);  
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
            ReachDTO reach = null;
            try
            {
                await _databaseFunctionsController.StartReachCalculation(_campaign.cmpid, _segins, _segbet, true, true, null);
                reach = await _reachController.GetFinalReachByCmpid(_campaign.cmpid);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error while recalculating data: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            loadingGrid.Visibility = Visibility.Collapsed;
            visibleGrid.Visibility = Visibility.Visible;

            if (reach == null)
                return;

            reachGrid.SetReach(reach);
            UpdateReach?.Invoke(this, new UpdateReachEventArgs(reach));
        }

        private async void btnRecalculateReach_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSeginsSegbet())
            {
                return;
            }
            await RecalculateReach();
        }

        private async void btnExportReach_Click(object sender, RoutedEventArgs e)
        {
       
            if (!CheckSeginsSegbet())
            {
                return;
            }

            string? destinationPath = GetDestinationPath();
            if (destinationPath == null)
                return;

            int cmpid = _campaign.cmpid;
            bool delete = true;
            bool expr = true;
            string fileName = Path.GetFileName(destinationPath);

            ReachFileManipulation fileManipulation = new ReachFileManipulation(fileName);
            try
            {
                string sourcePath = fileManipulation.GetSourcePath();
                await _databaseFunctionsController.StartReachCalculation(cmpid, _segins, _segbet, delete, expr, fileManipulation.FunctionPath);
                fileManipulation.MoveFileToPath(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot calculate reach!\n" + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private string GetDestinationPath()
        {
            // Create SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = $"pln-{RemoveWhiteSpaces(_campaign.cmpname.Trim())}_" + DateTime.Now.ToString("ddMM_HHmm");
            saveFileDialog.DefaultExt = ".pln";
            saveFileDialog.Filter = "PLN files (*.pln)|*.pln|All files (*.*)|*.*";

            // Show the dialog
            bool? result = saveFileDialog.ShowDialog();

            // Check if a location was selected
            if (result == true)
            {
                // Get the selected file path
                string filePath = saveFileDialog.FileName;

                return filePath;
            }
            else
            {
                return null;
            }
        }

        public string RemoveWhiteSpaces(string input)
        {
            string result = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return result;
        }       

        private bool CheckSeginsSegbet()
        {
            int segins;
            int segbet;

            if (!(int.TryParse(tbSegins.Text, out segins) && int.TryParse(tbSegbet.Text, out segbet)))
            {
                MessageBox.Show("Invalid values for segins or segbet!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            _segins = segins;
            _segbet = segbet;
            return true;
        }
    }
}
