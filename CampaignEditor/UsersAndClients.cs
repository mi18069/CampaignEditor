using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// This class is made in order to simplify process of adding/removing users/clients in database

namespace CampaignEditor
{
    public class UsersAndClients
    {
        private ClientController _clientController;
        private UserController _userController;
        private UserClientsController _userClientsController;

        private ClientDTO _client = null;
        public UsersAndClients(IClientRepository clientRepository, 
                               IUserRepository userRepository,
                               IUserClientsRepository userClientsRepository)
        {
            _clientController = new ClientController(clientRepository);
            _userController = new UserController(userRepository);
            _userClientsController = new UserClientsController(userClientsRepository);
        }

        public void Initialize(ClientDTO client)
        {
            _client = client;
        }

        #region Users of Client

        public async Task<IEnumerable<UserDTO>> GetAllUsersOfClient()
        {
            var userClients = await _userClientsController.GetAllUserClientsByClientId(_client.clid);
            var users = new List<UserDTO>();

            foreach (var userClient in userClients)
            {
                users.Add(await _userController.GetUserById(userClient.usrid));
            }

            return users;
        }

        // Used when you need all users except users from one client 
        public async Task<IEnumerable<UserDTO>> GetUsersNotFromClient(string clientname)
        {
            var AllUsers = await _userController.GetAllUsers();
            var ClientUsers = await GetAllUsersOfClient();

            var remainingUsers = AllUsers.Where(p => !ClientUsers.Any(p2 => p2.usrid == p.usrid)); 

            return remainingUsers;
        }

        #endregion

        #region Assign/Unassign Users to Clients


        public async Task AssignUserToClient(string username)
        {
            UserDTO user = await _userController.GetUserByUsername(username.Trim());
            var userClient = new UserClientsDTO(_client.clid, user.usrid);
            await _userClientsController.CreateUserClients(userClient);
        }

        public async Task UnassignUserFromClient(string username, string clientname)
        {
            UserDTO user = await _userController.GetUserByUsername(username.Trim());
            ClientDTO client = await _clientController.GetClientByName(clientname.Trim());
            await _userClientsController.DeleteUserClients(user.usrid, client.clid);
        }

        #endregion

        #region Delete Users and Clients
        public async void DeleteUserByUsername(string username)
        {          
            UserDTO user = await _userController.GetUserByUsername(username);
            int userID = user.usrid;
            var userClients = await _userClientsController.GetAllUserClientsByUserId(userID);

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

        #endregion
    }
}
