using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace CampaignEditor
{
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        public UserDTO user;

        private readonly IUserRepository _userRepository;
        private readonly IAbstractFactory<Clients> _factoryClients;
        private UserController _userController;

        private string appPath = Directory.GetCurrentDirectory();
        private string imgPeekPath = "\\images\\PassPeekImg.png";
        private string imgUnpeekPath = "\\images\\PassUnpeekImg.png";

        public MainWindow(IUserRepository userRepository, IAbstractFactory<Clients> factoryClients)
        {
            
            InitializeComponent();
            _userRepository = userRepository;
            _factoryClients = factoryClients;

            instance = this;

            _userController = new UserController(_userRepository);

            PassShowHide.Source = new BitmapImage(new Uri(appPath + imgPeekPath));
        }

        // Checks if the username and password are typed correctly
        private async void btnCheckCredentials_Click(object sender, RoutedEventArgs e)
        {
            lblError.Content = "";

            string username = tbUsername.Text.Trim();
            string password = pbPassword.Password.ToString().Trim();

            var userFound = await _userController.CheckCredentials(username, password);

            if (userFound == false)
            {
                lblError.Content = "Wrong username or password";                
            }
            else
            {
                user = await _userController.GetUserByUsername(username);
                _factoryClients.Create().Show();
                this.Close();
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
            PassShowHide.Source = new BitmapImage(new Uri(appPath + imgUnpeekPath));
            tbPassword.Visibility = Visibility.Visible;
            pbPassword.Visibility = Visibility.Hidden;
            tbPassword.Text = pbPassword.Password;
        }
        void HidePassword()
        {
            PassShowHide.Source = new BitmapImage(new Uri(appPath + imgPeekPath));
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
    }
}
