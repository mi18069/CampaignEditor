using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.StartupHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class UsersOfClient : Window
    {

        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;
        public UsersOfClient(IAbstractFactory<UsersAndClients> factoryUsersAndClients)
        {
            InitializeComponent();
            _factoryUsersAndClients = factoryUsersAndClients;
            PopulateItems();
        }

        private async void PopulateItems()
        {
            spUsers.Children.Clear();

            List<UserDTO> users = (List<UserDTO>) await _factoryUsersAndClients.Create().GetAllUsersOfClient("Stark");

            UsersListItem[] listItems = new UsersListItem[users.Count()];

            

            for (int i=0; i<listItems.Length; i++)
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
            imgGreenPlus.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\images\\GreenPlus.png"));
            Button.Content = imgGreenPlus;

            spUsers.Children.Add(Button);
        }
    }
}
