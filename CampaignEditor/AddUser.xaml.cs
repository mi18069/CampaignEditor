﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace CampaignEditor
{
    public partial class AddUser : Window
    {
        private readonly IUserRepository _userRepository;
        private UserController _userController;

        public AddUser(IUserRepository userRepository)
        {
            InitializeComponent();

            _userRepository = userRepository;
            _userController = new UserController(_userRepository);
        }

        private async void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text.Trim();
            string password = pbPassword.Password.ToString().Trim();
            string passwordConfirmed = pbPasswordConfirm.Password.ToString().Trim();
            int authorization = (cbAuthorization.Items.Count - 1) - cbAuthorization.SelectedIndex;
            string email = tbEmail.Text.Trim();
            string phone = tbPhone.Text.Trim();
            int enabled = 1;
            int father = MainWindow.instance.user.usrid;
            bool buy = false;

            if ( await CheckRegistrationCredentialsAsync(username, password, passwordConfirmed, phone) )
            {
                await _userController.CreateUser(new CreateUserDTO
                    (username, password, authorization, email, phone, enabled, father, buy));
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

        // TODO needs to check for instances in userClients table first, this method shouldn't be in this class
        private async void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text.Trim();

            await _userController.DeleteUserByUsername(username);
        }

        private async Task<bool> CheckRegistrationCredentialsAsync(string username, string password, string passwordConfirmed, string phone)
        {
            if (username.Length == 0)
            {
                lblError.Content = "Enter username";
                return false;
            }
            else if (password.Length == 0)
            {
                lblError.Content = "Enter password";
                return false;
            }
            else if (passwordConfirmed.Length == 0)
            {
                lblError.Content = "Confirm password";
                return false;
            }
            else if (username.Contains(@"\s") || password.Contains(@"\s"))
            {
                lblError.Content = "Username and password can't contain spaces";
                return false;
            }
            else if (username.Any(c => !(char.IsLetter(c) || char.IsDigit(c) || char.IsWhiteSpace(c))))
            {
                lblError.Content = "Username can't contain special characters";
                return false;
            }
            else if (username.Any(c => char.IsUpper(c)))
            {
                lblError.Content = "Username can't contain uppercase Letters";
                return false;
            }
            else if (password != passwordConfirmed)
            {
                lblError.Content = "Passwords not matching";
                return false;
            }
            else if (phone.Any(c => char.IsLetter(c)))
            {
                lblError.Content = "Unsupported phone format";
                return false;
            }
            else if (await _userController.GetUserByUsername(username) != null)
            {
                lblError.Content = "Username already exist";
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
