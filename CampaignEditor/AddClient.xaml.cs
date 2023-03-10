using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace CampaignEditor
{
    public partial class AddClient : Window
    {
        private ClientController _clientController;
        private UserController _userController;
        private UserClientsController _userClientsController;

        public bool success = false;
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
            IEnumerable<string> usernames = await _userController.GetAllUsernames();
            usernames = usernames.OrderBy(u => u);
            foreach (var username in usernames)
            {
                cbOwner.Items.Add(username);
            }
        }

        private async void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            string clientName = tbClientName.Text.Trim();
            if (await _clientController.GetClientByName(clientName) != null)
            {
                lblError.Content = "Client already exist";
            }
            else if (clientName == "")
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
                string ownerName = cbOwner.Items[cbOwner.SelectedIndex].ToString()!.Trim();
                UserDTO owner = await _userController.GetUserByUsername(ownerName);
                
                ClientDTO clientDTO = await _clientController.CreateClient(client);
                UserClientsDTO userClientsDTO = new UserClientsDTO(clientDTO.clid, owner.usrid);

                await _userClientsController.CreateUserClients(userClientsDTO);
                success = true;
                Close();
            }
        }


    }
}
