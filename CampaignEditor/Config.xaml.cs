using Database;
using System;
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
            cbServer.Items.Add(".");
            cbServer.Items.Add("localhost");
            cbServer.SelectedIndex = 1;

            FillFields();
        }

        private void FillFields()
        {
            Regex regPort = new Regex("port=\\d*;");
            Regex regUsername = new Regex("user id=\\w*;");
            Regex regPassword = new Regex("password=\\w*;");
            Regex regDatabase = new Regex("database=\\w*;");
            string connectionString = AppSettings.ConnectionString;
            
            string portString = regPort.Match(connectionString).Value;
            string portValue = portString.Substring(portString.IndexOf("=") + 1);
            tbPort.Text = portValue.Remove(portValue.Length-1, 1);
            string usernameString = regUsername.Match(connectionString).Value;
            string usernameValue = usernameString.Substring(usernameString.IndexOf("=") + 1);
            tbUsername.Text = usernameValue.Remove(usernameValue.Length - 1, 1);
            string passwordString = regPassword.Match(connectionString).Value;
            string passwordValue = passwordString.Substring(passwordString.IndexOf("=") + 1);
            pbPassword.Password = passwordValue.Remove(passwordValue.Length - 1, 1);
            string databaseString = regDatabase.Match(connectionString).Value;
            string databaseValue = databaseString.Substring(databaseString.IndexOf("=") + 1);
            tbDatabase.Text = databaseValue.Remove(databaseValue.Length - 1, 1);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = string.Format("Server={0};port={1};user id={2};password={3};database={4};",
                cbServer.Text.Trim(), tbPort.Text.Trim(), tbUsername.Text.Trim(), pbPassword.Password.ToString().Trim(), tbDatabase.Text.Trim());

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
            string connectionString = string.Format("Server={0};port={1};user id={2};password={3};database={4};",
                cbServer.Text.Trim(), tbPort.Text.Trim(), tbUsername.Text.Trim(), pbPassword.Password.ToString().Trim(), tbDatabase.Text.Trim());
            
            // Saving without checkings
            AppSetting setting = new AppSetting();
            setting.SaveConnectionString("cs", connectionString);
            MessageBox.Show("Connection succesfully saved", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();

        }
    }
}
