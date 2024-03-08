using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database;
using Database.Repositories;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Squirrel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class MainWindow : Window
    {
        private readonly IUserRepository _userRepository;
        private readonly IAbstractFactory<Clients> _factoryClients;
        private readonly IAbstractFactory<Config> _factoryConfig;
        private UserController _userController;
        private OnStartupContoller _onStartupController;

        public static UserDTO user = null;
        private bool onlyOne = false; // To ensure that only one window is shown
        public MainWindow(IUserRepository userRepository, IAbstractFactory<Clients> factoryClients,
            IAbstractFactory<Config> factoryConfig,
            IDatabaseFunctionsRepository dfRepository)
        {
            try
            {
                InitializeComponent();

                _userRepository = userRepository;
                _factoryClients = factoryClients;
                _factoryConfig = factoryConfig;

                _onStartupController = new OnStartupContoller(dfRepository);

                // Let it run in the background on every starting
                _onStartupController.RunUpdateUnavailableDates();

                _userController = new UserController(_userRepository);

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string connectionString = config.ConnectionStrings.ConnectionStrings["cs"].ConnectionString;
                AppSettings.ConnectionString = connectionString;

                // Access the image resource from your application resources
                ImageSource imageSource = (ImageSource)Application.Current.FindResource("pass_peek_icon");
                // Set the Source property of the Image control
                PassShowHide.Source = imageSource;

                AddVersionNumber();
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Title += $" v.{versionInfo.FileVersion } ";
        }
      
        private async Task CheckForUpdates()
        {
            // UpdateManager(@"location\for\updates")
            using (var manager = UpdateManager.GitHubUpdateManager("https://github.com/mi18069/CampaignEditor"))
            {
                var updateInfo = await manager.Result.CheckForUpdate();

                if (updateInfo.ReleasesToApply.Any())
                {
                    var result = MessageBox.Show("A new version of the application is available. Do you want to install it?", "Update Available", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        await manager.Result.UpdateApp();
                        Application.Current.Shutdown();
                    }
                }
            }

            /*using (var manager = new UpdateManager(@"C:\Temp\Releases"))
            {
                var updateInfo = await manager.CheckForUpdate();

                if (updateInfo.ReleasesToApply.Any())
                {
                    var result = MessageBox.Show("A new version of the application is available. Do you want to install it?", "Update Available", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        await manager.UpdateApp();
                        Application.Current.Shutdown();
                    }
                }
            }*/
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
                return;
            }
            finally
            {
                btnCheckCredentials.IsEnabled = true;
            }

            if (userFound == false)
            {
                lblError.Content = "Wrong username or password";                
            }
            else
            {
                if (!onlyOne)
                {
                    onlyOne = true;
                    user = await _userController.GetUserByUsername(username);

                    var f = _factoryClients.Create();
                    f.Show();

                    this.Close();

                }
            }
                
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
