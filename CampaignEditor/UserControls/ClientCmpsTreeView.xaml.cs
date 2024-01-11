using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ClientCmpsTreeView.xaml
    /// </summary>
    public class TreeViewData
    {
        public int UserLevel { get; set; } = MainWindow.user.usrlevel;

        public string ItemName { get; set; } = new string("AAA");

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
            tvd = new TreeViewData();
            // Default constructor for XAML
            InitializeComponent();

        }

        public async Task Initialize(UserDTO userDTO = null)
        {

            clientCampaignsLists.Clear();
            var clids = new List<int>();
            if (userDTO == null || userDTO.usrlevel == 0)
            {
                clids = (await _userClientsController.GetAllUserClients()).Select(c => c.cliid).ToList();
                clids = clids.Distinct().ToList();
            }
            else
                clids = (await _userClientsController.GetAllUserClientsByUserId(userDTO.usrid)).Select(c => c.cliid).ToList();

            List<ClientCampaignsList> ccList = new List<ClientCampaignsList>();

            foreach (var clid in clids)
            {
                var client = await _clientController.GetClientById(clid);
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

            bool applySinceFilter = false;
            bool applyActiveFilter = false;
            bool applyNotStartedFilter = false;
            bool applyFinishedFilter = false;

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

            ApplyFilters(currentFilteredData, applySinceFilter, applyActiveFilter, applyNotStartedFilter, applyFinishedFilter);
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

        private void ApplyFilters(ObservableCollection<ClientCampaignsList> dataToFilter, bool applySinceFilter, bool applyActiveFilter, bool applyNotStartedFilter, bool applyFinishedFilter)
        {

            foreach (var cc in dataToFilter)
            {
                var filteredCampaigns = cc.Campaigns.ToList();
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
    }
}