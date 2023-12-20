using AutoMapper;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Helpers;
using CampaignEditor.UserControls;
using CampaignEditor.UserControls.ForecastGrids;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CampaignEditor
{
    /// <summary>
    /// Class for selecting what to export to Excel
    /// </summary>
    public partial class PrintForecast : Window
    {
        public ListView lvChannels;
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
        private readonly IChannelRepository _channelRepository;

        ObservableCollection<Channel> customChannels = new ObservableCollection<Channel>();
        private bool[] visibleColumns = new bool[29];

        public PrintForecast(IMapper mapper)
        {
            InitializeComponent();

            _mapper = mapper;
        }

        public void Initialize()
        {
            FillChannels();
            FillWpColumns();
        }

        private void FillChannels()
        {
            customChannels.Clear();

            foreach (ChannelDTO channelDTO in lvChannels.Items)
            {
                var channel = _mapper.Map<Channel>(channelDTO);
                customChannels.Add(channel);
            }

            lvCustomChannels.ItemsSource = customChannels;
        }
        private void FillWpColumns()
        {
            wpColumns.Children.Clear();

            List<string> columnHeaders = new List<string> {"Channel", "Program", "Position", "Start time",
            "End time", "Block time", "Type", "Special", "Amr1", "Amr% 1", "Amr1 Trim", "Amr2", "Amr% 2", "Amr2 Trim", 
            "Amr3", "Amr% 3","Amr3 Trim", "Amr sale", "Amr% sale", "Amr sale Trim", 
            "Affinity","Prog coef", "Dp coef", "Seas coef", "Sec coef", "CPP", "Ins", "CPSP", "Price"};
            
            int numOfColumns = columnHeaders.Count;

            for (int i=0; i<numOfColumns; i++)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Content = columnHeaders[i];
                checkBox.VerticalContentAlignment = VerticalAlignment.Center;
                wpColumns.Children.Add(checkBox);
            }

        }

        public void CheckFields()
        {
            CheckCustomChannels();
            CheckCustomColumns();

        }

        private void CheckCustomChannels()
        {
            foreach (Channel channel in lvCustomChannels.Items)
            {
                channel.IsSelected = false;
            }
            foreach (ChannelDTO channel in lvChannels.SelectedItems)
            {
                // selecting channels in lvCustomChannels
                foreach (Channel customChannel in lvCustomChannels.Items)
                {
                    if (channel.chid == customChannel.chid)
                    {
                        customChannel.IsSelected = true;
                    }
                }
            }
        }

        private void CheckCustomColumns()
        {
            for (int i=0; i<mpGrid.mediaPlanColumns; i++)
            {
                var column = mpGrid.dgMediaPlans.Columns[i];
                if (column.Visibility == Visibility.Visible)
                {
                    visibleColumns[i] = true;
                }
                else
                {
                    visibleColumns[i] = false;
                }
            }

            for (int i=0; i<visibleColumns.Count(); i++)
            {
                CheckBox checkBox = wpColumns.Children[i] as CheckBox;
                if (visibleColumns[i] == true)
                {
                    checkBox.IsChecked = true;
                }
                else
                {
                    checkBox.IsChecked = false;
                }
            }
        }

        private List<ChannelDTO> GetSelectedChannels()
        {
            var selectedChannels = lvCustomChannels.Items.Cast<Channel>().Where(ch => ch.IsSelected).ToList();

            return _mapper.Map<IEnumerable<ChannelDTO>>(selectedChannels).ToList();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintForecastGrids();
            this.Hide();
        }

        private void PrintForecastGrids()
        {
            var selectedChannels = new List<ChannelDTO>();

            if (rbCustomChannels.IsChecked == true)
            {
                foreach (ChannelDTO channel in lvChannels.SelectedItems)
                {
                    selectedChannels.Add(channel);
                }
            }
            else if (rbCustomChannels.IsChecked == true)
            {
                selectedChannels = GetSelectedChannels();
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(async () =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    using (var excelPackage = new ExcelPackage(memoryStream))
                    {

                        // Create a new worksheet
                        var worksheet0 = excelPackage.Workbook.Worksheets.Add("Campaign Info");
                        await factoryPrintCmpInfo.PrintData(_campaign.cmpid, selectedChannels, worksheet0, 0, 0);

                        if (chbMpGrid.IsChecked == true)
                        {
                            var worksheet1 = excelPackage.Workbook.Worksheets.Add("Program Schema");
                            mpGrid.PopulateWorksheet(worksheet1, 0, 0);
                        }
                        if (chbWeeks1.IsChecked == true)
                        {
                            var worksheet2 = excelPackage.Workbook.Worksheets.Add("By Weeks 1");
                            sgGrid.PopulateWorksheet(worksheet2, 1, 1);
                        }
                        if (chbWeeks2.IsChecked == true)
                        {
                            var worksheet3 = excelPackage.Workbook.Worksheets.Add("By Weeks 2");
                            swgGrid.PopulateWorksheet(worksheet3, 1, 1);
                        }
                        if (chbDays.IsChecked == true)
                        {
                            var worksheet4 = excelPackage.Workbook.Worksheets.Add("By Days");
                            sdgGrid.PopulateWorksheet(worksheet4, 1, 1);
                        }
                        if (chbChannels.IsChecked == true)
                        {
                            var worksheet5 = excelPackage.Workbook.Worksheets.Add("Channel Goals");
                            cgGrid.PopulateWorksheet(selectedChannels, worksheet5, 1, 1);
                        }
                        if (chbListing.IsChecked == true)
                        {
                            var worksheet6 = excelPackage.Workbook.Worksheets.Add("Spot listing");
                            var list = _allMediaPlans.Where(mpTuple => mpTuple.MediaPlan.Insertations > 0 && selectedChannels.Any(ch => ch.chid == mpTuple.MediaPlan.chid)).ToList();
                            await factoryListing.PopulateWorksheet(list, worksheet6, 0, 0);
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

                }

            }));
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
