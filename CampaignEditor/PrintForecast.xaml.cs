using AutoMapper;
using Database.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.UserControls;
using CampaignEditor.UserControls.ForecastGrids;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Class for selecting what to export to Excel
    /// </summary>
    public partial class PrintForecast : Window
    {
        public ObservableRangeCollection<ChannelDTO> _selectedChannels;
        public ChannelsGoalsGrid cgGrid;
        public MediaPlanGrid mpGrid;
        public SpotDaysGoalsGrid sdgGrid;
        public SpotGoalsGrid sgGrid;
        public SpotWeekGoalsGrid swgGrid;
        public Listing factoryListing;
        public PrintCampaignInfo factoryPrintCmpInfo;

        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;
        public CampaignDTO _campaign;

        public bool shouldClose = false;

        private readonly IMapper _mapper;

        private bool[] visibleColumns = new bool[30];
        public List<MediaPlanTuple> visibleTuples = new List<MediaPlanTuple>();



        public PrintForecast(IMapper mapper)
        {
            InitializeComponent();

            _mapper = mapper;
        }

        public void MakeVisibleColumnsMask(bool hideSensitiveData = false)
        {
            for (int i=0; i<mpGrid.mediaPlanColumns; i++)
            {
                var column = mpGrid.dgMediaPlans.Columns[i];
                if (column.Visibility == Visibility.Visible)
                {
                    visibleColumns[i] = true;
                    if (hideSensitiveData && CheckIfColumnIsSensitive(column))
                    {
                        visibleColumns[i] = false;
                    }
                }
                else
                {
                    visibleColumns[i] = false;
                }
            }

        }

        private bool CheckIfColumnIsSensitive(DataGridColumn column)
        {
            string? headerName = column.Header.ToString();
            if (headerName == null)
                return true;

            if (headerName == "Channel" || headerName == "Program" 
                || headerName == "Position" || headerName == "Start time" || headerName == "End time" 
                || headerName == "Block time" || headerName == "Type" || headerName == "Special")
                return false;

            return true;
        }

        private async void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            bool hideSensitiveData = (bool)chbHideSensitiveData.IsChecked;
            MakeVisibleColumnsMask(hideSensitiveData);
            await PrintForecastGrids(hideSensitiveData);
            this.Hide();
        }

        private async Task PrintForecastGrids(bool hideSensitiveData = false)
        {
            var selectedChannels = new List<ChannelDTO>();
            foreach (ChannelDTO channel in _selectedChannels)
            {
                selectedChannels.Add(channel);
            }
            bool showAllDecimals = chbShowAllDecimals.IsChecked == true;

            using (var memoryStream = new MemoryStream())
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    // Create a new worksheet
                    var worksheet0 = excelPackage.Workbook.Worksheets.Add("Campaign Info");
                    await factoryPrintCmpInfo.PrintData(_campaign.cmpid, selectedChannels, worksheet0, 0, 0, hideSensitiveData);

                    if (chbMpGrid.IsChecked == true)
                    {
                        var worksheet1 = excelPackage.Workbook.Worksheets.Add("Program Schema");
                        mpGrid.PopulateWorksheet(visibleTuples, visibleColumns, worksheet1, 1, 1, showAllDecimals);
                    }
                    if (chbWeeks1.IsChecked == true)
                    {
                        var worksheet2 = excelPackage.Workbook.Worksheets.Add("By Weeks 1");
                        sgGrid.PopulateWorksheet(worksheet2, 1, 1, showAllDecimals);
                    }
                    if (chbWeeks2.IsChecked == true)
                    {
                        var worksheet3 = excelPackage.Workbook.Worksheets.Add("By Weeks 2");
                        swgGrid.PopulateWorksheet(worksheet3, 1, 1, showAllDecimals);
                    }
                    if (chbDays.IsChecked == true)
                    {
                        var worksheet4 = excelPackage.Workbook.Worksheets.Add("By Days");
                        sdgGrid.PopulateWorksheet(worksheet4, 1, 1, showAllDecimals);
                    }
                    if (chbChannels.IsChecked == true)
                    {
                        var worksheet5 = excelPackage.Workbook.Worksheets.Add("Channel Goals");
                        cgGrid.PopulateWorksheet(selectedChannels, worksheet5, 1, 1, showAllDecimals);
                    }
                    if (chbListing.IsChecked == true)
                    {
                        var worksheet6 = excelPackage.Workbook.Worksheets.Add("Spot listing");
                        //var list = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.Insertations > 0 && selectedChannels.Any(ch => ch.chid == mpTuple.MediaPlan.chid)).ToList();
                        factoryListing.PopulateWorksheet(visibleTuples, visibleColumns, worksheet6, 1, 1, showAllDecimals);
                    }

                    // Save the Excel package to a memory stream
                    excelPackage.SaveAs(memoryStream);
                    // Set the position of the memory stream back to the beginning
                    memoryStream.Position = 0;

                    // Show a dialog to the user for saving or opening the Excel file
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx",
                        DefaultExt = "xlsx"
                    };

                    SaveFile(saveFileDialog, memoryStream);
                }

             };
        }

        private void SaveFile(SaveFileDialog saveFileDialog, MemoryStream memoryStream)
        {

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Save the memory stream to a file
                    File.WriteAllBytes(saveFileDialog.FileName, memoryStream.ToArray());

                    try
                    {
                        string filePath = saveFileDialog.FileName;

                        // Open the saved Excel file using the default associated program
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch
                    {
                        MessageBox.Show("Unable to open Excel file",
                        "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (IOException ex)
                {
                    MessageBox.Show("Unable to change opened Excel file",
                        "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                {
                    MessageBox.Show("Unable to make Excel file",
                        "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
                Hide();
            }

        }

    }
}
