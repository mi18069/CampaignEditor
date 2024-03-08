using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Entities;
using CampaignEditor.Helpers;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Entities;
using Database.Repositories;

namespace CampaignEditor
{

    public partial class Clients : Window, INotifyPropertyChanged
    {
        // to keep a list of opened campaigns, we don't want the same campaign to be opened twice
        List<Campaign> openCampaignWindows = new List<Campaign>();

        private readonly IAbstractFactory<AddUser> _factoryAddUser;
        private readonly IAbstractFactory<UsersOfClient> _factoryUsersOfClient;
        private readonly IAbstractFactory<AddClient> _factoryAddClient;
        private readonly IAbstractFactory<NewCampaign> _factoryNewCampaign;
        private readonly IAbstractFactory<Rename> _factoryRename;
        private readonly IAbstractFactory<Campaign> _factoryCampaign;
        private readonly IAbstractFactory<AllUsers> _factoryAllUsers;
        private readonly IAbstractFactory<ChangePassword> _factoryChangePassword;
        private readonly IAbstractFactory<DuplicateCampaign> _factoryDuplicateCampaign;
        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;

        private UsersAndClients _usersAndClients;
        private CampaignManipulations _campaignManipulations;

        private UserController _userController;
        private CampaignController _campaignController;


        //private ClientsTreeView _clientsTree;

        public static Clients instance;
        public string searchClientsString = "Search clients";
        public string searchCampaignsString = "Search campaigns";

        private bool clientsUpdated = false;
        private bool campaignsUpdated = false;

        bool loadedAllCheckBoxes = false;

        #region Can Be Deleted

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _canBeDeleted = false;
        public bool CanBeDeleted
        {
            get { return _canBeDeleted; }
            set
            {
                _canBeDeleted = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        // Variables in order to determine wheather some of the context men uoptions should be disabled
        public bool isAdministrator { get; set; } = false;
        public bool isReadWrite { get; set; } = false;
        public bool isReadOnly { get; set; } = false;
        public Clients(IAbstractFactory<AddUser> factoryAddUser,
            IAbstractFactory<AddClient> factoryAddClient, IAbstractFactory<UsersOfClient> factoryUsersOfClient, IAbstractFactory<NewCampaign> factoryNewCampaign,
            IAbstractFactory<Rename> factoryRename, IAbstractFactory<Campaign> factoryCampaign, ICampaignRepository campaignRepository, IAbstractFactory<AllUsers> factoryAllUsers,
            IAbstractFactory<ChangePassword> factoryChangePassword, IAbstractFactory<DuplicateCampaign> factoryDuplicateCampaign,
            IUserRepository userRepository, IClientRepository clientRepository,
            IUserClientsRepository userClientsRepository, 
            IAbstractFactory<UsersAndClients> factoryUsersAndClients, IAbstractFactory<CampaignManipulations> factoryCampaignManipulations)
        {
            instance = this;
            this.DataContext = this;

            InitializeComponent();

            _factoryAddUser = factoryAddUser;
            _factoryAddClient = factoryAddClient;
            _factoryUsersOfClient = factoryUsersOfClient;
            _factoryNewCampaign = factoryNewCampaign;
            _factoryRename = factoryRename;
            _factoryCampaign = factoryCampaign;
            _campaignController = new CampaignController(campaignRepository);


            isAdministrator = MainWindow.user.usrlevel == 0;
            isReadWrite = MainWindow.user.usrlevel == 1 ? true : isAdministrator;
            isReadOnly = MainWindow.user.usrlevel == 2;

            tbUsername.Text = MainWindow.user.usrname.Trim();

            tbSearchCampaigns.Text = searchCampaignsString;
            tbSearchClients.Text = searchClientsString;

            _factoryAllUsers = factoryAllUsers;
            _factoryChangePassword = factoryChangePassword;
            _factoryDuplicateCampaign = factoryDuplicateCampaign;

            _usersAndClients = factoryUsersAndClients.Create();
            _campaignManipulations = factoryCampaignManipulations.Create();

            _userController = new UserController(userRepository);
            tvClients._userController = _userController;
            tvClients._clientController = new ClientController(clientRepository);
            tvClients._userClientsController = new UserClientsController(userClientsRepository);
            tvClients._campaignController = new CampaignController(campaignRepository);

            SubscribeEventsToTvClients();

            FillFilterByUsersComboBox();

        }

        private void SubscribeEventsToTvClients()
        {
            tvClients.ClientContextMenuEvent += TvClient_ClientContextMenuEvent;
            tvClients.UserContextMenuEvent += TvUser_UserContextMenuEvent;
            tvClients.CampaignContextMenuEvent += TvCampaign_CampaignContextMenuEvent;
        }

        private void TvCampaign_CampaignContextMenuEvent(object? sender, CampaignContextMenuEventArgs e)
        {
            Enum option = e.Option;
            switch (option)
            {
                case CampaignContextMenuEventArgs.Options.EditCampaign:
                    ShowCampaign(e.Campaign);
                    break;
                case CampaignContextMenuEventArgs.Options.RenameCampaign:
                    ShowRenameCampaign(e.Campaign);
                    break;
                case CampaignContextMenuEventArgs.Options.DeleteCampaign:
                    ShowDeleteCampaign(e.Campaign);
                    break;
                case CampaignContextMenuEventArgs.Options.DuplicateCampaign:
                    ShowDuplicateCampaign(e.Campaign);
                    break;
            }
        }

        private void TvUser_UserContextMenuEvent(object? sender, UserContextMenuEventArgs e)
        {
            Enum option = e.Option;
            switch (option)
            {
                case UserContextMenuEventArgs.Options.AllUsers:
                    ShowAllUsers();
                    break;
                case UserContextMenuEventArgs.Options.NewUser:
                    ShowNewUser();
                    break;
                case UserContextMenuEventArgs.Options.NewClient:
                    ShowNewClient();
                    break;
            }
        }

        private void TvClient_ClientContextMenuEvent(object? sender, Helpers.ClientContextMenuEventArgs e)
        {
            Enum option = e.Option;
            switch ( option ) 
            {
                case ClientContextMenuEventArgs.Options.UsersOfClient:
                    ShowUsersOfClient(e.Client);
                    break;
                case ClientContextMenuEventArgs.Options.NewCampaign:
                    ShowNewCampaign(e.Client);
                    break;
                case ClientContextMenuEventArgs.Options.RenameClient:
                    ShowRenameClient(e.Client);
                    break;
                case ClientContextMenuEventArgs.Options.DeleteClient:
                    ShowDeleteClient(e.Client);
                    break;
            }
        }

        #region Context menu options
        public async void ShowCampaign(CampaignDTO campaign)
        {
            // If campaign window is minimized, show it, if campaign is not open,
            // make new and place it in list of open campaigns 
            if (openCampaignWindows.Any(w => w._campaign.cmpid == campaign.cmpid))
            {
                Campaign window = openCampaignWindows.First(w => w._campaign.cmpid == campaign.cmpid);
                if (!window.IsVisible)
                {
                    window.Show();
                }

                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }

                // For setting window in front
                window.Topmost = true;
                window.Topmost = false;

            }
            else
            {
                var f = _factoryCampaign.Create();
                await f.Initialize(campaign, isReadOnly);
                openCampaignWindows.Add(f);

                f.Closing += CampaignWindow_Closing;
                f.Show();
                f.Activate();

            }

        }

        // remove from list of opened campaigns
        private void CampaignWindow_Closing(object sender, EventArgs e)
        {
            Campaign window = (Campaign)sender;
            openCampaignWindows.Remove(window);

        }

        private async void ShowNewCampaign(ClientDTO client)
        {
            var f = _factoryNewCampaign.Create();
            await f.Initialize(client.clid);
            f.ShowDialog();
            if (f.success)
            {
                if (f._campaign != null)
                    await tvClients.AddCampaign(f._campaign);
            }

        }

        private async void ShowUsersOfClient(ClientDTO client)
        {
            var f = _factoryUsersOfClient.Create();
            await f.Initialize(client);
            f.UnassignedUserEvent += Users_UserAuthorizationChangedEvent;
            f.UserAuthorizationChangedEvent += Users_UserAuthorizationChangedEvent;
            f.ShowDialog();
            f.UnassignedUserEvent -= Users_UserAuthorizationChangedEvent;
            f.UserAuthorizationChangedEvent -= Users_UserAuthorizationChangedEvent;
        }

        private async void ShowAllUsers()
        {
            var f = _factoryAllUsers.Create();
            await f.Initialize();
            f.UserDeletedEvent += Users_UserDeletedEvent;
            f.UserAuthorizationChangedEvent += Users_UserAuthorizationChangedEvent;
            f.ShowDialog();
            f.UserDeletedEvent -= Users_UserDeletedEvent;
            f.UserAuthorizationChangedEvent -= Users_UserAuthorizationChangedEvent;
        }

        private async void Users_UserAuthorizationChangedEvent(object? sender, UserDTO user)
        {
            int selectedIndex = cbUsers.SelectedIndex;
            int foundIndex = -1;

            for (int i = 0; i < cbUsers.Items.Count; i++)
            {
                if (cbUsers.Items[i].ToString().Trim() == user.usrname.Trim())
                {
                    foundIndex = i;
                    break;
                }
            }

            if (selectedIndex == foundIndex)
            {
                await tvClients.Initialize(user);
            }
        }

        private async void Users_UserDeletedEvent(object? sender, UserDTO user)
        {
            int selectedIndex = cbUsers.SelectedIndex;
            int foundIndex = -1;

            for (int i=0; i<cbUsers.Items.Count; i++)
            {
                if (cbUsers.Items[i].ToString().Trim() == user.usrname.Trim())
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1)
            {
                cbUsers.Items.RemoveAt(foundIndex);
            }

            if (selectedIndex == foundIndex)
            {
                await tvClients.Initialize(user);

            }
        }

        private void ShowNewUser()
        {
            var f = _factoryAddUser.Create();
            f.ShowDialog();
            if (f.success)
            {
                if (f.user == null)
                {
                    return;
                }
                UserDTO user = f.user;
                cbUsers.Items.Insert(1, user.usrname);
            }
        }

        private void ShowNewClient()
        {
            var f = _factoryAddClient.Create();
            f.ShowDialog();
            if (f.success && f.newClient != null)
                tvClients.AddClient(f.newClient);
        }

        private async void ShowDeleteClient(ClientDTO client)
        {
            if (MessageBox.Show($"Are you sure you want to delete Client {client.clname.Trim()}?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            if (!await _usersAndClients.CanClientBeDeleted(client))
            {
                MessageBox.Show("Cannot delete Client that have active campaigns!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(await _usersAndClients.DeleteClient(client)))
            {
                MessageBox.Show($"Cannot delete Client {client.clname.Trim()}!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            tvClients.RemoveClient(client);

        }

        private async void ShowRenameClient(ClientDTO client)
        {
            var f = _factoryRename.Create();
            await f.RenameClient(client);
            f.ShowDialog();
            if (f.renamed)
            {
                tvClients.RenameClient(f._client);
            }
        }

        private async void ShowRenameCampaign(CampaignDTO campaign)
        {
            var f = _factoryRename.Create();
            await f.RenameCampaign(campaign);
            f.ShowDialog();
            if (f.renamed)
            {
                await tvClients.RenameCampaign(campaign);
            }
        }

        private async void ShowDeleteCampaign(CampaignDTO campaign)
        {
            if (await _campaignManipulations.DeleteCampaign(campaign))
            {
                await tvClients.RemoveCampaign(campaign);
            }


        }

        private async void ShowDuplicateCampaign(CampaignDTO campaign)
        {
            var f = _factoryNewCampaign.Create();
            await f.Initialize(campaign.clid, campaign);
            f.ShowDialog();
            if (f.success)
            {
                if (f._campaign != null)
                {
                    await tvClients.AddCampaign(f._campaign);
                    if (!await _campaignManipulations.DuplicateCampaign(campaign, f._campaign))
                    {
                        // Delete campaign if duplication is unsuccessful
                        if (await _campaignManipulations.DeleteCampaign(campaign))
                        {
                            await tvClients.RemoveCampaign(campaign);
                        }
                    }
                }
            }

        }


        #endregion

        #region Filter ComboBox
        private async void FillFilterByUsersComboBox()
        {
            cbUsers.Items.Add("All");
            var usernames = await GetSupervisedUsernames(MainWindow.user);
            usernames = usernames.OrderBy(u => u);
            foreach (string username in usernames)
            {
                cbUsers.Items.Add(username);
            }

        }

        private async Task<IEnumerable<string>> GetSupervisedUsernames(UserDTO user)
        {
            IEnumerable<UserDTO> users = new List<UserDTO>();

            if (user.usrlevel == 0)
            {
                users = await _userController.GetAllUsers();
            }
            else
            {
                users = users.Append(await _userController.GetUserById(user.usrid));
            }
            IEnumerable<string> usernames = new List<string>();

            foreach (UserDTO userDTO in users)
            {
                usernames = usernames.Append(userDTO.usrname);
            }
            return usernames;
        }

        #endregion

        #region Filter text boxes
        private void tbSearchClients_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (tbSearchClients.Foreground == Brushes.LightGray)
            {
                tbSearchClients.Text = "";
                tbSearchClients.Foreground = Brushes.Black;
            }

        }

        private void tbSearchClients_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearchClients.Text == "")
            {
                tbSearchClients.Text = searchClientsString;
                tbSearchClients.Foreground = Brushes.LightGray;
            }

        }

        private async void tbSearchClients_TextChanged(object sender, TextChangedEventArgs e)
        {
            tvClients.FilterClientAndCampaign();

        }

        private void tbSearchCampaigns_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (tbSearchCampaigns.Foreground == Brushes.LightGray)
            {
                tbSearchCampaigns.Text = "";
                tbSearchCampaigns.Foreground = Brushes.Black;
            }

        }

        private void tbSearchCampaigns_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearchCampaigns.Text == "")
            {
                tbSearchCampaigns.Text = searchCampaignsString;
                tbSearchCampaigns.Foreground = Brushes.LightGray;
            }
        }

        private void tbSearchCampaigns_TextChanged(object sender, TextChangedEventArgs e)
        {
            tvClients.FilterClientAndCampaign();
        }

        #endregion

        #region Filters

        private void tbtnTV_Click(object sender, RoutedEventArgs e)
        {
            if (loadedAllCheckBoxes)
                tvClients.FilterData();
        }

        private void tbtnRadio_Click(object sender, RoutedEventArgs e)
        {
            if (loadedAllCheckBoxes)
                tvClients.FilterData();
        }

        private void dpStartDate_Initialized(object sender, System.EventArgs e)
        {
            dpStartDate.SelectedDate = DateTime.Now.AddYears(-1);

        }

        private void cbDatePicker_Checked(object sender, RoutedEventArgs e)
        {
            dpStartDate.IsEnabled = true;
            if (loadedAllCheckBoxes)
                tvClients.FilterData();

        }

        private void cbDatePicker_Unchecked(object sender, RoutedEventArgs e)
        {
            dpStartDate.IsEnabled = false;
            if (loadedAllCheckBoxes)
                tvClients.FilterData();
        }

        private void dpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loadedAllCheckBoxes)
                tvClients.FilterData();

        }

        private void cbFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (loadedAllCheckBoxes)
                tvClients.FilterData();

        }

        private void cbFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (loadedAllCheckBoxes)
                tvClients.FilterData();

        }


        private async void cbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string username;
            if (cbUsers.SelectedValue == null)
            {
                username = "All";
            }
            else
            {
                username = ((string)cbUsers.SelectedValue).Trim();
            }


            if (username == null || username == "All")
                await tvClients.Initialize();
            else
            {
                UserDTO user = await _userController.GetUserByUsername(username);
                await tvClients.Initialize(user);
            }

        }

        #endregion      
    
        


        private void Window_Closing(object sender, CancelEventArgs e)
        {

            if (MessageBox.Show("Application will close\nAre you sure you want to exit?", "Message: ",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }

        }

        private void btnPassChange_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryChangePassword.Create();
            f.Initialize(MainWindow.user);
            f.ShowDialog();
        }

        int checkboxesToLoad = 7;
        int loadedForNow = 0;
        private async void cb_Loaded(object sender, RoutedEventArgs e)
        {
            loadedForNow += 1;
            if (checkboxesToLoad == loadedForNow)
            {
                loadedAllCheckBoxes = true;
                await tvClients.Initialize(MainWindow.user);
            }
        }

        // Deactivating events
        /*protected override void OnClosing(CancelEventArgs e)
        {
            tvClients.ClientContextMenuEvent -= TvClient_ClientContextMenuEvent;
            tvClients.UserContextMenuEvent -= TvUser_UserContextMenuEvent;
            tvClients.CampaignContextMenuEvent -= TvCampaign_CampaignContextMenuEvent;
        }*/




    }
}