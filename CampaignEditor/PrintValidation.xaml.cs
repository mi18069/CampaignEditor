using CampaignEditor.UserControls.ValidationItems;
using Database.DTOs.CampaignDTO;
using Database.Entities;
using Microsoft.Win32;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for PrintValidation.xaml
    /// </summary>
    public partial class PrintValidation : Window
    {

        CampaignDTO _campaign;
        public string DgExpectedMask { get; set; }
        public string DgRealizedMask { get; set; }

        public List<DateOnly> _dates = new List<DateOnly>();
        public List<DateOnly> datesToPrint = new List<DateOnly>();
        public UIElementCollection ValidationDays { get; set; }
        public Dictionary<DateOnly, List<TermTuple?>> _dateExpectedDict;
        public Dictionary<DateOnly, List<MediaPlanRealized?>> _dateRealizedDict;

        public Dictionary<DateOnly, ValidationDay> ValidationDaysDict;


        public PrintValidation()
        {
            InitializeComponent();
        }

        public void Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            SetDates(campaign);
        }

        private void SetDates(CampaignDTO campaign)
        {
            dpFrom.DisplayDateStart = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
            dpFrom.DisplayDateEnd = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

            dpTo.DisplayDateStart = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
            dpTo.DisplayDateEnd = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

            dpOneDay.DisplayDateStart = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
            dpOneDay.DisplayDateEnd = TimeFormat.YMDStringToDateTime(campaign.cmpedate);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            bool allDecimals = (bool)chbAllDecimals.IsChecked;
            bool printExpected = (bool)chbPrintExpected.IsChecked;
            bool printRealized = (bool)chbPrintRealized.IsChecked;
            bool separateByDays = (bool)chbDateSeparator.IsChecked;
            SetDatesToPrint();
            Print(allDecimals, printExpected, printRealized, separateByDays);
        }

        private void SetDatesToPrint()
        {
            datesToPrint.Clear();
            if ((bool)rbAllDays.IsChecked)
            {
               foreach (var date in _dates)
               {
                    datesToPrint.Add(date);
               }
            }
            else if ((bool)rbOneDay.IsChecked)
            {
                if (!dpOneDay.SelectedDate.HasValue)
                {
                    MessageBox.Show("Select one date!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                datesToPrint.Add(DateOnly.FromDateTime(dpOneDay.SelectedDate.Value));
            }
            else
            {
                if (!dpFrom.SelectedDate.HasValue)
                {
                    MessageBox.Show("Select starting date!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!dpTo.SelectedDate.HasValue)
                {
                    MessageBox.Show("Select ending date!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dpFrom.SelectedDate > dpTo.SelectedDate)
                {
                    MessageBox.Show("Starting date needs to be before ending date!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                for (var date = dpFrom.SelectedDate.Value; date <= dpTo.SelectedDate.Value; date = date.AddDays(1))
                {
                    datesToPrint.Add(DateOnly.FromDateTime(date));
                }
            }
        }

        #region Print

        public void Print(bool allDecimals, bool printExpected, bool printRealized, bool separateByDays)
        {

            using (var memoryStream = new MemoryStream())
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    // Create a new worksheet
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Validation");
                    ValidationDay validationDay = ValidationDays[0] as ValidationDay;
                    if (validationDay == null)
                    {
                        return;
                    }

                    if (printExpected)
                    {
                        bool skipEmptyRows = false;
                        if (!printRealized)
                            skipEmptyRows = true;

                        var headerNamesExpected = validationDay.ExpectedGrid.Columns
                                .Select(column => column.Header.ToString())
                                .ToList();
                        AddHeaders(worksheet, 1, 1, headerNamesExpected, DgExpectedMask);
                        //AddExpected(worksheet, 2, 1, _dateExpectedDict, DgExpectedMask, allDecimals);
                        AddExpected(worksheet, 2, 1, ValidationDaysDict, DgExpectedMask, allDecimals, skipEmptyRows, separateByDays);
                    }
                    // if expected stack is hidden, don't place one extra column between them,
                    // else separate by width of 1 column

                    if (printRealized)
                    {
                        bool skipEmptyRows = false;
                        if (!printExpected)
                            skipEmptyRows = true;

                        int separationWidth = !printExpected ? 0 : 1 + DgExpectedMask.Count(e => e == '1');

                        var headerNamesRealized = validationDay.RealizedGrid.Columns
                                    .Select(column => column.Header.ToString())
                                    .ToList();

                        AddHeaders(worksheet, 1, 1 + separationWidth, headerNamesRealized, DgRealizedMask);
                        //AddRealized(worksheet, 2, 1 + separationWidth, _dateRealizedDict, DgRealizedMask, allDecimals);
                        AddRealized(worksheet, 2, 1 + separationWidth, ValidationDaysDict, DgRealizedMask, allDecimals, skipEmptyRows, separateByDays);
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

        private void AddHeaders(ExcelWorksheet worksheet, int rowOff, int colOff, List<string> headerNames, string headerMask)
        {

            int numOfColumns = headerMask.Count();

            // Set the column headers in Excel
            int colOffset = 0;
            for (int i = 0; i < numOfColumns; i++)
            {
                if (headerMask[i] == '1')
                {
                    worksheet.Cells[rowOff, colOff + colOffset].Value = headerNames[i];
                    worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                    colOffset += 1;
                }

            }
        }

        private void AddExpected(ExcelWorksheet worksheet, int rowOff, int colOff, Dictionary<DateOnly, ValidationDay> validationDaysDict, string mask, bool allDecimals = false, bool skipEmptyRows = false, bool separateByDays = false)
        {
            int colNum = mask.Count(c => c == '1');
            foreach (var date in datesToPrint)
            {
                List<TermTuple> termTuples = null;
                try
                {
                    var validationDay = validationDaysDict[date];
                    termTuples = validationDay.GetViewExpected;
                }
                catch
                {
                    continue;
                }

                if (separateByDays)
                {
                    //worksheet.Cells[rowOff, colOff].Value = date.ToShortDateString();
                    worksheet.Cells[rowOff, colOff].Value = date;
                    // last +1 for coloring the next empty row to look homogenous
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1 + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1 + 1].Style.Fill.BackgroundColor.SetColor(SetColorByStatus(-2));
                    rowOff++;
                }
                foreach (var termTuple in termTuples)
                {
                    if (termTuple.Status == -1)
                    {
                        if (!skipEmptyRows) 
                            rowOff++;
                        continue;
                    }
                    int colOffset = colOff;
                    if (mask[0] == '1')
                    {
                        //worksheet.Cells[rowOff, colOffset++].Value = termTuple.Date;
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.DateOnly;
                    }
                    if (mask[1] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.ChannelName.Trim();
                    }
                    if (mask[2] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.name.Trim();
                    }
                    if (mask[3] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.DayPart.name.Trim();
                    }
                    if (mask[4] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Blocktime;
                    }
                    if (mask[5] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Position;
                    }
                    if (mask[6] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Amrp1;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Amrp1.Value, 2);
                        }
                    }
                    if (mask[7] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Amrp2;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Amrp2.Value, 2);
                        }

                    }
                    if (mask[8] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Amrp3;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Amrp3.Value, 2);
                        }

                    }
                    if (mask[9] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Amrpsale;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Amrpsale.Value, 2);
                        }

                    }
                    if (mask[10] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Cpp;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Cpp.Value, 2);
                        }

                    }
                    if (mask[11] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Chcoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Chcoef.Value, 2);
                        }

                    }
                    if (mask[12] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Progcoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Progcoef.Value, 2);
                        }

                    }
                    if (mask[13] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Dpcoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Dpcoef.Value, 2);
                        }

                    }
                    if (mask[14] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Seascoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Seascoef.Value, 2);
                        }

                    }
                    if (mask[15] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Seccoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Seccoef.Value, 2);
                        }

                    }
                    if (mask[16] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.CoefA;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.CoefA.Value, 2);
                        }

                    }
                    if (mask[17] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.CoefB;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.CoefB.Value, 2);
                        }

                    }
                    if (mask[18] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.Spot.spotname.Trim();

                    }
                    if (mask[19] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.Spotlength;

                    }
                    if (mask[20] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = termTuple.Spot.spotcode;

                    }
                    if (mask[21] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = termTuple.Price;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(termTuple.Price.Value, 2);
                        }

                    }
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1].Style.Fill.BackgroundColor.SetColor(SetExpectedColorByStatus(termTuple.Status, termTuple.StatusAD));
                    rowOff++;

                }
            }


        }

        private void AddRealized(ExcelWorksheet worksheet, int rowOff, int colOff, Dictionary<DateOnly, ValidationDay> validationDaysDict, string mask, bool allDecimals = false, bool skipEmptyRows = false, bool separateByDays = false)
        {
            int colNum = mask.Count(c => c == '1');
            foreach (var date in datesToPrint)
            {
                List<MediaPlanRealized> realizedTuples = null;
                try
                {
                    var validationDay = validationDaysDict[date];
                    realizedTuples = validationDay.GetViewRealized;
                }
                catch
                {
                    continue;
                }

                if (separateByDays)
                {
                    //worksheet.Cells[rowOff, colOff].Value = date.ToShortDateString();
                    worksheet.Cells[rowOff, colOff].Value = date;
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1].Style.Fill.BackgroundColor.SetColor(SetColorByStatus(-2));
                    rowOff++;
                }
                
                foreach (var mediaPlanRealized in realizedTuples)
                {
                    if (mediaPlanRealized.status == -1)
                    {
                        if (!skipEmptyRows)
                            rowOff++;
                        continue;
                    }
                    int colOffset = colOff;
                    if (mask[0] == '1')
                    {
                        //worksheet.Cells[rowOff, colOffset++].Value = TimeFormat.YMDStringToDateOnly(mediaPlanRealized.Date).ToShortDateString();
                        worksheet.Cells[rowOff, colOffset++].Value = TimeFormat.YMDStringToDateOnly(mediaPlanRealized.Date);
                    }
                    if (mask[1] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Channel.chname.Trim();
                    }
                    if (mask[2] == '1')
                    {
                        var name = mediaPlanRealized.name.Trim() ?? "";
                        worksheet.Cells[rowOff, colOffset++].Value = name;
                    }
                    if (mask[3] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = TimeFormat.TimeStrToRepresentative(mediaPlanRealized.stimestr);
                    }
                    if (mask[4] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.durf;
                    }
                    if (mask[5] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.dure;
                    }
                    if (mask[6] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.spotname.Trim();
                    }
                    if (mask[7] == '1')
                    {
                        var amrp1 = mediaPlanRealized.amrp1;
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = amrp1;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(amrp1, 2);
                        }

                    }
                    if (mask[8] == '1')
                    {
                        var amrp2 = mediaPlanRealized.amrp2;
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = amrp2;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(amrp2, 2);
                        }

                    }
                    if (mask[9] == '1')
                    {
                        var amrp3 = mediaPlanRealized.amrp3;
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = amrp3;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(amrp3, 2);
                        }

                    }
                    if (mask[10] == '1')
                    {
                        var amrpsale = mediaPlanRealized.amrpsale;
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = amrpsale;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(amrpsale, 2);
                        }

                    }
                    if (mask[11] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Cpp;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.Cpp.Value, 2);
                        }

                    }
                    if (mask[12] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Chcoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.Chcoef.Value, 2);
                        }

                    }
                    if (mask[13] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Progcoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.Progcoef.Value, 2);
                        }

                    }
                    if (mask[14] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Dpcoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.Dpcoef.Value, 2);
                        }

                    }
                    if (mask[15] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Seascoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.Seascoef.Value, 2);
                        }

                    }
                    if (mask[16] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Seccoef;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.Seccoef.Value, 2);
                        }

                    }
                    if (mask[17] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.CoefA;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.CoefA.Value, 2);
                        }

                    }
                    if (mask[18] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.CoefB;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.CoefB.Value, 2);
                        }

                    }
                    if (mask[19] == '1')
                    {
                        if (allDecimals)
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.price;
                        }
                        else
                        {
                            worksheet.Cells[rowOff, colOffset++].Value = Math.Round(mediaPlanRealized.price.Value, 2);
                        }

                    }
                    if (mask[20] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.status;
                    }
                    if (mask[21] == '1')
                    {
                        worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.Accept;
                    }
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowOff, colOff, rowOff, colOff + colNum - 1].Style.Fill.BackgroundColor.SetColor(SetColorByStatus(mediaPlanRealized.status.Value));
                    rowOff++;

                }
            }


        }

        private System.Drawing.Color SetExpectedColorByStatus(int status, int statusad = 0)
        {
            switch (statusad)
            {
                case 1: return System.Drawing.Color.YellowGreen;
                case 2: return System.Drawing.Color.Gray;
                default: break;
            }

            switch (status)
            {
                case -2: return System.Drawing.Color.BlueViolet; // For row representing date
                case -1: return System.Drawing.Color.Transparent;
                case 0: return System.Drawing.Color.LightGray;
                case 1: return System.Drawing.Color.LightGreen;
                case 2: return System.Drawing.Color.OrangeRed;
                case 3: return System.Drawing.Color.Green;
                case 4: return System.Drawing.Color.Orange;
                case 5: return System.Drawing.Color.PaleVioletRed;
                case 6: return System.Drawing.Color.Violet;
                case 7: return System.Drawing.Color.Cyan;
                default: return System.Drawing.Color.Transparent;
            }
        }

        private System.Drawing.Color SetColorByStatus(int? status)
        {
            switch (status)
            {
                case -2: return System.Drawing.Color.BlueViolet; // For row representing date
                case -1: return System.Drawing.Color.Transparent;
                case 0: return System.Drawing.Color.LightGray;
                case 1: return System.Drawing.Color.LightGreen;
                case 2: return System.Drawing.Color.OrangeRed;
                case 3: return System.Drawing.Color.Green;
                case 4: return System.Drawing.Color.Orange;
                case 5: return System.Drawing.Color.PaleVioletRed;
                case 6: return System.Drawing.Color.Violet;
                case 7: return System.Drawing.Color.Cyan;
                default: return System.Drawing.Color.Transparent;
            }
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




        #endregion

        public bool shouldClose = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!shouldClose)
            {
                // Cancel the closing
                e.Cancel = true;

                // Hide the window instead of closing it
                this.Hide();
            }
        }
    }
}
