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

        private ClientDTO _client;
        public AssignUser(IAbstractFactory<UsersAndClients> factoryUsersAndClients)
        {
            _factoryUsersAndClients = factoryUsersAndClients;
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

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            var item = cbRemainingUsers.SelectedItem.ToString()!;
            UsersOfClient.instance.AssignUser(cbRemainingUsers.SelectedItem.ToString()!);
            this.Close();
        }
    }
}
