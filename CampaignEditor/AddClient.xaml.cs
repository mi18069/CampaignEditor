using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace CampaignEditor
{
    public partial class AddClient : Window
    {
        private ClientController _clientController;
        private UserController _userController;
        private UserClientsController _userClientsController;

        public bool success = false;
        public ClientDTO? newClient = null;
        public AddClient(IClientRepository clientRepository, IUserRepository userRepository, 
            IUserClientsRepository userClientsRepository)
        {
            InitializeComponent();

            _clientController = new ClientController(clientRepository);
            _userController = new UserController(userRepository);
            _userClientsController = new UserClientsController(userClientsRepository);

            FillComboBox();
        }

        private async void FillComboBox()
        {
            IEnumerable<UserDTO> users = await _userController.GetAllUsers();
            users = users.OrderBy(u => u.usrname);
            cbOwner.ItemsSource = users;
        }

        private async void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            string clientName = tbClientName.Text.Trim();
            
            if (clientName == "")
            {
                lblError.Content = "Enter client name";
            }
            else if (cbOwner.SelectedIndex < 0)
            {
                lblError.Content = "Select client's owner";
            }
            else
            {
                CreateClientDTO client = new CreateClientDTO(clientName, true, 0);
                UserDTO owner = cbOwner.SelectedItem as UserDTO;

                ClientDTO clientDTO;
                try
                {
                    clientDTO = await _clientController.CreateClient(client);
                    if (clientDTO == null)
                    {
                        MessageBox.Show("Cannot create client", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Cannot create client", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                UserClientsDTO userClientsDTO = new UserClientsDTO(clientDTO.clid, owner.usrid);

                try
                {
                    await _userClientsController.CreateUserClients(userClientsDTO);

                }
                catch
                {
                    try
                    {
                        await _clientController.DeleteClientById(clientDTO.clid);
                        MessageBox.Show("Error while connecting user to client", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("Error while connecting user to client", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                newClient = clientDTO;
                success = true;
                Close();
            }
        }


    }
}
