using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CampaignEditor.StartupHelpers;

namespace CampaignEditor
{

    public partial class Clients : Window
    {

        private readonly IAbstractFactory<AddUser> _factoryAddUser;
        private readonly IAbstractFactory<AddClient> _factoryAddClient;
        private readonly IAbstractFactory<ClientsTreeView> _factoryClientsTreeView;
        private readonly IAbstractFactory<AddCampaign> _factoryAddCampaign;
        private ClientsTreeView _clientsTree;

        public static Clients instance;
        public string searchClientsString = "Search clients";
        public string searchCampaignsString = "Search campaigns";

        private bool clientsUpdated = false;
        private bool campaignsUpdated = false;

        public Clients(IAbstractFactory<ClientsTreeView> factoryClientsTreeView, IAbstractFactory<AddUser> factoryAddUser, 
            IAbstractFactory<AddClient> factoryAddClient, IAbstractFactory<AddCampaign> factoryAddCampaign)
        {
            InitializeComponent();
            _factoryAddUser = factoryAddUser;
            _factoryAddClient = factoryAddClient;
            _factoryClientsTreeView = factoryClientsTreeView;
            _factoryAddCampaign = factoryAddCampaign;

            instance = this;

            _clientsTree = factoryClientsTreeView.Create();
            _clientsTree.InitializeTree();

            lblUsername.Content += MainWindow.instance.user.usrname;
            tbSearchCampaigns.Text = searchCampaignsString;
            tbSearchClients.Text = searchClientsString;

            FillFilterByUsersComboBox();
        }

        private async void FillFilterByUsersComboBox()
        {
            cbUsers.Items.Add("All");
            foreach (string username in await _clientsTree.GetSupervisedUsernames())
            {
                cbUsers.Items.Add(username);
            }
        }

        private void tbSearchClients_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ( tbSearchClients.Foreground == Brushes.LightGray)
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

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            _factoryAddUser.Create().Show();
        }

        private void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            _factoryAddClient.Create().Show();
        }

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

        private void btnAddCampaign_Click(object sender, RoutedEventArgs e)
        {
            _factoryAddCampaign.Create().Show();
        }

        private void cbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _clientsTree.UpdateTree();
        }
    }
}
