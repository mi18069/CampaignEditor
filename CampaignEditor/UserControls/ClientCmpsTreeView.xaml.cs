using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.UserClients;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ClientCmpsTreeView.xaml
    /// </summary>
    public class TreeViewData
    {
        public int UserLevel { get; set; } = MainWindow.user.usrlevel;

        public ObservableCollection<ClientCampaignsList>? collection;

    }
    public partial class ClientCmpsTreeView : UserControl
    {
        public UserController _userController;
        public ClientController _clientController;
        public UserClientsController _userClientsController;
        public CampaignController _campaignController;

        public TreeViewData tvd;

        private Clients clientsInstance = Clients.instance;

        ObservableCollection<ClientCampaignsList> clientCampaignsLists = new ObservableCollection<ClientCampaignsList>();
        ObservableCollection<ClientCampaignsList> currentFilteredData = new ObservableCollection<ClientCampaignsList>();

        public ClientCmpsTreeView()
        {
            this.DataContext = this.tvd;
            tvd = new TreeViewData();
            // Default constructor for XAML
            InitializeComponent();

        }

        public async Task Initialize(UserDTO userDTO = null)
        {

            clientCampaignsLists.Clear();
            /*var clids = new List<int>();
            if (userDTO == null || userDTO.usrlevel <= 0)
            {
                clids = (await _userClientsController.GetAllUserClients()).Select(c => c.cliid).ToList();
                clids = clids.Distinct().ToList();
            }
            else
                clids = (await _userClientsController.GetAllUserClientsByUserId(userDTO.usrid)).Select(c => c.cliid).ToList();*/

            var clids = new List<int>();
            if (userDTO == null || userDTO.usrlevel < 0)
            {
                clids = (await _clientController.GetAllClients()).Select(client => client.clid).ToList();
            }
            else
                clids = (await _userClientsController.GetAllUserClientsByUserId(userDTO.usrid)).Select(c => c.cliid).ToList();

            List<ClientCampaignsList> ccList = new List<ClientCampaignsList>();

            foreach (var clid in clids)
            {
                var client = await _clientController.GetClientById(clid);

                // Skip inactive clients
                if (!client.clactive)
                    continue;

                var campaigns = await _campaignController.GetCampaignsByClientId(client.clid);
                campaigns = campaigns.OrderByDescending(cmp => cmp.cmpsdate);

                var clientCampaigns = new ClientCampaignsList { Client = client };
                foreach (var campaign in campaigns)
                {
                    clientCampaigns.Campaigns.Add(campaign);
                }
                ccList.Add(clientCampaigns);
            }
            ccList = ccList.OrderBy(cc => cc.Client.clname).ToList();
            foreach (var cc in ccList)
            {
                clientCampaignsLists.Add(cc);
            }

            FilterData();
        }

        private ObservableCollection<ClientCampaignsList> CopyObservable(IEnumerable<ClientCampaignsList> listToCopy)
        {
            var newList = new ObservableCollection<ClientCampaignsList>();
            foreach (var cc in listToCopy)
            {
                var newCC = new ClientCampaignsList { Client = cc.Client };
                foreach (var campaign in cc.Campaigns)
                {
                    // maybe this needs to have new
                    newCC.Campaigns.Add(campaign);
                }
                newList.Add(newCC);
            }
            return newList;
        }

        public void FilterData()
        {
            currentFilteredData = CopyObservable(clientCampaignsLists);

            bool applyTvFilter = false;
            bool applyRadioFilter = false;
            bool applySinceFilter = false;
            bool applyActiveFilter = false;
            bool applyNotStartedFilter = false;
            bool applyFinishedFilter = false;

            if (clientsInstance.tbtnTV.IsChecked == false)
            {
                applyTvFilter = true;
            }
            if (clientsInstance.tbtnRadio.IsChecked == false)
            {
                applyRadioFilter = true;
            }
            if (clientsInstance.cbDatePicker.IsChecked == true && clientsInstance.dpStartDate.SelectedDate.HasValue)
            {
                applySinceFilter = true;
            }
            if (clientsInstance.cbShowActive.IsChecked == true)
            {
                applyActiveFilter = true;
            }
            if (clientsInstance.cbShowNotStarted.IsChecked == true)
            {
                applyNotStartedFilter = true;
            }
            if (clientsInstance.cbShowFinished.IsChecked == true)
            {
                applyFinishedFilter = true;
            }

            ApplyFilters(currentFilteredData, applyTvFilter, applyRadioFilter, applySinceFilter, applyActiveFilter, applyNotStartedFilter, applyFinishedFilter);
            FilterClientAndCampaign();
        }

        public void FilterClientAndCampaign()
        {
            string clientSearchString = Clients.instance.tbSearchClients.Text;
            string campaignSearchString = Clients.instance.tbSearchCampaigns.Text;

            List<ClientCampaignsList> filteredData = CopyObservable(currentFilteredData).ToList();

            if (clientSearchString.Length >= 3 && clientSearchString != Clients.instance.searchClientsString)
            {
                string prefix = Clients.instance.tbSearchClients.Text.Trim().ToLower();
                filteredData = filteredData.Where(cc => cc.Client.clname.ToLower().StartsWith(prefix)).ToList();

            }
            if (campaignSearchString.Length >= 3 && campaignSearchString != Clients.instance.searchCampaignsString)
            {
                string prefix = Clients.instance.tbSearchCampaigns.Text.Trim().ToLower();
                for (int i = 0; i < filteredData.Count; i++)
                {
                    var clientCampaigns = filteredData[i];
                    var newCampaigns = new List<ClientCampaignsList>();
                    var campaigns = clientCampaigns.Campaigns.Where(cmp => cmp.cmpname.ToLower().StartsWith(prefix)).ToList();
                    if (campaigns.Count == 0)
                    {
                        filteredData.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        clientCampaigns.Campaigns.Clear();
                        foreach (var campaign in campaigns)
                        {
                            clientCampaigns.Campaigns.Add(campaign);
                        }
                    }
                }
            }

            var dataToFilter = CopyObservable(filteredData);
            tvd.collection = dataToFilter;
            _tvUsers.ItemsSource = tvd.collection;
        }

        private void ApplyFilters(ObservableCollection<ClientCampaignsList> dataToFilter, bool applyTvFilter, bool applyRadioFilter, bool applySinceFilter, bool applyActiveFilter, bool applyNotStartedFilter, bool applyFinishedFilter)
        {

            foreach (var cc in dataToFilter)
            {
                var filteredCampaigns = cc.Campaigns.ToList();
                if (applyTvFilter)
                {
                    filteredCampaigns = filteredCampaigns.Where(cmp => cmp.tv == false).ToList();
                }
                if (applyRadioFilter)
                {
                    filteredCampaigns = filteredCampaigns.Where(cmp => cmp.tv == true).ToList();
                }
                if (applySinceFilter)
                {
                    DateTime date = clientsInstance.dpStartDate.SelectedDate!.Value;
                    filteredCampaigns = filteredCampaigns.Where(cmp => TimeFormat.YMDStringToDateTime(cmp.cmpsdate) > date).ToList();
                }
                if (!applyActiveFilter)
                {
                    filteredCampaigns = filteredCampaigns.Where(cmp => cmp.active != true).ToList();
                }
                if (!applyNotStartedFilter)
                {
                    filteredCampaigns = filteredCampaigns.Where(cmp => TimeFormat.YMDStringToDateTime(cmp.cmpsdate) < DateTime.Now).ToList();
                }
                if (!applyFinishedFilter)
                {
                    filteredCampaigns = filteredCampaigns.Where(cmp => TimeFormat.YMDStringToDateTime(cmp.cmpedate) > DateTime.Now).ToList();
                }
                cc.Campaigns.Clear();
                foreach (var campaignDTO in filteredCampaigns)
                {
                    cc.Campaigns.Add(campaignDTO);
                }
            }

            tvd.collection = dataToFilter;
            _tvUsers.ItemsSource = tvd.collection;

        }

        #region Events

        public event EventHandler<ClientContextMenuEventArgs> ClientContextMenuEvent;

        private void UsersOfClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            ClientCampaignsList clientCampaignsList = (ClientCampaignsList)textBlock.DataContext;
            ClientDTO client = clientCampaignsList.Client;

            ClientContextMenuEvent?.Invoke(this, new ClientContextMenuEventArgs(client, ClientContextMenuEventArgs.Options.UsersOfClient));
        }
        private void NewCampaignMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            ClientCampaignsList clientCampaignsList = (ClientCampaignsList)textBlock.DataContext;
            ClientDTO client = clientCampaignsList.Client;

            ClientContextMenuEvent?.Invoke(this, new ClientContextMenuEventArgs(client, ClientContextMenuEventArgs.Options.NewCampaign));
        }
        private void RenameClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            ClientCampaignsList clientCampaignsList = (ClientCampaignsList)textBlock.DataContext;
            ClientDTO client = clientCampaignsList.Client;

            ClientContextMenuEvent?.Invoke(this, new ClientContextMenuEventArgs(client, ClientContextMenuEventArgs.Options.RenameClient));
        }
        private void DeleteClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            ClientCampaignsList clientCampaignsList = (ClientCampaignsList)textBlock.DataContext;
            ClientDTO client = clientCampaignsList.Client;

            ClientContextMenuEvent?.Invoke(this, new ClientContextMenuEventArgs(client, ClientContextMenuEventArgs.Options.DeleteClient));
        }

        public event EventHandler<UserContextMenuEventArgs> UserContextMenuEvent;

        private void NewClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UserContextMenuEvent?.Invoke(this, new UserContextMenuEventArgs(UserContextMenuEventArgs.Options.NewClient));
        }


        public event EventHandler<CampaignContextMenuEventArgs> CampaignContextMenuEvent;

        private void EditCampaignMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            CampaignDTO campaign = (CampaignDTO)textBlock.DataContext;

            CampaignContextMenuEvent?.Invoke(this, new CampaignContextMenuEventArgs(campaign, CampaignContextMenuEventArgs.Options.EditCampaign));
        }
        private void _tvUsers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            object treeViewItem = _tvUsers.SelectedItem;

            // If clicked object is not campaign, then ignore double click. If it is fire event
            try
            {
                var campaign = (CampaignDTO)treeViewItem;
                if (campaign != null)
                {
                    CampaignContextMenuEvent?.Invoke(this, new CampaignContextMenuEventArgs(campaign, CampaignContextMenuEventArgs.Options.EditCampaign));
                }
            }
            catch
            {
                return;
            }

        }
        private void RenameCampaignMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            CampaignDTO campaign = (CampaignDTO)textBlock.DataContext;

            CampaignContextMenuEvent?.Invoke(this, new CampaignContextMenuEventArgs(campaign, CampaignContextMenuEventArgs.Options.RenameCampaign));
        }
        private void DeleteCampaignMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            CampaignDTO campaign = (CampaignDTO)textBlock.DataContext;

            CampaignContextMenuEvent?.Invoke(this, new CampaignContextMenuEventArgs(campaign, CampaignContextMenuEventArgs.Options.DeleteCampaign));
        }

        private void DuplicateCampaignMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TextBlock textBlock = (TextBlock)contextMenu.PlacementTarget;
            CampaignDTO campaign = (CampaignDTO)textBlock.DataContext;

            CampaignContextMenuEvent?.Invoke(this, new CampaignContextMenuEventArgs(campaign, CampaignContextMenuEventArgs.Options.DuplicateCampaign));
        }

        #endregion

        public void AddClient(ClientDTO client)
        {
            var clientCampaigns = new ClientCampaignsList { Client = client };
            clientCampaignsLists.Insert(0, clientCampaigns);
            FilterData();
        }

        public void RenameClient(ClientDTO client)
        {
            for (int i = 0; i < clientCampaignsLists.Count; i++)
            {
                var ccList = clientCampaignsLists[i];
                if (ccList.Client.clid == client.clid)
                {
                    clientCampaignsLists[i].Client = client;
                    break;
                }
            }

            FilterData();
        }

        public void RemoveClient(ClientDTO client)
        {
            for (int i=0; i<clientCampaignsLists.Count; i++)
            {
                var ccList = clientCampaignsLists[i];
                if (ccList.Client.clid == client.clid)
                {
                    clientCampaignsLists.RemoveAt(i);
                    break;
                }
            }

            FilterData();
        }

        public async Task AddCampaign(CampaignDTO campaign)
        {
            var client = await _clientController.GetClientById(campaign.clid);

            clientCampaignsLists.First(cc => cc.Client.clid == client.clid).Campaigns.Insert(0, campaign);
            FilterData();
        }

        public async Task RenameCampaign(CampaignDTO campaign)
        {
            var client = await _clientController.GetClientById(campaign.clid);

            for (int i = 0; i < clientCampaignsLists.Count; i++)
            {
                var ccList = clientCampaignsLists[i];
                if (ccList.Client.clid == client.clid)
                {
                    for (int j=0; j<ccList.Campaigns.Count(); j++)
                    {
                        if (ccList.Campaigns[j].cmpid == campaign.cmpid)
                        {
                            clientCampaignsLists[i].Campaigns[j] = campaign;
                            break;
                        }
                    }
                }
            }


            FilterData();
        }

        public async Task RemoveCampaign(CampaignDTO campaign)
        {
            var client = await _clientController.GetClientById(campaign.clid);

            for (int i = 0; i < clientCampaignsLists.Count; i++)
            {
                var ccList = clientCampaignsLists[i];
                if (ccList.Client.clid == client.clid)
                {
                    for (int j = 0; j < ccList.Campaigns.Count(); j++)
                    {
                        if (ccList.Campaigns[j].cmpid == campaign.cmpid)
                        {
                            clientCampaignsLists[i].Campaigns.RemoveAt(j);
                            break;
                        }
                    }
                }
            }

            FilterData();
        }

    }
}