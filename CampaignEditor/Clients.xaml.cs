﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;

namespace CampaignEditor
{

    public partial class Clients : Window, INotifyPropertyChanged
    {

        private readonly IAbstractFactory<AddUser> _factoryAddUser;
        private readonly IAbstractFactory<UsersOfClient> _factoryUsersOfClient;
        private readonly IAbstractFactory<AddClient> _factoryAddClient;
        private readonly IAbstractFactory<ClientsTreeView> _factoryClientsTreeView;
        private readonly IAbstractFactory<AddCampaign> _factoryAddCampaign;
        private readonly IAbstractFactory<NewCampaign> _factoryNewCampaign;
        private readonly IAbstractFactory<Rename> _factoryRename;

        private ClientsTreeView _clientsTree;

        public static Clients instance;
        public string searchClientsString = "Search clients";
        public string searchCampaignsString = "Search campaigns";

        private bool clientsUpdated = false;
        private bool campaignsUpdated = false;

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
        public bool isAdministrator { get; set; }  = false;
        public bool isReadWrite { get; set;  } = false;

        public Clients(IAbstractFactory<ClientsTreeView> factoryClientsTreeView, IAbstractFactory<AddUser> factoryAddUser,
            IAbstractFactory<AddClient> factoryAddClient, IAbstractFactory<AddCampaign> factoryAddCampaign,
            IAbstractFactory<UsersOfClient> factoryUsersOfClient, IAbstractFactory<NewCampaign> factoryNewCampaign,
            IAbstractFactory<Rename> factoryRename)
        {
            InitializeComponent();
            this.DataContext = this;
            _factoryAddUser = factoryAddUser;
            _factoryAddClient = factoryAddClient;
            _factoryClientsTreeView = factoryClientsTreeView;
            _factoryAddCampaign = factoryAddCampaign;
            _factoryUsersOfClient = factoryUsersOfClient;
            _factoryNewCampaign = factoryNewCampaign;
            _factoryRename = factoryRename;

            instance = this;

            _clientsTree = factoryClientsTreeView.Create();
            _clientsTree.Initialization(MainWindow.user);
            _ = _clientsTree.InitializeTree();

            
            isAdministrator = MainWindow.user.usrlevel == 0;
            isReadWrite = MainWindow.user.usrlevel == 1 ? true : isAdministrator;

            lblUsername.Content += MainWindow.user.usrname;

            tbSearchCampaigns.Text = searchCampaignsString;
            tbSearchClients.Text = searchClientsString;

            FillFilterByUsersComboBox();
        }

        #region Filter ComboBox
        private async void FillFilterByUsersComboBox()
        {
            cbUsers.Items.Add("All");
            foreach (string username in await _clientsTree.GetSupervisedUsernames())
            {
                cbUsers.Items.Add(username);
            }
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

        private void tbSearchClients_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearchClients.Text.Length >= 3 && tbSearchClients.Text != searchClientsString)
            {
                _clientsTree.UpdateTree();
                clientsUpdated = true;
            }
            else
            {
                if (clientsUpdated)
                {
                    _clientsTree.UpdateTree();
                    clientsUpdated = false;
                }

            }
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
            if (tbSearchCampaigns.Text.Length >= 3 && tbSearchCampaigns.Text != searchCampaignsString)
            {
                _clientsTree.UpdateTree();
                campaignsUpdated = true;
            }
            else
            {
                if (campaignsUpdated)
                {
                    _clientsTree.UpdateTree();
                    campaignsUpdated = false;
                }

            }
        }

        #endregion

        #region Filters
        private void dpStartDate_Initialized(object sender, System.EventArgs e)
        {
            dpStartDate.SelectedDate = DateTime.Now.AddYears(-1);
        }

        private void cbDatePicker_Checked(object sender, RoutedEventArgs e)
        {
            if (_clientsTree != null)
                _clientsTree.UpdateTree();
            dpStartDate.IsEnabled = true;
        }

        private void cbDatePicker_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_clientsTree != null)
                _clientsTree.UpdateTree();
            dpStartDate.IsEnabled = false;
        }

        private void dpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_clientsTree != null)
                _clientsTree.UpdateTree();
        }

        private void cbFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (_clientsTree != null)
                _clientsTree.UpdateTree();
        }

        private void cbFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_clientsTree != null)
                _clientsTree.UpdateTree();
        }


        private void cbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _clientsTree.UpdateTree();
        }

        #endregion

        #region Context menu mechanism
        // For creating specific context menus based on selected item
        private async void tvClients_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem SelectedItem = tvClients.SelectedItem as TreeViewItem;
            if (SelectedItem is null)
                return;
            switch (SelectedItem.Tag.ToString())
            {
                case "Client":
                    await UpdateCanBeDeleted();
                    tvClients.ContextMenu = tvClients.Resources["ClientContext"] as System.Windows.Controls.ContextMenu;
                    break;
                case "Campaign":
                    tvClients.ContextMenu = tvClients.Resources["CampaignContext"] as System.Windows.Controls.ContextMenu;
                    break;
            }
        }

        private async Task UpdateCanBeDeleted()
        {
            string clientname = ((HeaderedItemsControl)tvClients.SelectedItem).Header.ToString()!.Trim();
            CanBeDeleted = await _clientsTree.CheckCanBeDeleted(clientname);
        }

        // In order to select TreeViewItem on mouse right click
        private void tvClients_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        
        #endregion

        #region Context menu options
        public void btnAddCampaign_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAddCampaign.Create();
            string campaignName = ((HeaderedItemsControl)tvClients.SelectedItem).Header.ToString()!.Trim();
            f.Initialization(campaignName);
            f.Show();
        }

        private async void btnNewCampaign_Click(object sender, RoutedEventArgs e)
        {
            string clientname = ((HeaderedItemsControl)tvClients.SelectedItem).Header.ToString()!.Trim();
            var f = _factoryNewCampaign.Create();
            f.Initialize(clientname);
            f.ShowDialog();
            await _clientsTree.UpdateTree();
        }

        private void btnShowUsersOfClient_Click(object sender, RoutedEventArgs e)
        {
            _factoryUsersOfClient.Create().Show();
        }
        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAddUser.Create();
            f.Show();
        }

        private async void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryAddClient.Create();
            f.ShowDialog();
            if (f.success)
                await _clientsTree.InitializeTree();
        }
        private async void btnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            string clientname = ((HeaderedItemsControl)tvClients.SelectedItem).Header.ToString()!.Trim();
            await _clientsTree.DeleteClient(clientname);
        }


        #endregion

        private async void btnRenameClient_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryRename.Create();
            MenuItem menuItem = (MenuItem)sender;
            string name = ((TreeViewItem)tvClients.SelectedValue).Header.ToString().Trim();
            f.RenameClient(name);
            f.ShowDialog();
            if (f.renamed)
            {
                await _clientsTree.InitializeTree();
            }
        }

        private async void btnRenameCampaign_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryRename.Create();
            MenuItem menuItem = (MenuItem)sender;
            string name = ((TreeViewItem)tvClients.SelectedValue).Header.ToString().Trim();
            f.RenameCampaign(name);
            f.ShowDialog();
            if (f.renamed)
            {
                await _clientsTree.InitializeTree();
            }
        }
    }
}
