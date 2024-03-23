using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class UsersOfClient : Window, INotifyPropertyChanged
    {
        public static UsersOfClient instance;
        private ClientDTO _client = null;

        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;
        private readonly IAbstractFactory<AssignUser> _factoryAssignUser;

        private ClientController _clientController;
        private UserController _userController;
        private UserClientsController _userClientsController;

        List<UserDTO> _unassigned = new List<UserDTO>();
        List<UserDTO> _assigned = new List<UserDTO>();

        private bool _isModified;
        private bool _isUpdated = false;

        public bool IsUpdated { get; }
        public bool isModified
        {
            get { return _isModified; }
            set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    OnPropertyChanged(nameof(isModified));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public UsersOfClient(IAbstractFactory<UsersAndClients> factoryUsersAndClients,
                             IAbstractFactory<AssignUser> factoryAssignUser,
                             IClientRepository clientRepository,
                             IUserRepository userRepository,
                             IUserClientsRepository userClientsRepository)
        {
            
            instance = this;
            InitializeComponent();
            this.DataContext = this;
            _factoryUsersAndClients = factoryUsersAndClients;
            _factoryAssignUser = factoryAssignUser; 

            _clientController = new ClientController(clientRepository);
            _userController = new UserController(userRepository);
            _userClientsController = new UserClientsController(userClientsRepository);

            if (MainWindow.user.usrlevel >= 1)
            {
                lbUsers.IsEnabled = false;
            }
        }

        public event EventHandler<UserDTO> UserAuthorizationChangedEvent;
        public event EventHandler<UserDTO> UnassignedUserEvent;
        public event EventHandler<UserDTO> CancelEvent;

        public async Task Initialize(ClientDTO client)
        {
            _client = client;
            await PopulateItems();
        }

        private async Task PopulateItems()
        {
            lbUsers.Initialize(new UsersListItem());
            lbUsers.Items.RemoveAt(0);
            
            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            List<Tuple<UserDTO, int>> usersAuthorizations = (List<Tuple<UserDTO, int>>)await f.GetAllUserAuthorizationsOfClient(_client.clid);

            foreach (var userAuthorization in usersAuthorizations) 
            {
                var item = new UsersListItem();
                item.User = userAuthorization.Item1;
                item.UsrLevel = userAuthorization.Item2;
                AddItemEvents(item);                
                lbUsers.Items.Insert(0, item);
            }

            var button = lbUsers.Items[lbUsers.Items.Count - 1] as Button;
            button.Click += Button_Click;
            if (MainWindow.user.usrlevel > 0)
            {
                button.IsEnabled = false;
            }
            lbUsers.ResizeItems(lbUsers.Items);
            
        }

        private void AddItemEvents(UsersListItem item)
        {
            item.BtnUnassignedClicked += BtnUnassign_Click;
            item.UserLevelSelectionChanged += CbUserLevel_SelectionChanged;
        }

        private void DeleteItemEvents(UsersListItem item)
        {
            item.BtnUnassignedClicked -= BtnUnassign_Click;
            item.UserLevelSelectionChanged -= CbUserLevel_SelectionChanged;
        }

        private async void CbUserLevel_SelectionChanged(object sender, UserDTO user)
        {
            var item = sender as UsersListItem;
            //user.usrlevel = item.cbUserLevel.SelectedIndex;
            int userlevel = item.cbUserLevel.SelectedIndex;
            if (userlevel < user.usrlevel)
            {
                MessageBox.Show("Client authorization cannot be greater than user authorization", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                item.cbUserLevel.SelectedIndex = user.usrlevel;
                return;
            }
            try
            {
                await _userClientsController.UpdateUserClients(_client.clid, user.usrid, userlevel);
                /*await _userController.UpdateUser(new UpdateUserDTO(user));
                UserAuthorizationChangedEvent?.Invoke(this, user);*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAssignUser.Create();
            f.Initialize(_client);
            f.ShowDialog();
            if (f.isAssigned)
            {
                _assigned.Add(f.user);
                var item = lbUsers.Items[lbUsers.Items.Count - 2] as UsersListItem;
                item.User = f.user;
                AddItemEvents(item);
                isModified = true;
                await AssignUser(f.user);
            }
            else
            {
                lbUsers.Items.RemoveAt(lbUsers.Items.Count - 2);
            }
                
        }

        private async void BtnUnassign_Click(object sender, UserDTO user)
        {
            var item = sender as UsersListItem;
            _unassigned.Add(user);
            lbUsers.Items.Remove(item);
            DeleteItemEvents(item);
            await UnassignUser(user);
        }


        public async Task UnassignUser(UserDTO user)
        {
            await _factoryUsersAndClients.Create().UnassignUserFromClient(user, _client);
            isModified = true;
            UserAuthorizationChangedEvent?.Invoke(this, user);
        }
        public async Task AssignUser(UserDTO user)
        {
            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            await f.AssignUserToClient(user);
            isModified = true;

        }

        private async void btnClose_Click(object sender, RoutedEventArgs e)
        {
            /*if (isModified)
            {
                var f = _factoryUsersAndClients.Create();
                f.Initialize(_client);

                foreach (var user in _assigned)
                {
                    try
                    {
                        await f.UnassignUserFromClient(user, _client);
                    }
                    catch
                    {
                        continue;
                    }
                    
                }

                foreach (var user in _unassigned)
                {
                    try
                    {
                        await f.AssignUserToClient(user);
                    }
                    catch
                    {
                        continue;
                    }
                    
                }



            }
            CancelEvent?.Invoke(this, null);*/
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            /*if (isModified)
            {
                for (int i=0; i<lbUsers.Items.Count-1; i++)
                {
                    var item = lbUsers.Items[i] as UsersListItem;
                    if (item != null && item.authorizationChanged)
                    {
                        var user = await _userController.GetUserByUsername(item.lblUsername.Content.ToString()!.Trim());
                        user.usrlevel = item.cbUserLevel.SelectedIndex;
                        await _userController.UpdateUser(new UpdateUserDTO(user));
                    }
                }
                _isUpdated = true;
            }
            this.Close();*/
        }

        // Deactivating events
        protected override void OnClosing(CancelEventArgs e)
        {
            for (int i = 0; i < lbUsers.Items.Count - 1; i++)
            {
                UsersListItem item = lbUsers.Items[i] as UsersListItem;
                DeleteItemEvents(item);
            }
            try
            {
                var button = lbUsers.Items[lbUsers.Items.Count - 1] as Button;
                button.Click -= Button_Click;
            }
            catch
            {

            }

        }
    }

}
