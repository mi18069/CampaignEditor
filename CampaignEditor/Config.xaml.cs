using Database;
using Database.Data;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace CampaignEditor
{

    public partial class Config : Window
    {
        public Config()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            FillFields();
        }

        private void FillFields()
        {
            string encryptedString = AppSettings.ConnectionString.Trim();
            // Decrypt string
            var connectionString = string.Empty;
            try
            {
                connectionString = EncryptionUtility.DecryptString(encryptedString);
            }
            catch
            {
                connectionString = "Server=;port=;user id=;password=;database=;";
            }

            Regex regServer = new Regex("Server=[^;]*;");
            Regex regPort = new Regex("port=[^;]*;");
            Regex regUsername = new Regex("user id=[^;]*;");
            Regex regPassword = new Regex("password=[^;]*;");
            Regex regDatabase = new Regex("database=[^;]*;");

            try
            {
                string serverString = regServer.Match(connectionString).Value;
                string serverValue = serverString.Substring(serverString.IndexOf("=") + 1);
                tbServer.Text = serverValue.Remove(serverValue.Length - 1, 1);
            }
            catch
            {
                tbServer.Text = "";
            }

            try
            {
                string portString = regPort.Match(connectionString).Value;
                string portValue = portString.Substring(portString.IndexOf("=") + 1);
                tbPort.Text = portValue.Remove(portValue.Length - 1, 1);
            }
            catch
            {
                tbPort.Text = "";
            }

            try
            {
                string usernameString = regUsername.Match(connectionString).Value;
                string usernameValue = usernameString.Substring(usernameString.IndexOf("=") + 1);
                tbUsername.Text = usernameValue.Remove(usernameValue.Length - 1, 1);
            }
            catch
            {
                tbUsername.Text = "";
            }

            try
            {
                string passwordString = regPassword.Match(connectionString).Value;
                string passwordValue = passwordString.Substring(passwordString.IndexOf("=") + 1);
                pbPassword.Password = passwordValue.Remove(passwordValue.Length - 1, 1);
            }
            catch
            {
                pbPassword.Password = "";
            }

            try
            {
                string databaseString = regDatabase.Match(connectionString).Value;
                string databaseValue = databaseString.Substring(databaseString.IndexOf("=") + 1);
                tbDatabase.Text = databaseValue.Remove(databaseValue.Length - 1, 1);
            }
            catch
            {
                tbDatabase.Text = "";
            }

        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {

            if (tbServer.Text.Contains(';') ||
                tbPort.Text.Contains(';') ||
                tbUsername.Text.Contains(';') ||
                pbPassword.Password.Contains(';') ||
                tbDatabase.Text.Contains(';'))
            {
                MessageBox.Show("Property cannot contain symbol \";\"", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string connectionString = string.Format("Server={0};port={1};user id={2};password={3};database={4};",
                tbServer.Text.Trim(), tbPort.Text.Trim(), tbUsername.Text.Trim(), pbPassword.Password.ToString().Trim(), tbDatabase.Text.Trim());

            try
            {
                PgHelper helper = new PgHelper(connectionString);
                if (helper.isConnection)
                    MessageBox.Show("Test connection succeded", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Save();

        }

        private void Save()
        {
            if (tbServer.Text.Contains(';') ||
                tbPort.Text.Contains(';') ||
                tbUsername.Text.Contains(';') ||
                pbPassword.Password.Contains(';') ||
                tbDatabase.Text.Contains(';'))
            {
                MessageBox.Show("Property cannot contain symbol \";\"", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string connectionString = string.Format("Server={0};port={1};user id={2};password={3};database={4};",
                tbServer.Text.Trim(), tbPort.Text.Trim(), tbUsername.Text.Trim(), pbPassword.Password.ToString().Trim(), tbDatabase.Text.Trim());

            // Encrypting connectionString
            var encryptedString = string.Empty;
            try
            {
                encryptedString = EncryptionUtility.EncryptString(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save settings: " + ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Saving without checkings
            AppSetting setting = new AppSetting();
            setting.SaveConnectionString("cs", encryptedString);
            MessageBox.Show("Connection succesfully saved", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Save();
            }
        }
    }
}
