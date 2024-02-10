using System.IO;
using System.Windows;
using System;
using Database.Data;
using Database;
using System.Text.RegularExpressions;

namespace CampaignEditor.UserControls.ForecastGrids
{
    public class ReachFileManipulation
    {

        private readonly string _fileName;
        private readonly string _defaultPath = @"\pln_temp\";
        private string _sourceDir = string.Empty;
        public string SourcePath { get; private set; }
        public string FunctionPath { get; private set; }

        public ReachFileManipulation(string fileName)
        {
            _fileName = fileName;
        }

        public string? GetSourcePath()
        {
            string? connPath = GetConnPath();

            if (connPath == null)
            {
                return null;
            }

            _sourceDir = connPath + _defaultPath;
            SourcePath = connPath + _defaultPath + _fileName;
            FunctionPath = _defaultPath + _fileName;

            try
            {
                Directory.CreateDirectory(_sourceDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while generating tmp directory!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return SourcePath;
        }      

        private string? GetConnPath()
        {
            string encryptedString = AppSettings.ConnectionString.Trim();
            var connectionString = string.Empty;
            try
            {
                connectionString = EncryptionUtility.DecryptString(encryptedString);
            }
            catch
            {
                return null;
            }

            Regex regServer = new Regex("Server=[^;]*;");
            try
            {
                string serverString = regServer.Match(connectionString).Value;
                string serverValue = serverString.Substring(serverString.IndexOf("=") + 1);
                var server = serverValue.Remove(serverValue.Length - 1, 1);
                if (String.Compare(server, "localhost") == 0)
                    return "C:";
                else
                    return @"\\" + server + @"\C$\";
            }
            catch
            {
                return null;
            }
        }

        public void MoveFileToPath(string sourcePath, string destinationPath)
        {
            try
            {
                // Move the file to the destination directory
                File.Move(sourcePath, destinationPath);
                MessageBox.Show($"Pln file created!", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException e)
            {
                MessageBox.Show($"Cannot create pln file!\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                try
                {
                    File.Delete(sourcePath);
                }
                catch
                {

                }
            }
            finally
            {
                try
                {
                    Directory.Delete(_sourceDir);
                }
                catch
                {

                }
            }
        }
    }
}
