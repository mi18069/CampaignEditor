using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class AllUsers : Window
    {
        private readonly IAbstractFactory<AddUser> _factoryAddUser;
        private readonly IAbstractFactory<UsersAndClients> _factoryUsersAndClients;

        private UserController _userController;

        public AllUsers(IAbstractFactory<AddUser> factoryAddUser, IAbstractFactory<UsersAndClients> factoryUsersAndClients,
            IUserRepository userRepository)
        {
            InitializeComponent();
            _factoryAddUser = factoryAddUser;
            _factoryUsersAndClients = factoryUsersAndClients;

            _userController = new UserController(userRepository);
        }

        public event EventHandler<UserDTO> UserDeletedEvent;
        public event EventHandler<UserDTO> UserAuthorizationChangedEvent;

        public async Task Initialize()
        {
            await PopulateItems();
        }

        private async Task PopulateItems()
        {
            
            lbUsers.Initialize(new UsersListItem());
            lbUsers.Items.RemoveAt(0);

            var allUsers = await _userController.GetAllUsers();
            allUsers = allUsers.OrderByDescending(u => u.usrname); // it will flip after insertations

            foreach (var user in allUsers)
            {
                var item = new UsersListItem();
                item.User = user;
                item.UsrLevel = user.usrlevel;
                AddItemEvents(item);
                lbUsers.Items.Insert(0, item);
            }

            var button = lbUsers.Items[lbUsers.Items.Count - 1] as Button;
            button.Click += Button_Click;
            lbUsers.ResizeItems(lbUsers.Items);

        }

        private void AddItemEvents(UsersListItem item)
        {
            item.UserLevelSelectionChanged += CbUserLevel_SelectionChanged;
            item.MouseDoubleClick += Item_MouseDoubleClick;
            item.MouseRightButtonDown += Item_MouseRightButtonDown;
            item.BtnUnassignedClicked += Item_BtnUnassignedClicked;
        }
        private void DeleteItemEvents(UsersListItem item)
        {
            item.UserLevelSelectionChanged -= CbUserLevel_SelectionChanged;
            item.MouseDoubleClick -= Item_MouseDoubleClick;
            item.MouseRightButtonDown -= Item_MouseRightButtonDown;
            item.BtnUnassignedClicked -= Item_BtnUnassignedClicked;
        }
        private async void Item_BtnUnassignedClicked(object? sender, UserDTO e)
        {
            try
            {
                var item = sender as UsersListItem;
                await DeleteUserFromULItem(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private async void Item_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            MenuItem item = new MenuItem();
            item.Header ="Delete user";
            item.Click += async (obj, ea) =>
            {
                var item = sender as UsersListItem;
                await DeleteUserFromULItem(item);
                DeleteItemEvents(item);
            };

            menu.Items.Add(item);
            lbUsers.ContextMenu = menu;


        }

        private async Task DeleteUserFromULItem(UsersListItem item)
        {
            var user = item.User;
            if (MessageBox.Show("Are you sure you want to delete user " + user.usrname.Trim() + "?", "Message", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var f = _factoryUsersAndClients.Create();
                if (await f.DeleteUser(user))
                {
                    lbUsers.Items.Remove(item);
                    UserDeletedEvent?.Invoke(this, item.User);
                }
            }


        }

        private async void Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            var item = sender as UsersListItem;
            var username = item.lblUsername.Content.ToString()!.Trim();
            var user = await _userController.GetUserByUsername(username.Trim());

            var f = _factoryAddUser.Create();
            f.Initialize(user);
            f.ShowDialog();
            if (f.success)
            {
                item.lblUsername.Content = f.user.usrname.ToString().Trim();
                item.cbUserLevel.SelectedIndex = f.user.usrlevel;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAddUser.Create();
            f.ShowDialog();
            if (f.success)
            {
                if (f.user == null)
                {
                    return;
                }
                var item = lbUsers.Items[lbUsers.Items.Count - 2] as UsersListItem;
                item.User = f.user;
                AddItemEvents(item);
            }
            else
            {
                lbUsers.Items.RemoveAt(lbUsers.Items.Count - 2);
            }

        }

        private async void CbUserLevel_SelectionChanged(object sender, UserDTO user)
        {
            var item = sender as UsersListItem;
            user.usrlevel = item.cbUserLevel.SelectedIndex;
            try
            {
                if (MainWindow.user.usrid == user.usrid)
                {
                    MessageBox.Show("You cannot demote yourself", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    item.cbUserLevel.SelectedIndex = 0;
                    return;
                }
                await _userController.UpdateUser(new UpdateUserDTO(user));
                UserAuthorizationChangedEvent?.Invoke(this, user);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Deactivating events
        protected override void OnClosing(CancelEventArgs e)
        {
            for (int i=0; i<lbUsers.Items.Count - 1; i++)
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
