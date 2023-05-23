using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using CampaignEditor.StartupHelpers;
using Database.DTOs.MediaPlanDTO;
using Database.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            item.MouseDoubleClick += Item_MouseDoubleClick;
            item.MouseRightButtonDown += Item_MouseRightButtonDown;
        }

        private async void Item_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            MenuItem item = new MenuItem();
            item.Header ="Delete user";
            item.Click += async (obj, ea) =>
            {
                DeleteUserFromULItem(sender, e);
            };

            menu.Items.Add(item);
            lbUsers.ContextMenu = menu;


        }

        private void DeleteUserFromULItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as UsersListItem;
            var username = item.lblUsername.Content.ToString()!.Trim();

            if (MessageBox.Show("Are you sure you want to delete user " + username + "?", "Message", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var f = _factoryUsersAndClients.Create();
                f.DeleteUserByUsername(username);
                lbUsers.Items.Remove(item);
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
            if (f.isAdded)
            {
                item.lblUsername.Content = f.user.usrname.ToString().Trim();
                item.cbUserLevel.SelectedIndex = f.user.usrlevel;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAddUser.Create();
            f.ShowDialog();
            if (f.isAdded)
            {
                var item = lbUsers.Items[lbUsers.Items.Count - 2] as UsersListItem;
                FillFields(item, f.user);
                AddItemEvents(item);
            }
            else
            {
                lbUsers.Items.RemoveAt(lbUsers.Items.Count - 2);
            }

        }

        private async void CbUserLevel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
            var user = await _userController.GetUserByUsername(username.Trim());
            user.usrlevel = item.cbUserLevel.SelectedIndex;
            await _userController.UpdateUser(new UpdateUserDTO(user));
        }

        private void FillFields(UsersListItem item, UserDTO user)
        {
            item.lblUsername.Content = user.usrname.Trim();
            item.cbUserLevel.SelectedIndex = user.usrlevel;
            item.authorizationChanged = false;
            item.btnUnassign.Visibility = Visibility.Collapsed;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
