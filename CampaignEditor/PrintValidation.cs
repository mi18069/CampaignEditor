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
        public static void Print(Dictionary<DateOnly, List<TermTuple>> dayTermDict,
                                 Dictionary<DateOnly, List<MediaPlanRealized>> dayRealizedDict) 
        {

            using (var memoryStream = new MemoryStream())
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    // Create a new worksheet
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Campaign Info");

                    AddHeaders(worksheet, 1, 1);
                    AddTerms(worksheet, dayTermDict, 2, 1);
                    //17
                    AddHeaders(worksheet, 1, 20, true);
                    AddRealized(worksheet, dayRealizedDict, 2, 20);
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
                columnHeaders = new List<string>() { "Program", "Day Part", "Position",
                "Block time", "Amr% 1", "Amr% 2", "Amr% 3", "Amr Sale%", "CPP",
                "Prog coef", "Dp coef", "Seas coef", "Sec coef", "Length", "Price", "Spot"};
            }
            else
            {
                columnHeaders = new List<string>() { "Program", 
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

        private static void AddTerms(ExcelWorksheet worksheet, Dictionary<DateOnly, List<TermTuple>> dayTermDict, int rowOff, int colOff)
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
                worksheet.Cells[rowOff++, colOff].Value = date.ToShortDateString();
                foreach(var termTuple in termTuples)
                {
                    worksheet.Cells[rowOff, colOff].Value = termTuple.MediaPlan.name.Trim();
                    worksheet.Cells[rowOff, colOff + 1].Value = termTuple.MediaPlan.DayPart.name.Trim();
                    worksheet.Cells[rowOff, colOff + 2].Value = termTuple.MediaPlan.Position;
                    worksheet.Cells[rowOff, colOff + 3].Value = termTuple.MediaPlan.Blocktime;
                    worksheet.Cells[rowOff, colOff + 4].Value = termTuple.MediaPlan.Amrp1;
                    worksheet.Cells[rowOff, colOff + 5].Value = termTuple.MediaPlan.Amrp2;
                    worksheet.Cells[rowOff, colOff + 6].Value = termTuple.MediaPlan.Amrp3;
                    worksheet.Cells[rowOff, colOff + 7].Value = termTuple.MediaPlan.Amrpsale;
                    worksheet.Cells[rowOff, colOff + 8].Value = termTuple.Cpp;
                    worksheet.Cells[rowOff, colOff + 9].Value = termTuple.MediaPlan.Progcoef;
                    worksheet.Cells[rowOff, colOff + 10].Value = termTuple.MediaPlan.Dpcoef;
                    worksheet.Cells[rowOff, colOff + 11].Value = termTuple.Seascoef;
                    worksheet.Cells[rowOff, colOff + 12].Value = termTuple.Seccoef;
                    worksheet.Cells[rowOff, colOff + 13].Value = termTuple.Spot.spotlength;
                    worksheet.Cells[rowOff, colOff + 14].Value = termTuple.Price;
                    worksheet.Cells[rowOff, colOff + 15].Value = termTuple.Spot.spotcode;
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
                worksheet.Cells[rowOff++, colOff].Value = date.ToShortDateString();
                foreach (var mediaPlanRealized in realizedTuples)
                {
                    worksheet.Cells[rowOff, colOff].Value = mediaPlanRealized.name;
                    worksheet.Cells[rowOff, colOff + 1].Value = TimeFormat.TimeStrToRepresentative(mediaPlanRealized.stimestr);
                    worksheet.Cells[rowOff, colOff + 2].Value = mediaPlanRealized.amrp1;
                    worksheet.Cells[rowOff, colOff + 3].Value = mediaPlanRealized.amrp2;
                    worksheet.Cells[rowOff, colOff + 4].Value = mediaPlanRealized.amrp3;
                    worksheet.Cells[rowOff, colOff + 5].Value = mediaPlanRealized.amrpsale;
                    worksheet.Cells[rowOff, colOff + 6].Value = mediaPlanRealized.cpp;
                    worksheet.Cells[rowOff, colOff + 7].Value = mediaPlanRealized.progcoef;
                    worksheet.Cells[rowOff, colOff + 8].Value = mediaPlanRealized.dpcoef;
                    worksheet.Cells[rowOff, colOff + 9].Value = mediaPlanRealized.seascoef;
                    worksheet.Cells[rowOff, colOff + 10].Value = mediaPlanRealized.seccoef;
                    worksheet.Cells[rowOff, colOff + 11].Value = mediaPlanRealized.dure;
                    worksheet.Cells[rowOff, colOff + 12].Value = mediaPlanRealized.durf;
                    worksheet.Cells[rowOff, colOff + 13].Value = mediaPlanRealized.price;
                    worksheet.Cells[rowOff, colOff + 14].Value = mediaPlanRealized.spotname;
                    worksheet.Cells[rowOff, colOff + 15].Value = SetStatus(mediaPlanRealized.status);
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
