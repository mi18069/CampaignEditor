using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CampaignEditor
{
    public partial class UsersOfClient : Window
    {
        public static UsersOfClient instance;
        private ClientDTO _client = null;

        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";

        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;
        private readonly IAbstractFactory<AssignUser> _factoryAssignUser;

        private ClientController _clientController;
        public UsersOfClient(IAbstractFactory<UsersAndClients> factoryUsersAndClients,
                             IAbstractFactory<AssignUser> factoryAssignUser,
                             IClientRepository clientRepository)
        {
            instance = this;
            InitializeComponent();
            _factoryUsersAndClients = factoryUsersAndClients;
            _factoryAssignUser = factoryAssignUser; 
            _clientController = new ClientController(clientRepository);
        }


        public async Task Initialize(string clientname)
        {
            _client = await _clientController.GetClientByName(clientname);
            PopulateItems();

        }
        private async void PopulateItems()
        {
            spUsers.Children.Clear();

            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            List<UserDTO> users = (List<UserDTO>) await f.GetAllUsersOfClient();
            users = users.OrderBy(u => u.usrname).ToList();

            UsersListItem[] listItems = new UsersListItem[users.Count()];



            for (int i = 0; i < listItems.Length; i++)
            {
                listItems[i] = new UsersListItem();
                listItems[i].Username = users[i].usrname;

                listItems[i].Userlevel = users[i].usrlevel == 0 ? "Administrator" :
                                         users[i].usrlevel == 1 ? "Read and write" : "Read";

                spUsers.Children.Add(listItems[i]);
            }

            var Button = new Button();
            Button.Width = 50;
            Button.Height = 50;
            Button.Background = new SolidColorBrush(Colors.White);
            Button.BorderThickness = new Thickness(0);
            Image imgGreenPlus = new Image();
            imgGreenPlus.Source = new BitmapImage(new Uri(appPath + imgGreenPlusPath));
            Button.Content = imgGreenPlus;
            Button.Click += new RoutedEventHandler(AssignUser_Click);


            spUsers.Children.Add(Button);
        }

        public async Task UnassignUser_Click(string username)
        {
            await _factoryUsersAndClients.Create().UnassignUserFromClient(username, _client.clname.Trim());
            PopulateItems();
        }

        private void AssignUser_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAssignUser.Create();
            f.Initialize(_client);
            f.ShowDialog();
        }
        public async Task AssignUser(string username)
        {
            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            await f.AssignUserToClient(username);
            PopulateItems();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
