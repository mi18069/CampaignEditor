using CampaignEditor.StartupHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CampaignEditor
{
    public partial class AssignUser : Window
    {

        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;
        public AssignUser(IAbstractFactory<UsersAndClients> factoryUsersAndClients)
        {
            _factoryUsersAndClients = factoryUsersAndClients;
            InitializeComponent();
            PopulateComboBox();
        }

        private async void PopulateComboBox()
        {
            var usernames = await _factoryUsersAndClients.Create().GetUsersNotFromClient("Stark");
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
