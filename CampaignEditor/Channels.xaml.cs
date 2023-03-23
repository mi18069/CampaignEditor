using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ActivityDTO;
using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ChannelGroupDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.PricelistDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class Channels : Window
    {
        private ClientDTO _client;
        private CampaignDTO _campaign;

        private ChannelController _channelController;
        private PricelistController _pricelistController;
        private PricelistChannelsController _pricelistChannelsController;
        private ActivityController _activityController;
        private ChannelCmpController _channelCmpController;

        private readonly IAbstractFactory<PriceList> _factoryPriceList;
        private readonly IAbstractFactory<GroupChannels> _factoryGroupChannels;

        private List<ChannelDTO> _allChannelList;
        private ObservableCollection<ChannelDTO> _channelList;
        // This list will serve to get all Pricelists for client
        private List<PricelistDTO> _allPricelistsList;
        private ObservableCollection<PricelistDTO> _pricelistList;
        private ObservableCollection<ActivityDTO> _activityList;
        private ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _selected = 
                                new ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>();

        private List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _selectedChannels =
                                new List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>();

        private List<PricelistDTO> _pricelistsToBeDeleted = new List<PricelistDTO>();

        public bool channelsModified = false;
        public bool canEdit = false;
        private bool onlyActive = false; // For chbActive
        private bool changePricelist = true;
        
        #region Getters and Setters for lists

        private List<ChannelDTO> AllChannelList
        {
            get { return _allChannelList; }
            set { _allChannelList = value; }
        }

        private ObservableCollection<ChannelDTO> ChannelList
        {
            get { return _channelList;  }
            set { _channelList = value; }
        }
        private ObservableCollection<PricelistDTO> PricelistList
        {
            get { return _pricelistList;  }
            set { _pricelistList = value; }
        }
        private List<PricelistDTO> PricelistsToBeDeleted
        {
            get { return _pricelistsToBeDeleted; }
            set { _pricelistsToBeDeleted = value; }
        }
        private List<PricelistDTO> AllPricelistsList
        {
            get { return _allPricelistsList; }
            set { _allPricelistsList = value; }
        }
        private ObservableCollection<ActivityDTO> ActivityList
        {
            get { return _activityList;  }
            set { _activityList = value; }
        }
        private ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> Selected
        {
            get { return _selected;  }
            set { _selected = value; }
        }

        public List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> SelectedChannels
        {
            get { return _selectedChannels; }
            set { _selectedChannels = value; }
        }
        #endregion

        public Channels(IChannelRepository channelRepository, IPricelistRepository pricelistRepository,
            IPricelistChannelsRepository pricelistChannelsRepository, IActivityRepository activityRepository,
            IAbstractFactory<PriceList> factoryPriceList, IChannelCmpRepository channelCmpRepository,
            IAbstractFactory<GroupChannels> factoryGroupChannels)
        {
            this.DataContext = this;
            _channelController = new ChannelController(channelRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _activityController = new ActivityController(activityRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);

            _factoryPriceList = factoryPriceList;
            _factoryGroupChannels = factoryGroupChannels;

            InitializeComponent();
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign,
            List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> channelList = null)
        {
            this._client = client;
            this._campaign = campaign;
            SelectedChannels = channelList;
            await FillLists();
            await FillChannelGroups();
        }

        private async Task FillLists()
        {
            Selected.Clear();

            if (SelectedChannels == null)
            {

                // Initializing and sorting lists, so it don't have to be sorted later
                _channelList = new ObservableCollection<ChannelDTO>((await _channelController.GetAllChannels()).OrderBy(c => c.chname));
                _allPricelistsList = (List<PricelistDTO>)(await _pricelistController.GetAllClientPricelists(_client.clid));
                _pricelistList = new ObservableCollection<PricelistDTO>((AllPricelistsList).OrderByDescending(p => p.valfrom));
                _activityList = new ObservableCollection<ActivityDTO>((await _activityController.GetAllActivities()).OrderBy(a => a.act));

                // Binding to ListViews
                lvChannels.ItemsSource = ChannelList;
                lvPricelists.ItemsSource = PricelistList;
                lvActivities.ItemsSource = ActivityList;

                AllChannelList = ChannelList.ToList();

                var selectedChannels = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
                if (selectedChannels != null)
                {
                    foreach (var selected in selectedChannels)
                    {
                        ChannelDTO channel = await _channelController.GetChannelById(selected.chid);
                        PricelistDTO pricelist = await _pricelistController.GetPricelistById(selected.plid);
                        ActivityDTO activity = await _activityController.GetActivityById(selected.actid);

                        MoveToSelected(channel, pricelist, activity);
                    }
                    SelectedChannels = Selected.ToList();
                }
            }
            else
            {   
                foreach (var selected in SelectedChannels)
                {
                    // If we want to move some channel from channelList to Seleccted,
                    // first we must find that object in ChannelList, if not,
                    // just take from selectedList
                    ChannelDTO selectedChannel = null;
                    foreach (ChannelDTO channel in ChannelList)
                    {
                        if (channel.chname.Trim() == selected.Item1.chname.Trim())
                        {
                            selectedChannel = channel;
                        }

                    }
                    if (selectedChannel == null)
                        selectedChannel = selected.Item1;
                    MoveToSelected(selectedChannel, selected.Item2, selected.Item3);
                }
                channelsModified = false;
            }

            dgSelected.ItemsSource = Selected;
        }

        #region ToSelected and FromSelected 

        private void MoveToSelected(ChannelDTO channel, PricelistDTO pricelist, ActivityDTO activity)
        {
            var list = Selected.Select(t => t.Item1).ToList(); // making a list to pass to function FindIndex
            int index = FindIndex(list, channel); // Finding the right index, to keep the list sorted
            Selected.Insert(index, Tuple.Create(channel, pricelist, activity)!);
            ChannelList.Remove(channel);
            AllChannelList.Remove(channel);
            channelsModified = true;
        }
        private void MoveFromSelected(Tuple<ChannelDTO, PricelistDTO, ActivityDTO> tuple)
        {
            var channel = tuple.Item1;
            Selected.Remove(tuple);
            int index = FindIndex(ChannelList, channel);
            ChannelList.Insert(index, channel);
            AllChannelList.Insert(index, channel);
            channelsModified = true;
        }
        private void btnToSelected_Click(object sender, RoutedEventArgs e)
        {
            // At least one item from every listView needs to be selected to execute
            if (lvChannels.SelectedItems.Count > 0 &&
                lvPricelists.SelectedItems.Count > 0 &&
                lvActivities.SelectedItems.Count > 0) 
            {
                int n = lvChannels.SelectedItems.Count;
                var channels = lvChannels.SelectedItems;

                PricelistDTO pricelist = lvPricelists.SelectedItem as PricelistDTO;
                ActivityDTO activity = lvActivities.SelectedItem as ActivityDTO;
                for (int i=0; i<n; i++)
                {
                    ChannelDTO channel = channels[0] as ChannelDTO; // 0 because we need n iterations, and in each we remove one item
                    changePricelist = false;
                    MoveToSelected(channel, pricelist, activity);
                    changePricelist = true;
                }
            }
        }

        private void btnFromSelected_Click(object sender, RoutedEventArgs e)
        {
            int n = dgSelected.SelectedItems.Count;
            if (n > 0)
            {
                var selectedItems = dgSelected.SelectedItems;
                for (int i=0; i<n; i++)
                {
                    // 0 because we need n iterations, and in each we remove one item
                    var tuple = selectedItems[0] as Tuple<ChannelDTO, PricelistDTO, ActivityDTO>;
                    MoveFromSelected(tuple);
                }
            }
        }

        // Used to find the correct position of element in collection
        private int FindIndex(IEnumerable<ChannelDTO> ChannelList, ChannelDTO channelDTO)
        {
            int index = ChannelList.Count();
            for (int i = 0; i < ChannelList.Count(); i++)
            {
                if (channelDTO.chname.CompareTo(ChannelList.ElementAt(i).chname) < 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        // On row double click, moves clicked item to Channel list
        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            var tuple = row.Item as Tuple<ChannelDTO, PricelistDTO, ActivityDTO>;
            MoveFromSelected(tuple);
        }

        #endregion

        #region Selection Changed

        private async void lvChannels_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!changePricelist && lvChannels.SelectedItems.Count > 0)
                return;
            PricelistDTO lastSelected = null;
            if (lvPricelists.SelectedItems.Count > 0)
            {
                lastSelected = lvPricelists.SelectedItem as PricelistDTO;
            }
            PricelistList.Clear();
            // Making lists of integers for faster transition of elements
            List<int> plids = new List<int>();
            List<int> chids = new List<int>();

            // taking plids from AllPricelistsList because this elements already shows 
            // only pricelists available for current client
            foreach (var pricelist in AllPricelistsList)
            {
                // for adding only id-s of pricelists withing Date range
                if (onlyActive)
                {
                    if (TimeFormat.YMDStringToDateTime(pricelist.valfrom.ToString().Trim()).AddDays(-1) > DateTime.Now ||
                        TimeFormat.YMDStringToDateTime(pricelist.valto.ToString().Trim()).AddDays(1) < DateTime.Now)
                    {
                        continue;
                    }
                    else
                    {
                        plids.Add(pricelist.plid);
                    }
                }
                else
                {
                    plids.Add(pricelist.plid);
                }
            }
            foreach (ChannelDTO chid in lvChannels.SelectedItems)
            {
                chids.Add(chid.chid);
            }

            // Getting the right pricelistChannels ids which should be displayed
            if (plids.Count == 0)
                return;

            var plIds = await _pricelistChannelsController.GetIntersectedPlIds(plids, chids);

            // Clearing and filling with the available pricelists for selected channels
            // == 0 because when more channels are selected, only one should modify lvPricelists
            int selectedIndex = 0;

            foreach (int plid in plIds)
            {
                PricelistDTO pricelist = await _pricelistController.GetPricelistById(plid);
                PricelistList.Add(pricelist);
                if (lastSelected != null && lastSelected.plid == pricelist.plid)
                {
                    lvPricelists.SelectedIndex = selectedIndex;
                }
                selectedIndex++;
            }
        }

            // In order to select ListView on mouse right click
        private void lvPricelists_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListViewItem listViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (listViewItem != null)
            {
                listViewItem.Focus();
                listViewItem.IsSelected = true;
                lvPricelists.ContextMenu = lvPricelists.Resources["PricelistContext"] as System.Windows.Controls.ContextMenu;
                e.Handled = true;
            }
        }

        static ListViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ListViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as ListViewItem;
        }


        #endregion

        #region Pricelist
        private async void btnEditPricelist_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryPriceList.Create();
            if (lvPricelists.SelectedItems.Count > 0)
                await f.Initialize(_client, lvPricelists.SelectedItem as PricelistDTO);
            else
                await f.Initialize(_client);
            f.ShowDialog();
            if (f.pricelistChanged)
            {
                _allPricelistsList = (List<PricelistDTO>)(await _pricelistController.GetAllClientPricelists(_client.clid)); 
                lvChannels_SelectionChanged(lvChannels, null);
            }
        }
        private async void btnNewPricelist_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryPriceList.Create();
            await f.Initialize(_client);
            f.ShowDialog();
            if (f.pricelistChanged)
            {
                _allPricelistsList = (List<PricelistDTO>)(await _pricelistController.GetAllClientPricelists(_client.clid)); 
                lvChannels_SelectionChanged(lvChannels, null);
            }
        }
        private void chbActivePricelists_Checked(object sender, RoutedEventArgs e)
        {
            onlyActive = true;
            lvChannels_SelectionChanged(lvChannels, null);
        }

        private void chbActivePricelists_Unchecked(object sender, RoutedEventArgs e)
        {
            onlyActive = false;
            lvChannels_SelectionChanged(lvChannels, null);
        }
        #endregion

        #region Channel Groups

        private async Task FillChannelGroups()
        {
            var f = _factoryGroupChannels.Create();
            await f.Initialize(_client, _campaign);
            lbChannelGroups.ItemsSource = f.ChannelGroupList;
        }
        private async void btnEditChannelGroups_Click(object sender, RoutedEventArgs e)
        {
            var f = _factoryGroupChannels.Create();
            await f.Initialize(_client, _campaign);
            f.ShowDialog();
        }

        private async void lbChannelGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbChannelGroups.SelectedItems.Count == 0)
            {
                ChannelList.Clear();
                foreach (ChannelDTO channel in AllChannelList)
                {
                    ChannelList.Add(channel);
                }
            }
            else if (e.AddedItems.Count > 0)
            {
                // Assuring that the only one element is selected
                ListBox listBox = sender as ListBox;
                var valid = e.AddedItems[0];
                foreach (var item in new ArrayList(listBox.SelectedItems))
                {
                    if (item != valid)
                    {
                        listBox.SelectedItems.Remove(item);
                    }
                }

                //Implementation of logic
                ChannelGroupDTO channelGroup = lbChannelGroups.SelectedItems[0] as ChannelGroupDTO;
                int chgrid = channelGroup.chgrid;
                ChannelList.Clear();
                var f = _factoryGroupChannels.Create();
                await f.Initialize(_client, _campaign);
                var tuples = f.Assigned.Where(t => t.Item2.chgrid == chgrid);
                for (int i = 0; i < AllChannelList.Count; i++)
                {
                    ChannelDTO channel = AllChannelList[i] as ChannelDTO;
                    if (tuples.Any(t => t.Item1.chid == channel.chid))
                    {
                        ChannelList.Add(channel);
                        continue;
                    }
                }
                lvChannels.SelectAll();
            }

        }

        #endregion


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (channelsModified)
                SelectedChannels = Selected.ToList();
            this.Hide();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            channelsModified = false;
            while (PricelistsToBeDeleted.Count > 0)
            {
                PricelistList.Add(PricelistsToBeDeleted[0]);
                PricelistsToBeDeleted.RemoveAt(0);
            }
            this.Hide();
        }

        public async Task UpdateDatabase(List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> channelList)
        {
            await _channelCmpController.DeleteChannelCmpByCmpid(_campaign.cmpid);
            foreach (var channel in SelectedChannels)
            {
                CreateChannelCmpDTO channelCmp = new CreateChannelCmpDTO
                    (_campaign.cmpid, channel.Item1.chid, channel.Item2.plid, channel.Item3.actid, -1, -1);
                await _channelCmpController.CreateChannelCmp(channelCmp);
            }
            foreach (PricelistDTO pricelist in PricelistsToBeDeleted)
            {
                await _pricelistController.DeletePricelistById(pricelist.plid);
            }
        }

        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        #region Context Menu
        private async void miDeletePricelist_Click(object sender, RoutedEventArgs e)
        {
            PricelistDTO pricelist = lvPricelists.SelectedItem as PricelistDTO;
            //PricelistDTO pricelist = sender as PricelistDTO;
            PricelistsToBeDeleted.Add(pricelist);
            PricelistList.Remove(pricelist);
        }

        #endregion
    }
}
