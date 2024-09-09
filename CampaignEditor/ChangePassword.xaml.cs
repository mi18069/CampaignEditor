using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    /// <summary>
    /// This is where clients will change their passwords
    /// </summary>
    public partial class ChangePassword : Window
    {

        private UserController _userController;

        UserDTO _user;
        public ChangePassword(IUserRepository userRepository)
        {
            _userController = new UserController(userRepository);

            InitializeComponent();
        }

        public void Initialize(UserDTO user)
        {
            this._user = user;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckNewPassword() && CheckOldpassword())
            {
                await ChangePasswords();
                MessageBox.Show("Password succesfully changed", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private async Task ChangePasswords()
        {
            string newPassword = pbNewPassword.Password.ToString().Trim();
            _user.usrpass = newPassword;

            await _userController.UpdateUser(new UpdateUserDTO(_user));
        }

        private bool CheckNewPassword()
        {
            string newPassword = pbNewPassword.Password.ToString().Trim();
            string repPassword = pbRepPassword.Password.ToString().Trim();

            if (newPassword.Length == 0)
            {
                MessageBox.Show("Enter password", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (repPassword.Length == 0)
            {
                MessageBox.Show("Confirm password", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (newPassword.Contains(@"\s"))
            {
                MessageBox.Show("Username and password can't contain spaces", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (newPassword != repPassword)
            {
                MessageBox.Show("New and repeated passwords are not same", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool CheckOldpassword()
        {
            string password = _user.usrpass.Trim();
            string oldPassword = pbOldPassword.Password.ToString().Trim();
            if (password == null || oldPassword != password) 
            {
                MessageBox.Show("Old password is incorect", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
