using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class MainWindow : Window
    {
        private readonly IUserRepository _userRepository;
        private readonly IAbstractFactory<Clients> _factoryClients;
        private readonly IAbstractFactory<Config> _factoryConfig;
        private UserController _userController;

        public static UserDTO user = null;
        public MainWindow(IUserRepository userRepository, IAbstractFactory<Clients> factoryClients,
            IAbstractFactory<Config> factoryConfig)
        {
            try
            {
                InitializeComponent();

                _userRepository = userRepository;
                _factoryClients = factoryClients;
                _factoryConfig = factoryConfig;
              
                _userController = new UserController(_userRepository);
                
                // With encryption, also change in DataContext.cs
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string connectionString = config.ConnectionStrings.ConnectionStrings["cs"].ConnectionString;
                AppSettings.ConnectionString = connectionString;
                
                
                /*var connectionString = Environment.GetEnvironmentVariable("Conn_String_Publish");
                //var connectionString = Environment.GetEnvironmentVariable("Conn_String_Local");
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Handle case when environment variable is not set
                   MessageBox.Show("MyAppConnectionString environment variable not set.",
                       "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                AppSettings.ConnectionString = connectionString;*/

                // Access the image resource from your application resources
                ImageSource imageSource = (ImageSource)Application.Current.FindResource("pass_peek_icon");
                // Set the Source property of the Image control
                PassShowHide.Source = imageSource;

                // Subscribe to the Loaded event
                Loaded += MainWindow_Loaded;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // Event handler for the Loaded event
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddVersionNumber();
        }

        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Title += $" v.{versionInfo.FileVersion } ";
        }


        // Checks if the username and password are typed correctly
        private async void btnCheckCredentials_Click(object sender, RoutedEventArgs e)
        {
            btnCheckCredentials.IsEnabled = false;
            lblError.Content = "";

            string username = tbUsername.Text.Trim();
            string password = pbPassword.Password.ToString().Trim();

            bool userFound = false;
            try
            {
                userFound = await _userController.CheckCredentials(username, password);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                btnCheckCredentials.IsEnabled = true;
                return;
            }


            if (userFound == false)
            {
                lblError.Content = "Wrong username or password";                
            }
            else
            {
                    user = await _userController.GetUserByUsername(username);

                    var f = _factoryClients.Create();
                    f.Show();

                    this.Close();            
            }
            btnCheckCredentials.IsEnabled = true;


        }

        #region passwordMechanism
        private void pbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblError.Content = "";
            UpdateLogInButtonAvailability();

            if (pbPassword.Password.Length > 0)
                PassShowHide.Visibility = Visibility.Visible;
            else
                PassShowHide.Visibility = Visibility.Hidden;

        }

        private void PassShowHide_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowPassword();
        }

        private void PassShowHide_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HidePassword();
        }

        private void PassShowHide_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HidePassword();
        }
        void ShowPassword()
        {
            // Access the image resource from your application resources
            ImageSource imageSource = (ImageSource)Application.Current.FindResource("pass_unpeek_icon");
            // Set the Source property of the Image control
            PassShowHide.Source = imageSource;
            tbPassword.Visibility = Visibility.Visible;
            pbPassword.Visibility = Visibility.Hidden;
            tbPassword.Text = pbPassword.Password;
        }
        void HidePassword()
        {
            // Access the image resource from your application resources
            ImageSource imageSource = (ImageSource)Application.Current.FindResource("pass_peek_icon");
            // Set the Source property of the Image control
            PassShowHide.Source = imageSource;
            tbPassword.Visibility = Visibility.Hidden;
            pbPassword.Visibility = Visibility.Visible;
            pbPassword.Focus();
        }
        private void pbPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ((char)e.Key == (char)Key.Space);
        }

        #endregion

        private void tbUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            lblError.Content = "";
            UpdateLogInButtonAvailability();
        }

        // Log in unavailable if one of fields is empty
        private void UpdateLogInButtonAvailability()
        {
            if (tbUsername.Text.Trim().Length == 0 || pbPassword.Password.ToString().Trim().Length == 0)
                btnCheckCredentials.IsEnabled = false;
            else
                btnCheckCredentials.IsEnabled = true;
        }

        // Disable copy-paste mechanism
        private void textBoxes_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void btnConfigDatabase_Click(object sender, RoutedEventArgs e)
        {
            _factoryConfig.Create().ShowDialog();

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            // Check if the Enter key is pressed
            if (e.Key == Key.Enter && btnCheckCredentials.IsEnabled)
            {
                btnCheckCredentials_Click(sender, e);
                e.Handled = true;
            }

        }
    }
}
