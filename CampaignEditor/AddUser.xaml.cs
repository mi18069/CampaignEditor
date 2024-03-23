using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace CampaignEditor
{
    public partial class AddUser : Window
    {
        private UserController _userController;

        public bool success = false;
        public UserDTO? user = null;
        public AddUser(IUserRepository userRepository)
        {
            InitializeComponent();

            _userController = new UserController(userRepository);

        }

        public void Initialize(UserDTO user)
        {
            this.user = user;
            FillFields(user);
        }

        private void FillFields(UserDTO user)
        {
            tbUsername.Text = user.usrname.ToString().Trim();
            pbPassword.Password = user.usrpass.ToString().Trim();   
            pbPasswordConfirm.Password = user.usrpass.ToString().Trim();
            cbAuthorization.SelectedIndex = user.usrlevel;
            tbEmail.Text = user.email.Trim();
            tbPhone.Text = user.telefon.Trim();
        }

        private async void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text.Trim();
            string password = pbPassword.Password.ToString().Trim();
            string passwordConfirmed = pbPasswordConfirm.Password.ToString().Trim();
            int authorization = cbAuthorization.SelectedIndex;
            string email = tbEmail.Text.Trim();
            string phone = tbPhone.Text.Trim();
            int enabled = 1;
            int father = MainWindow.user.usrid;
            bool buy = false;

            if ( await CheckRegistrationCredentialsAsync(username, password, passwordConfirmed, phone) )
            {
                if (user != null)
                {
                    user.usrname = username;
                    user.usrpass = password;
                    user.usrlevel = authorization;
                    user.email = email;
                    user.telefon = phone;
                    user.enabled = enabled;
                    // not updating user father
                    user.buy = buy;
                    await _userController.UpdateUser(new UpdateUserDTO(user));
                }
                else
                {
                    try
                    {
                        user = await _userController.CreateUser(new CreateUserDTO
                            (username, password, authorization, email, phone, enabled, father, buy));
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user == null)
                    {
                        MessageBox.Show("Cannot create user!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                success = true;
                this.Close();
            }

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

        private async Task<bool> CheckRegistrationCredentialsAsync(string username, string password, string passwordConfirmed, string phone)
        {
            if (username.Length == 0)
            {
                MessageBox.Show("Enter username", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (password.Length == 0)
            {
                MessageBox.Show("Enter password", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (passwordConfirmed.Length == 0)
            {
                MessageBox.Show("Confirm password", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (username.Contains(@"\s") || password.Contains(@"\s"))
            {
                MessageBox.Show("Username and password can't contain spaces", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (username.Any(c => !(char.IsLetter(c) || char.IsDigit(c) || char.IsWhiteSpace(c))))
            {
                MessageBox.Show("Username can't contain special characters", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (username.Any(c => char.IsUpper(c)))
            {
                MessageBox.Show("Username can't contain uppercase Letters", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (password != passwordConfirmed)
            {
                MessageBox.Show("Passwords not matching", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (phone.Any(c => char.IsLetter(c)))
            {
                MessageBox.Show("Unsupported phone format", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            // this condition is checking only if we're creating new user
            else if (user == null && await _userController.GetUserByUsername(username) != null)
            {
                MessageBox.Show("Username already exists", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void pbPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            lblError.Content = "";
            e.Handled = ((char)e.Key == (char)Key.Space);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
