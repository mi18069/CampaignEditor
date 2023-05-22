using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;

namespace CampaignEditor
{
    public partial class AssignUser : Window
    {

        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;
        private UserController _userController;

        private ClientDTO _client;

        public bool isAssigned = false;
        public UserDTO user;
        public AssignUser(IAbstractFactory<UsersAndClients> factoryUsersAndClients,
            IUserRepository userRepository)
        {
            this.DataContext = this;
            _factoryUsersAndClients = factoryUsersAndClients;
            _userController = new UserController(userRepository);
            InitializeComponent();
        }

        public void Initialize(ClientDTO client)
        {
            _client = client;
            PopulateComboBox();
        }

        private async void PopulateComboBox()
        {
            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            var usernames = await f.GetUsersNotFromClient(_client.clname.Trim());
            usernames = usernames.OrderBy(u => u.usrname);
            foreach (var username in usernames)
            {
                cbRemainingUsers.Items.Add(username.usrname.Trim());
            }

            if (cbRemainingUsers.Items.Count > 0)
                cbRemainingUsers.SelectedIndex = 0;
            else
                btnAssign.IsEnabled = false;
        }

        private async void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            var username = cbRemainingUsers.SelectedItem.ToString()!.Trim();
            if (username != null)
            {
                //UsersOfClient.instance.AssignUser(cbRemainingUsers.SelectedItem.ToString()!);
                user = await _userController.GetUserByUsername(username);
                isAssigned = true;
                this.Close();
            }

            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
