using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor
{
    public class ClientsTreeView
    { 
        SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> _clientCampaignsDict = new SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>>(new ClientComparer());
        SortedDictionary<UserDTO, IEnumerable<ClientDTO>> _userClientsDict = new SortedDictionary<UserDTO, IEnumerable<ClientDTO>>(new UserComparer());

        private UserController _userController;
        private ClientController _clientController;
        private UserClientsController _userClientsController;
        private CampaignController _campaignController;
        private UserDTO _userDTO;
        public TreeView _tvClients = Clients.instance.tvClients;

        public ClientsTreeView(IUserRepository userRepository, IClientRepository clientRepository,
            IUserClientsRepository userClientsRepository, ICampaignRepository campaignRepository)
        {

            _userController = new UserController(userRepository);
            _clientController = new ClientController(clientRepository);
            _userClientsController = new UserClientsController(userClientsRepository);
            _campaignController = new CampaignController(campaignRepository);
        }

        public void Initialization(UserDTO user)
        {
            _userDTO = user;
        }

        // Initializing dictionaries
        public async void InitializeTree()
        {
            IEnumerable<UserDTO> users = new List<UserDTO>();

            if (_userDTO.usrlevel == 0)
            {
                users = await _userController.GetAllUsers();
            }
            else
            {
                users = users.Append(await _userController.GetUserById(_userDTO.usrid));
            }

            foreach (UserDTO user in users)
            {
                IEnumerable<UserClientsDTO> userClients = await _userClientsController.GetAllUserClientsByUserId(user.usrid);
                IEnumerable<ClientDTO> clients = new List<ClientDTO>();
                IEnumerable<CampaignDTO> campaigns = new List<CampaignDTO>();

                foreach (UserClientsDTO userClient in userClients)
                {
                    clients = clients.Append(await _clientController.GetClientById(userClient.cliid));
                }

                _userClientsDict[user] = clients;

                foreach (ClientDTO client in clients)
                {
                    campaigns = await _campaignController.GetCampaignsByClientId(client.clid);
                    campaigns = campaigns.OrderByDescending(c => c.cmpsdate);
                    _clientCampaignsDict[client] = campaigns;
                }

            }
            UpdateTree();
        }

        // Checking for changes in tree
        public async void UpdateTree()
        {
            SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict =
                new SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>>(_clientCampaignsDict, new ClientComparer());

            string clientSearchString = Clients.instance.tbSearchClients.Text;
            string campaignSearchString = Clients.instance.tbSearchCampaigns.Text;
            bool expandTree = false;
           
            if (clientSearchString.Length >= 3 && clientSearchString != Clients.instance.searchClientsString)
            {
                clientCampaignsDict = FilterClientsByPrefix(clientCampaignsDict, Clients.instance.tbSearchClients.Text.ToLower());
            }      
            if (campaignSearchString.Length >= 3 && campaignSearchString != Clients.instance.searchCampaignsString)
            {
                clientCampaignsDict = FilterCampaignsByPrefix(clientCampaignsDict, Clients.instance.tbSearchCampaigns.Text.ToLower());
                expandTree = true;
            }
            if (Clients.instance.cbUsers.SelectedIndex != -1 && Clients.instance.cbUsers.SelectedIndex != 0)
            {
                var username = Clients.instance.cbUsers.SelectedValue.ToString().Trim();
                clientCampaignsDict = await FilterClientsByUsername(clientCampaignsDict, username);
            }
            clientCampaignsDict = FilterCampaigns(clientCampaignsDict);

            InitializeTreeView(clientCampaignsDict, expandTree);
        }

        // Initializing treeView GUI
        public void InitializeTreeView(SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict, bool expand = false)
        {
            _tvClients.Items.Clear();
            
            foreach (ClientDTO client in clientCampaignsDict.Keys)
            {
                TreeViewItem newClient = new TreeViewItem();
                newClient.Header = client.clname;
                newClient.Tag = "Client";
                _tvClients.Items.Add(newClient);               

                foreach (CampaignDTO campaign in clientCampaignsDict[client])
                {
                    TreeViewItem newCampaign = new TreeViewItem();
                    if (IsCampaignExpired(campaign))
                    {
                        newCampaign.Foreground = Brushes.Gray;
                        newCampaign.Header = campaign.cmpname;
                    }
                    else if (IsCampaignStarted(campaign))
                    {
                        newCampaign.Header = campaign.cmpname;
                    }
                    newCampaign.Tag = "Campaign";
                    newCampaign.MouseDoubleClick += Clients.instance.btnAddCampaign_Click;
                    newClient.Items.Add(newCampaign);
                }
            }

            if (expand)
            {
                foreach (var item in _tvClients.Items)
                {
                    var tvi = item as TreeViewItem;
                    if (tvi != null)
                        tvi.ExpandSubtree();
                }
            }

        } 

        // Tried something with UserControls
        /*public void InitializeTreeView(SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict, bool expand = false)
        {
            _tvClients.Items.Clear();

            foreach (ClientDTO client in clientCampaignsDict.Keys)
            {
                TreeViewClientsItem newClient = new TreeViewClientsItem();
                newClient.Item = client;             
                int numCampaigns = _clientCampaignsDict[client].Count();
                newClient.Description = numCampaigns + " campaigns";
                newClient.Tag = "Client";
                _tvClients.Items.Add(newClient);

                foreach (CampaignDTO campaign in clientCampaignsDict[client])
                {
                    TreeViewCampaignsItem newCampaign = new TreeViewCampaignsItem();
                    if (IsCampaignExpired(campaign))
                    {
                        newCampaign.Foreground = Brushes.Gray;      
                    }
                    else if (IsCampaignStarted(campaign))
                    {
                        newCampaign.Foreground = Brushes.LightGreen;
                    }
                    newCampaign.Item = campaign;
                    
                    newCampaign.Description = "( " + PrintCampaignDate(campaign) + " )";
                    newCampaign.Tag = "Campaign";

                    
                    //newClient.Items.Add(newCampaign);
                }
            }

            if (expand)
            {
                foreach (var item in _tvClients.Items)
                {
                    var tvi = item as TreeViewItem;
                    if (tvi != null)
                        tvi.ExpandSubtree();
                }
            }

        } */

        public async Task<IEnumerable<string>> GetSupervisedUsernames()
        {
            IEnumerable<UserDTO> users = new List<UserDTO>();

            if (_userDTO.usrlevel == 0)
            {
                users = await _userController.GetAllUsers();
            }
            else
            {
                users = users.Append(await _userController.GetUserById(_userDTO.usrid));
            }
            IEnumerable<string> usernames = new List<string>();

            foreach (UserDTO user in users)
            {
                usernames = usernames.Append(user.usrname);
            }
            return usernames;
        }

        #region Filters
        private SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> FilterCampaigns(SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict)
        {
            bool startDate = false;
            bool active = true;
            bool notStarted = true;
            bool finished = true;

            if ( (bool)Clients.instance.cbDatePicker.IsChecked! )
                startDate = true;
            if ((bool)Clients.instance.cbShowActive.IsChecked!)
                active = false;
            if ((bool)Clients.instance.cbShowNotStarted.IsChecked!)
                notStarted = false;
            if ((bool)Clients.instance.cbShowFinished.IsChecked!)
                finished = false;

            for (int i=0; i<clientCampaignsDict.Count(); i++)
            {
                if (startDate)
                {
                    ClientDTO client = clientCampaignsDict.ElementAt(i).Key;
                    clientCampaignsDict[client] = clientCampaignsDict[client].
                        Where(cmp => ParseDateTime(cmp.cmpsdate, cmp.cmpstime) > Clients.instance.dpStartDate.SelectedDate);
                }
                if (active)
                {
                    ClientDTO client = clientCampaignsDict.ElementAt(i).Key;
                    clientCampaignsDict[client] = clientCampaignsDict[client].Where(cmp => !cmp.active);
                }
                if (notStarted)
                {
                    ClientDTO client = clientCampaignsDict.ElementAt(i).Key;
                    clientCampaignsDict[client] = clientCampaignsDict[client].
                        Where(cmp => ParseDateTime(cmp.cmpsdate, cmp.cmpstime) < DateTime.Now);
                }
                if (finished)
                {
                    ClientDTO client = clientCampaignsDict.ElementAt(i).Key;
                    clientCampaignsDict[client] = clientCampaignsDict[client].
                        Where(cmp => ParseDateTime(cmp.cmpsdate, cmp.cmpstime) > DateTime.Now && !cmp.active);
                }
            }

            return clientCampaignsDict;
        }

        public SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> FilterClientsByPrefix(SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict, string prefix)
        {

            for (int i=0; i<clientCampaignsDict.Keys.Count; i++)
            {
                ClientDTO client = clientCampaignsDict.ElementAt(i).Key;
                var name = client.clname.ToLower();
                if (!client.clname.ToLower().StartsWith(prefix))
                {
                    clientCampaignsDict.Remove(client);
                    i--;
                }
            }

            return clientCampaignsDict;  
        }

        public SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> FilterCampaignsByPrefix(SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict, string prefix)
        {

            for (int i = 0; i < clientCampaignsDict.Keys.Count; i++)
            {
                ClientDTO client = clientCampaignsDict.ElementAt(i).Key;

                if (ClientContainsCampaignPrefix(clientCampaignsDict[client], prefix))
                {
                    clientCampaignsDict[client] = GetCampaignsByPrefix(clientCampaignsDict[client], prefix);                 
                }
                else
                {
                    clientCampaignsDict.Remove(client);
                    i--;
                }
            }

            return clientCampaignsDict;
        }

        public async Task<SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>>> FilterClientsByUsername(SortedDictionary<ClientDTO, IEnumerable<CampaignDTO>> clientCampaignsDict, string username)
        {
            var user = await _userController.GetUserByUsername(username);

            for (int i = 0; i < clientCampaignsDict.Keys.Count; i++)
            {
                ClientDTO client = clientCampaignsDict.ElementAt(i).Key;
                if (!_userClientsDict[user].Contains(client))
                {
                    clientCampaignsDict.Remove(client);
                    i--;
                }
            }

            return clientCampaignsDict;
        }

        private IEnumerable<CampaignDTO> GetCampaignsByPrefix(IEnumerable<CampaignDTO> campaigns, string prefix)
        {
            List<CampaignDTO> campaignsList = new List<CampaignDTO>();
            foreach (var campaign in campaigns)
            {
                if (campaign.cmpname.ToLower().StartsWith(prefix))
                    campaignsList.Add(campaign);
            }
            return campaignsList;
        }

        private bool ClientContainsCampaignPrefix(IEnumerable<CampaignDTO> campaigns, string prefix)
        {           
            foreach (var campaign in campaigns)
            {
                if (campaign.cmpname.ToLower().StartsWith(prefix))
                    return true;
            }
            return false;
        }
        #endregion

        #region Parsers

        private string PrintCampaignDate(CampaignDTO campaign)
        {
            DateTime startDateTime = ParseDateTime(campaign.cmpsdate, campaign.cmpstime);
            DateTime endDateTime = ParseDateTime(campaign.cmpedate, campaign.cmpetime);

            return startDateTime.ToString() + " - " + endDateTime.ToString();
        }

        private DateTime ParseDateTime(string dateString, string timeString)
        {
            int year = Int32.Parse(dateString.Substring(0, 4));
            int month = Int32.Parse(dateString.Substring(4, 2));
            int day = Int32.Parse(dateString.Substring(6, 2));

            int hours = Int32.Parse(timeString.Substring(0, 2));
            int minutes = Int32.Parse(timeString.Substring(3, 2));
            int seconds = Int32.Parse(timeString.Substring(6, 2));

            DateTime dateTime = new DateTime(year, month, day);
            dateTime = dateTime.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);

            return dateTime;
        }
        private bool IsCampaignStarted(CampaignDTO campaign)
        {
            DateTime dateTime = ParseDateTime(campaign.cmpsdate, campaign.cmpstime);

            DateTime now = DateTime.Now;
            return dateTime < now;
        }

        private bool IsCampaignExpired(CampaignDTO campaign)
        {

            DateTime dateTime = ParseDateTime(campaign.cmpedate, campaign.cmpetime);

            DateTime now = DateTime.Now;
            return dateTime < now;
        }

        #endregion

        #region Comparers
        public class ClientComparer : IComparer<ClientDTO>
        {
            public int Compare(ClientDTO client1, ClientDTO client2)
            {
                return client1.clname.CompareTo(client2.clname);
            }
        }

        public class UserComparer : IComparer<UserDTO>
        {
            public int Compare(UserDTO user1, UserDTO user2)
            {
                return user1.usrname.CompareTo(user2.usrname);
            }
        }

        #endregion
    }
}
