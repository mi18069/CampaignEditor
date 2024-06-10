using Database.Entities;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace CampaignEditor
{
    public static class PrintValidation
    {
        public static void Print(Dictionary<DateOnly, List<TermTuple>> dateExpectedDict,
                                 Dictionary<DateOnly, List<MediaPlanRealized>> dateRealizedDict,
                                 List<DateOnly> dates, bool[] expectedMask, bool[] realizedMask) 
        {

            using (var memoryStream = new MemoryStream())
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    // Create a new worksheet
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Campaign Info");

                    AddHeaders(worksheet, 1, 1);
                    AddExpected(worksheet, dateExpectedDict, 2, 1);
                    //17
                    AddHeaders(worksheet, 1, 20, true);
                    AddRealized(worksheet, dateRealizedDict, 2, 20);
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

        private static void AddHeaders(ExcelWorksheet worksheet, int rowOff, int colOff, bool realized = false)
        {
            List<string> columnHeaders; 

            if (!realized)
            {
                columnHeaders = new List<string>() { "Date", "Channel", "Program", "Day Part",
                "Block time", "Position", "Spotcode", "Spot len",  "Amr% 1", "Amr% 2", "Amr% 3", "Amr Sale%", "CPP",
                "Prog coef", "Dp coef", "Seas coef", "Sec coef", "Length", "Price", "Spot"};
            }
            else
            {
                columnHeaders = new List<string>() { "Date", "Program", 
                "Time", "Amr% 1", "Amr% 2", "Amr% 3", "Amr Sale%", "CPP",
                "Prog coef", "Dp coef", "Seas coef", "Sec coef", "Effective", "Formatted", "Price", "Spot", "Status"};
            }

            int numOfColumns = columnHeaders.Count();

            // Set the column headers in Excel
            int colOffset = 0;
            for (int i = 0; i < numOfColumns; i++)
            {
                worksheet.Cells[rowOff, colOff + colOffset].Value = columnHeaders[i];
                worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[rowOff, colOff + colOffset].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DAA520"));
                colOffset += 1;              
            }
        }

        private static void AddExpected(ExcelWorksheet worksheet, Dictionary<DateOnly, List<TermTuple>> dayTermDict, int rowOff, int colOff)
        {

            List<DateOnly> dates = dayTermDict.Keys.OrderBy(k => k).ToList();
            foreach (var date in dates)
            {
                List<TermTuple> termTuples = null;
                try
                {
                    termTuples = dayTermDict[date];
                }
                catch
                {
                    continue;
                }
                var dateString = date.ToShortDateString();
                rowOff++;
                foreach (var termTuple in termTuples)
                {
                    var colOffset = colOff;
                    worksheet.Cells[rowOff, colOffset++].Value = dateString;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.name.Trim();
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.DayPart.name.Trim();
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Position;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Blocktime;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Amrp1;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Amrp2;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Amrp3;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Amrpsale;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.Cpp;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Progcoef;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.MediaPlan.Dpcoef;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.Seascoef;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.Seccoef;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.Spot.spotlength;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.Price;
                    worksheet.Cells[rowOff, colOffset++].Value = termTuple.Spot.spotcode;
                    rowOff++;

                }
            }

            
        }

        private static void AddRealized(ExcelWorksheet worksheet, Dictionary<DateOnly, List<MediaPlanRealized>> dayRealizedDict, int rowOff, int colOff)
        {
            List<DateOnly> dates = dayRealizedDict.Keys.OrderBy(k => k).ToList();

            foreach (var date in dates)
            {
                List<MediaPlanRealized> realizedTuples = null;
                try
                {
                    realizedTuples = dayRealizedDict[date];
                }
                catch
                {
                    continue;
                }
                var dateString = date.ToShortDateString();
                rowOff++;
                foreach (var mediaPlanRealized in realizedTuples)
                {
                    var colOffset = colOff;
                    worksheet.Cells[rowOff, colOffset++].Value = dateString;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.name;
                    worksheet.Cells[rowOff, colOffset++].Value = TimeFormat.TimeStrToRepresentative(mediaPlanRealized.stimestr);
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.amrp1;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.amrp2;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.amrp3;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.amrpsale;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.cpp;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.progcoef;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.dpcoef;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.seascoef;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.seccoef;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.dure;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.durf;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.price;
                    worksheet.Cells[rowOff, colOffset++].Value = mediaPlanRealized.spotname;
                    worksheet.Cells[rowOff, colOffset++].Value = SetStatus(mediaPlanRealized.status);
                    rowOff++;

                }
            }


        }

        private static string SetStatus(int? status)
        {
            switch (status)
            {
                case 1: return "OK";
                case 2: return "NOT OK";
                default: return "UNRESOLVED";
            }
        }

        private static void SaveFile(SaveFileDialog saveFileDialog, MemoryStream memoryStream)
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
    }
}
