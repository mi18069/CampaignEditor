using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.Repositories;
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

        List<string> _unassigned = new List<string>();
        List<string> _assigned = new List<string>();

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
                             IUserRepository userRepository)
        {
            
            instance = this;
            InitializeComponent();
            this.DataContext = this;
            _factoryUsersAndClients = factoryUsersAndClients;
            _factoryAssignUser = factoryAssignUser; 

            _clientController = new ClientController(clientRepository);
            _userController = new UserController(userRepository);

            if (MainWindow.user.usrlevel == 2)
            {
                this.IsEnabled = false;
            }
        }


        public async Task Initialize(string clientname)
        {
            _client = await _clientController.GetClientByName(clientname);
            await PopulateItems();

        }

        private async Task PopulateItems()
        {
            lbUsers.Initialize(new UsersListItem());
            lbUsers.Items.RemoveAt(0);
            
            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            List<UserDTO> users = (List<UserDTO>)await f.GetAllUsersOfClient();
            users = users.OrderBy(u => u.usrname).ToList();

            foreach (var user in users) 
            {
                var item = new UsersListItem();
                FillFields(item, user);
                AddItemEvents(item);                
                lbUsers.Items.Insert(0, item);
            }

            var button = lbUsers.Items[lbUsers.Items.Count - 1] as Button;
            button.Click += Button_Click;
            lbUsers.ResizeItems(lbUsers.Items);
            
        }

        private void AddItemEvents(UsersListItem item)
        {
            item.cbUserLevel.SelectionChanged += CbUserLevel_SelectionChanged;
            item.btnUnassign.Click += BtnUnassign_Click;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAssignUser.Create();
            f.Initialize(_client);
            f.ShowDialog();
            if (f.isAssigned)
            {
                _assigned.Add(f.user.usrname);
                var item = lbUsers.Items[lbUsers.Items.Count - 2] as UsersListItem;
                FillFields(item, f.user);
                AddItemEvents(item);
                isModified = true;
                await AssignUser(f.user.usrname);
            }
            else
            {
                lbUsers.Items.RemoveAt(lbUsers.Items.Count - 2);
            }
                
        }

        private async void BtnUnassign_Click(object sender, RoutedEventArgs e)
        {
            // Traverse the visual tree to find the DataGridRow and DataGridCell that contain the selected cell
            DependencyObject parent = VisualTreeHelper.GetParent((DependencyObject)sender);
            while (parent != null && !(parent is UsersListItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent == null)
            {
                return; // error
            }
            var item = parent as UsersListItem;
            var username = item.lblUsername.Content.ToString()!.Trim();
            _unassigned.Add(username);
            lbUsers.Items.Remove(item);
            isModified = true;
            await UnassignUser(username);
        }

        private void CbUserLevel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            isModified = true;
        }

        private void FillFields(UsersListItem item, UserDTO user)
        {
            item.lblUsername.Content = user.usrname.Trim();
            item.cbUserLevel.SelectedIndex = user.usrlevel;
            item.authorizationChanged = false;
        }

        public async Task UnassignUser(string username)
        {
            await _factoryUsersAndClients.Create().UnassignUserFromClient(username.Trim(), _client.clname.Trim());
            isModified = true;
        }
        public async Task AssignUser(string username)
        {
            var f = _factoryUsersAndClients.Create();
            f.Initialize(_client);
            await f.AssignUserToClient(username);
            isModified = true;

        }

        private async void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var f = _factoryUsersAndClients.Create();
                f.Initialize(_client);

                foreach (var username in _assigned)
                {
                    try
                    {
                        await f.UnassignUserFromClient(username.Trim(), _client.clname.Trim());
                    }
                    catch
                    {
                        continue;
                    }
                    
                }

                foreach (var username in _unassigned)
                {
                    try
                    {
                        await f.AssignUserToClient(username.Trim());
                    }
                    catch
                    {
                        continue;
                    }
                    
                }



            }
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
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
            this.Close();
        }
    }

}
