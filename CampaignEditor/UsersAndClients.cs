using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

// This class is made in order to simplify process of adding/removing users/clients in database

namespace CampaignEditor
{
    public class UsersAndClients
    {
        private ClientController _clientController;
        private UserController _userController;
        private CampaignController _campaignController;
        private UserClientsController _userClientsController;
        private DayPartController _dayPartController;
        private DPTimeController _dpTimeController;
        private DGConfigController _dgConfigController;

        private ClientDTO _client = null;
        public UsersAndClients(IClientRepository clientRepository, 
                               IUserRepository userRepository,
                               IUserClientsRepository userClientsRepository,
                               ICampaignRepository campaignRepository,
                               IDGConfigRepository dgConfigRepository,
                               IDayPartRepository dayPartRepository,
                               IDPTimeRepository dpTimeRepository)
        {
            _clientController = new ClientController(clientRepository);
            _userController = new UserController(userRepository);
            _userClientsController = new UserClientsController(userClientsRepository);
            _campaignController = new CampaignController(campaignRepository);
            _dayPartController = new DayPartController(dayPartRepository);
            _dpTimeController = new DPTimeController(dpTimeRepository);
            _dgConfigController = new DGConfigController(dgConfigRepository);
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

        public async Task<IEnumerable<Tuple<UserDTO, int>>> GetAllUserAuthorizationsOfClient(int clid)
        {
            var userClients = await _userClientsController.GetAllUserClientsByClientId(clid);
            var usersAuthorization = new List<Tuple<UserDTO, int>>();
            foreach (var userClient in userClients)
            {
                usersAuthorization.Add(Tuple.Create(await _userController.GetUserById(userClient.usrid), userClient.usrlevel));
            }
            usersAuthorization = usersAuthorization.OrderBy(ua => ua.Item1.usrname).ToList();

            return usersAuthorization;
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


        public async Task AssignUserToClient(UserDTO user)
        {
            var userClient = new UserClientsDTO(_client.clid, user.usrid, user.usrlevel);
            await _userClientsController.CreateUserClients(userClient);
        }

        public async Task UnassignUserFromClient(UserDTO user, ClientDTO client)
        {
            await _userClientsController.DeleteUserClients(user.usrid, client.clid);
        }

        #endregion


        #region Delete Users and Clients
        public async Task<bool> DeleteUser(UserDTO user)
        {
            IEnumerable<UserClientsDTO> userClients; 
            try
            {
                userClients = await _userClientsController.GetAllUserClientsByUserId(user.usrid);

                foreach (UserClientsDTO userClient in userClients)
                {
                    await _userClientsController.DeleteUserClientsByUserId(user.usrid);
                }
                await _dgConfigController.DeleteDGConfigByUsrid(user.usrid);
                await _userController.DeleteUserById(user.usrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteClient(ClientDTO client)
        {
            // First delete all userClients bounds

            var userClients = await _userClientsController.GetAllUserClientsByClientId(client.clid);
            try
            {
                bool success = await _userClientsController.DeleteUserClientsByClientId(client.clid);
                if (!success)
                {
                    throw new Exception();
                }
            }
            catch
            {
                var newUserClients = await _userClientsController.GetAllUserClientsByClientId(client.clid);

                // Try to retrieve deleted userClients bonds
                if (userClients.Count() != newUserClients.Count())
                {
                    var deletedUserClients = new List<UserClientsDTO>();
                    foreach (var userClient in userClients)
                    {
                        if (!newUserClients.Select(uc => uc.usrid).Contains(userClient.usrid))
                        {
                            deletedUserClients.Add(userClient);
                        }
                    }


                    foreach (var userClient in deletedUserClients)
                    {
                        try
                        {
                            await _userClientsController.CreateUserClients(userClient);
                        }
                        catch
                        {
                            // cannot retrieve deleted userClient bonds
                        }
                    }

                }

                return false;
            }

            try
            {
                // If client have some campaigns then set client to inactive
                // otherwise, delete that client
                if (userClients.Count() > 0)
                {
                    client.clactive = false;
                    bool success = await _clientController.UpdateClient(new UpdateClientDTO(client));
                    if (!success)
                        return false;
                }
                else
                {
                    // Delete dayParts binded to that client
                    var dayParts = await _dayPartController.GetAllClientDayParts(client.clid);
                    foreach (var dayPart in dayParts)
                    {
                        await _dpTimeController.DeleteDPTimeByDPId(dayPart.dpid);
                        await _dayPartController.DeleteDayPart(dayPart.dpid);
                    }

                    bool success = await _dgConfigController.DeleteDGConfigByClid(client.clid);
                    success = success || await _clientController.DeleteClientById(client.clid);
                    if (!success)
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> CanClientBeDeleted(ClientDTO client)
        {
            var campaigns = await _campaignController.GetCampaignsByClientId(client.clid);

            // Can't delete Client that have active campaigns
            foreach (CampaignDTO campaign in campaigns)
            {
                if (campaign.active)
                    return false;
            }

            return true;
        }


        #endregion
    }
}
