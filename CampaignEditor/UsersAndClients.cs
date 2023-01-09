using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Repositories;
using System.Collections.Generic;

// This class is made in order to simplify process of adding/removing users/clients in database

namespace CampaignEditor
{
    public class UsersAndClients
    {
        private ClientController _clientController;
        private UserController _userController;
        private UserClientsController _userClientsController;
        public UsersAndClients(IClientRepository clientRepository, 
                               IUserRepository userRepository,
                               IUserClientsRepository userClientsRepository)
        {
            _clientController = new ClientController(clientRepository);
            _userController = new UserController(userRepository);
            _userClientsController = new UserClientsController(userClientsRepository);
        }

        public async void DeleteUserByUsername(string username)
        {          
            UserDTO user = await _userController.GetUserByUsername(username);
            int userID = user.usrid;
            List<UserClientsDTO> userClients = (List<UserClientsDTO>)await _userClientsController.GetAllUserClientsByUserId(userID);

            foreach (UserClientsDTO userClient in userClients)
            {
                await _userClientsController.DeleteUserClientsByUserId(userID);
            }
            await _userController.DeleteUserByUsername(username);
        }

        public async void DeleteClientByClientname(string clientname)
        {
            ClientDTO client = await _clientController.GetClientByName(clientname);
            int clientId = client.clid;
            List<UserClientsDTO> userClients = (List<UserClientsDTO>)await _userClientsController.GetAllUserClientsByClientId(clientId);

            foreach (UserClientsDTO userClient in userClients)
            {
                await _userClientsController.DeleteUserClientsByClientId(clientId);
            }
            await _clientController.DeleteClientById(clientId);
        }
    }
}
